﻿using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using ProtoBuf;
using System.Collections.Concurrent;
using GameMachine;
using ClientMessage = GameMachine.Messages.ClientMessage;
using Entity = GameMachine.Messages.Entity;
using PlayerLogout = GameMachine.Messages.PlayerLogout;


namespace GameMachine
{
    public class Client
    {
        private IPEndPoint udp_ep;
        private UdpClient udpClient;
        private int port;
        private string host;
        private ClientMessage clientMessage;
        private string playerId;
        private string authtoken;
        private bool measure = false;
        public static ConcurrentQueue<Entity> entityQueue = new ConcurrentQueue<Entity>();


        public Client(string _playerId, string _authtoken, bool measure=false)
        {
            playerId = _playerId;
            authtoken = _authtoken;
            host = Config.udpHost;
            port = Config.udpPort;
            clientMessage = CreateClientMessage();
            this.measure = measure;

            Start();
        }

        public ClientMessage CreateClientMessage()
        {
            GameMachine.Messages.Player player = new GameMachine.Messages.Player();
            player.id = playerId;
            player.authtoken = authtoken;
            ClientMessage clientMessage = new ClientMessage();
            clientMessage.player = player;
            return clientMessage;
        }

        public void Stop()
        {
            PlayerLogout logout = new PlayerLogout();
            logout.authtoken = authtoken;
            logout.playerId = playerId;
            ClientMessage message = CreateClientMessage();
            message.playerLogout = logout;
            byte[] bytes = Serialize(message);
            udpClient.Send(bytes, bytes.Length, host, port);
            udpClient.Close();
        }

        private void Start()
        {
            udp_ep = new IPEndPoint(IPAddress.Any, 0);
            udpClient = new UdpClient(udp_ep);
            receiveData();

            ClientMessage message = CreateClientMessage();
            message.playerConnect = new GameMachine.Messages.PlayerConnect();
            Send(Serialize(message));
        }
				
        private void SendCallback(IAsyncResult ar)
        {
            UdpClient u = (UdpClient)ar.AsyncState;
            u.EndSend(ar);
        }

        public void SendEntity(Entity entity)
        {
            clientMessage.entity.Add(entity);
            Send(Serialize(clientMessage));
            clientMessage.entity.Clear();
        }

        public void Send(byte[] bytes)
        {
            udpClient.BeginSend(bytes, bytes.Length, host, port, new AsyncCallback(SendCallback), udpClient);
        }
		
        private void dataReady(IAsyncResult ar)
        {
            byte[] bytes = udpClient.EndReceive(ar, ref udp_ep);
            ClientMessage message = Deserialize(bytes);
            foreach (Entity entity in message.entity)
            {
                if (measure)
                {
                    Client.entityQueue.Enqueue(entity); 
                } else
                {
                    ClientMessageQueue.entityQueue.Enqueue(entity); 
                }

            }
            receiveData();
        }
		
        private void receiveData()
        {
            udpClient.BeginReceive(new AsyncCallback(dataReady), udp_ep);
        }

        private ClientMessage Deserialize(byte[] bytes)
        {
            MemoryStream stream = new MemoryStream(bytes);
            return Serializer.Deserialize<ClientMessage>(stream);
        }
		
        private byte[] Serialize(ClientMessage message)
        {
            MemoryStream stream = new MemoryStream();
            Serializer.Serialize(stream, message);
            return stream.ToArray();
        }
    }
}