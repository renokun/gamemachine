require 'yaml'
require 'rjack-logback'
require 'java'
require 'benchmark'
require 'socket'
require 'settingslogic'
require 'spoon'
require 'consistent_hashing'


jars = Dir[File.join(File.dirname(__FILE__), '../java_lib', '*.jar')]
jars.each {|jar| require jar}

java_import 'akka.actor.UntypedActorFactory'
java_import 'akka.serialization.SerializationExtension'
java_import 'akka.serialization.Serializer'
java_import 'java.util.concurrent.ConcurrentHashMap'
java_import 'com.game_machine.core.GameMachineLoader'
java_import 'com.game_machine.core.AskProxy'
java_import 'com.game_machine.core.ActorUtil'
java_import 'akka.actor.ActorRef'
java_import 'akka.actor.ActorSystem'
java_import 'akka.actor.Props'
java_import 'akka.actor.UntypedActor'
java_import 'com.game_machine.core.GatewayMessage'
java_import 'com.game_machine.core.net.server.UdpServerHandler'
java_import 'com.game_machine.core.net.server.UdpServer'
java_import 'com.game_machine.core.net.server.UdtServer'
java_import 'com.game_machine.core.NetMessage'
java_import 'java.net.InetSocketAddress'
java_import 'com.barchart.udt.SocketUDT'
java_import 'com.barchart.udt.TypeUDT'
java_import 'com.game_machine.entity_system.generated.ObjectdbPut'
java_import 'com.game_machine.entity_system.generated.ObjectdbGet'
java_import 'com.game_machine.entity_system.generated.ObjectdbUpdate'
java_import 'com.game_machine.entity_system.generated.Entity'
java_import 'com.game_machine.entity_system.generated.EntityList'
java_import 'com.game_machine.entity_system.generated.GameCommand'
java_import 'com.game_machine.entity_system.generated.PlayerConnection'
java_import 'com.game_machine.entity_system.generated.ChatMessage'
java_import 'com.game_machine.entity_system.generated.Player'
java_import 'com.game_machine.entity_system.generated.PlayersAroundMe'
java_import 'com.game_machine.entity_system.generated.ClientId'
java_import 'akka.routing.RoundRobinRouter'
java_import 'com.typesafe.config.Config'
java_import 'com.typesafe.config.ConfigFactory'
#java_import 'com.game_machine.core.persistence.ObjectDb'
java_import 'com.game_machine.core.persistence.Query'
java_import 'com.game_machine.core.persistence.QueryCallable'
java_import 'java.util.concurrent.atomic.AtomicInteger'
java_import 'akka.testkit.TestActorRef'
java_import 'com.google.common.hash.HashFunction'
java_import 'com.google.common.hash.Hashing'
java_import 'akka.actor.Terminated'
java_import 'scala.concurrent.duration.Duration'
java_import 'java.util.concurrent.TimeUnit'
java_import 'akka.pattern.Patterns'
java_import 'scala.concurrent.Await'
java_import 'scala.concurrent.Future'
java_import 'akka.pattern.AskableActorSelection'
java_import 'akka.actor.DeadLetter'
java_import 'java.io.Serializable'




unless ENV['GAME_ENV']
  ENV['GAME_ENV'] = 'development'
end

module GameMachine
  def self.env
    ENV['GAME_ENV']
  end

  def self.configure_logging

    @@logger = RJack::SLF4J[ 'game_machine' ]

    RJack::Logback.configure do
      console = RJack::Logback::ConsoleAppender.new do |a|

      end
      RJack::Logback.root.add_appender( console )
      RJack::Logback.root.level = RJack::Logback::INFO
    end
  end

  def self.logger
    @@logger
  end
end

require_relative 'game_machine/settings'
require_relative 'game_machine/actor_factory'
require_relative 'game_machine/hashring'
require_relative 'game_machine/gateway'
require_relative 'game_machine/game_actor'
require_relative 'game_machine/game_actor_ref'
require_relative 'game_machine/server'
require_relative 'game_machine/command_router'
require_relative 'game_machine/client'
require_relative 'game_machine/local_echo'
require_relative 'game_machine/connection_manager'
require_relative 'game_machine/actor_builder'
require_relative 'game_machine/object_db'
require_relative 'game_machine/system_monitor'

app_classes = Dir[File.join(File.dirname(__FILE__), '../app', '*.rb')]
app_classes.each {|app_class| require app_class}
puts "GAME_ENV is #{GameMachine.env}"
