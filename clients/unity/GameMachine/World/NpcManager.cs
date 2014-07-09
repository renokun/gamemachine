using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using  System.Text.RegularExpressions;
using GameMachine.World;
using GameMachine.Core;
using TrackData = GameMachine.Messages.TrackData;

namespace GameMachine.World
{
	public class NpcManager : MonoBehaviour
	{

		public static Dictionary<string, Npc> npcs = new Dictionary<string, Npc> ();
		public static int npcCount = 0;
		public string playerType = "OtherPlayer";

		NpcManager ()
		{
		}

		void Start ()
		{
			InvokeRepeating ("ExpireNpcs", 2, 1.5F);
		}

		// Remove objects from world that we haven't seen an update for in a while.  This is fairly strict
		// right now until we put in the ability to 'warp' objects that are really behind to their proper
		// location
		public void ExpireNpcs ()
		{
			var itemsToRemove = npcs.Where (f => (Time.time - f.Value.lastUpdate) > 0.9f).ToArray ();
			foreach (var item in itemsToRemove) {
				//Logger.Debug("Removing " + item.Key);
				Destroy (item.Value.gameObject);
				npcs.Remove (item.Key);
			}
                
		}

		public static void DestroyNpc (string name)
		{
			Destroy (npcs [name]);
			npcs.Remove (name);
		}

		public void UpdateFromTracking (List<TrackingUpdate> updates)
		{
			npcCount = npcs.Count;
			GameObject npcObject;

			foreach (TrackingUpdate update in updates) {

				if (npcs.ContainsKey (update.entityId)) {
					// Update existing npc
					Npc npc = npcs [update.entityId];

					if (update.trackData.direction == null) {
						npc.UpdateTarget (new Vector3 (update.x, update.z, update.y));
					} else {
						TrackData extra = update.trackData;
						Vector3 target = new Vector3 (update.x, update.z, update.y);
						Vector3 direction = new Vector3 (extra.direction.x, extra.direction.y, extra.direction.z);
						npc.UpdatePlayer (target, direction, extra.speed);
					}

				} else {
					if (update.entityId != User.Instance.username) {
						// New npc
						string npcType = "OtherPlayer";

						if (update.entityId.StartsWith ("male")) {
							npcType = "MaleNpc";
						} else if (update.entityId.StartsWith ("viking")) {
							npcType = "Npc";
						} else if (update.entityId.StartsWith ("golem")) {
							npcType = "GolemNpc";
						} else if (update.entityId.StartsWith ("worm")) {
							npcType = "WormNpc";
						}

						npcObject = (GameObject)Instantiate (Resources.Load (npcType));
						npcObject.name = update.entityId;
						Npc npc = npcObject.GetComponent<Npc> ();
						npc.name = update.entityId;
						if (npcType == "WormNpc") {
							npc.isAggressive = true;
						}
						npcs.Add (update.entityId, npc);
						npc.transform.position = npc.SpawnLocation (new Vector3 (update.x, 0f, update.y));

						if (npcType == playerType) {
							npc.isPlayer = true;
						}

						//npc.transform.position = new Vector3(update.x, 40f, update.y);
					}
				}
			}


		}
	}
}