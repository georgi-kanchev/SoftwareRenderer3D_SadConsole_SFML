using NetCoreServer;
using NetFwTypeLib;
using SadConsole;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TcpClient = NetCoreServer.TcpClient;
using SadRogue.Primitives;

namespace SMPL
{
	public static class Multiplayer
	{
		internal static WindowPlus window;

		public enum ChoiceAction { ServerStart, ServerStop, ClientConnect, ClientDisconnect, ClientTakenUID }
		public struct Message
		{
			internal enum ChoiceType
			{
				None, Connection, ChangeID, ClientConnected, ClientDisconnected, ClientOnline,
				ClientToAll, ClientToClient, ClientToServer, ServerToAll, ServerToClient,
				ClientToAllAndServer
			}
			public enum ChoiceToward { Server, Client, AllClients, ServerAndAllClients }
			internal const string SEP = ";$*#", COMP_SEP = "=!@,", TEMP_SEP = ")`.&";

			public class ParamsReceive : Event.Parameters { public Message Message { get; set; } }
			public const string OnReceive = nameof(Multiplayer) + nameof(Message) + nameof(OnReceive);

			public string Content { get; set; }
			public string Tag { get; set; }
			public string ReceiverUniqueID { get; set; }
			public string SenderUniqueID { get; internal set; }
			public ChoiceToward Receivers { get; set; }
			internal ChoiceType type;

			public Message(ChoiceToward receivers, string tag, string content, bool unreliable = false, string receiverClientUniqueID = null)
			{
				Content = content;
				Tag = tag;
				ReceiverUniqueID = receiverClientUniqueID;
				SenderUniqueID = ClientUniqueID;
				Receivers = receivers;
				type = receivers switch
				{
					ChoiceToward.Server => ClientIsConnected ? ChoiceType.ClientToServer : ChoiceType.None,
					ChoiceToward.Client => ClientIsConnected ? ChoiceType.ClientToClient : ChoiceType.ServerToClient,
					ChoiceToward.AllClients => ClientIsConnected ? ChoiceType.ClientToAll : ChoiceType.ServerToAll,
					ChoiceToward.ServerAndAllClients => ClientIsConnected ? ChoiceType.ClientToAllAndServer : ChoiceType.ServerToAll,
					_ => ChoiceType.None,
				};
			}
			public override string ToString()
			{
				var send = SenderUniqueID == null || SenderUniqueID == "" ? "from the Server" : $"from Client '{SenderUniqueID}'";
				var rec = Receivers == ChoiceToward.Client ? $"to Client '{ReceiverUniqueID}'" : $"to {Receivers}";
				return
					$"Multiplayer Message {send} {rec}" +
					$"Tag: {Tag}" +
					$"Content: {Content}";
			}
		}
		internal class Session : TcpSession
		{
			public Session(TcpServer server) : base(server) { }

			protected override void OnConnected() { }
			protected override void OnDisconnected()
			{
				var disconnectedClient = clientRealIDs[Id];
				clientRealIDs.Remove(Id);
				clientIDs.Remove(disconnectedClient);
				var msg = new Message(Message.ChoiceToward.AllClients, null, disconnectedClient)
				{ type = Message.ChoiceType.ClientDisconnected };
				SendMessage(msg);

				Log($"Client '{disconnectedClient}' disconnected. {ConnectedClients()}");
				OnClientDisconnect?.Invoke(disconnectedClient);
			}
			protected override void OnReceived(byte[] buffer, long offset, long size)
			{
				var rawMessages = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
				DecodeMessages(Id, rawMessages);
			}
			protected override void OnError(SocketError error) => Log($"[!] {error}");
		}
		internal class Server : TcpServer
		{
			public Server(IPAddress address, int port) : base(address, port) { }
			protected override TcpSession CreateSession() { return new Session(this); }
			protected override void OnError(SocketError error)
			{
				ServerIsRunning = false;
				Log($"[!] {error}");
				OnServerStop?.Invoke();
			}
		}
		internal class Client : TcpClient
		{
			private bool stop;

			public Client(string address, int port) : base(address, port) { }

			public void DisconnectAndStop()
			{
				stop = true;
				DisconnectAsync();
				while (IsConnected)
					Thread.Yield();
			}
			protected override void OnConnected()
			{
				ClientIsConnected = true;
				clientIDs.Add(ClientUniqueID);
				var ip = client.Socket.RemoteEndPoint.ToString().Split(':')[0];
				if (ServerIsRunning == false)
					Log($"Connected as '{ClientUniqueID}' to LAN Server[{ip}].");

				OnClientConnect?.Invoke(ClientUniqueID);

				var msg = new Message(Message.ChoiceToward.Server, null, Id.ToString()) { type = Message.ChoiceType.Connection };
				client.SendAsync(MessageToString(msg));
			}
			protected override void OnDisconnected()
			{
				if (ClientIsConnected)
				{
					ClientIsConnected = false;
					Log("Disconnected from the LAN Server.");
					clientIDs.Clear();
					OnClientDisconnect?.Invoke(ClientUniqueID);
					if (stop == true)
						return;
				}

				Thread.Sleep(1000);

				Log("Trying to reconnect...");
				ConnectAsync();
			}
			protected override void OnReceived(byte[] buffer, long offset, long size)
			{
				var rawMessages = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
				DecodeMessages(Id, rawMessages);
			}
			protected override void OnError(SocketError error)
			{
				ClientIsConnected = false;
				Log($"[!] {error}");
			}
		}
		public class ParamsClient : Event.Parameters
		{
			public string UID { get; set; }
		}

		public static Action OnServerStart { get; set; }
		public static Action OnServerStop { get; set; }
		public static Action<string> OnClientConnect { get; set; }
		public static Action<string> OnClientDisconnect { get; set; }
		public static Action<string> OnClientUniqueIDTaken { get; set; }

		private static readonly Dictionary<Guid, string> clientRealIDs = new();
		private static readonly List<string> clientIDs = new();
		private static readonly int serverPort = 1234;
		internal static Server server;
		internal static Client client;

		public const string SameDeviceIP = "127.0.0.1";
		public static bool ClientIsConnected { get; private set; }
		public static bool ServerIsRunning { get; private set; }
		public static string ClientUniqueID { get; private set; }

		static Multiplayer()
		{
			window = new(80, 20, SadConsole.Input.Keys.Home) { Title = $" Multiplayer (clients online: {clientIDs.Count}) " };
			Game.Instance.FrameUpdate += OnUpdate;
		}

		public static void StartServer()
		{
			try
			{
				if (ServerIsRunning)
				{
					Log("[!] Server is already starting/started.");
					return;
				}
				if (ClientIsConnected)
				{
					Log("[!] Cannot start a Server while a Client.");
					return;
				}

				OpenPort();

				server = new Server(IPAddress.Any, serverPort);
				server.Start();
				ServerIsRunning = true;

				Log("Started a LAN Server.");
				Log("Clients can connect through those IPs if they are in the same network:");
				Log($"Same device: {SameDeviceIP}");

				var hostName = Dns.GetHostName();
				var hostEntry = Dns.GetHostEntry(hostName);
				for (int i = 0; i < hostEntry.AddressList.Length; i++)
				{
					if (hostEntry.AddressList[i].AddressFamily != AddressFamily.InterNetwork) continue;

					var ipParts = hostEntry.AddressList[i].ToString().Split('.');
					var isRouter = ipParts[0] == "192" && ipParts[1] == "168";
					var ipType = isRouter ? "Same router: " : "Same VPN: ";
					Log($"{ipType}{hostEntry.AddressList[i]}");
				}

				OnServerStart?.Invoke();
			}
			catch (Exception ex)
			{
				ServerIsRunning = false;
				if (ex.Message.Contains("Access is denied"))
					Log("[!] In order to start the Multiplayer LAN Server, run the game as an Administrator.");
				else
					Log($"[!] {ex.Message}");
				OnServerStop?.Invoke();
			}
		}
		public static void StopServer()
		{
			try
			{
				if (ServerIsRunning == false)
				{
					Log("[!] Server is not running.");
					return;
				}
				if (ClientIsConnected)
				{
					Log("[!] Cannot stop a server while a client.");
					return;
				}
				ServerIsRunning = false;
				server.Stop();
				Log("The LAN Server was stopped.");
				OnServerStop?.Invoke();
			}
			catch (Exception ex)
			{
				ServerIsRunning = false;
				Log($"[!] {ex.Message}");
				OnServerStop?.Invoke();
				return;
			}
		}
		public static void ConnectClient(string clientUniqueID, string serverIP)
		{
			if (ClientIsConnected)
			{
				Log("[!] Already connecting/connected.");
				return;
			}
			if (ServerIsRunning)
			{
				Log("[!] Cannot connect as Client while hosting a Server.");
				return;
			}
			try
			{
				client = new Client(serverIP, serverPort);
			}
			catch (Exception)
			{
				Log($"[!] The IP '{serverIP}' is invalid.");
				return;
			}

			client.ConnectAsync();
			ClientUniqueID = clientUniqueID;
			ClientIsConnected = true;
			Log($"Connecting to LAN Server[{serverIP}]...");
		}
		public static void DisconnectClinet()
		{
			if (ClientIsConnected == false)
			{
				Log($"[!] Cannot disconnect when not connected as Client.");
				return;
			}
			client.DisconnectAndStop();
		}
		public static void SendMessage(Message message)
		{
			if (MessageDisconnected())
				return;
			if (ServerIsRunning && message.Receivers == Message.ChoiceToward.Server)
				return;

			var msgStr = MessageToString(message);
			if (ClientIsConnected)
				client.SendAsync(msgStr);
			else server.Multicast(msgStr);
		}

		private static void OnUpdate(object sender, GameHost e)
		{
			if (Time.FrameCount % 100 != 0)
				return;

			window.Title = $" Multiplayer (clients online: {clientIDs.Count}) ";
		}
		private static string MessageToString(Message message)
		{
			return
				$"{Message.SEP}" +
				$"{(int)message.type}{Message.COMP_SEP}" +
				$"{message.SenderUniqueID}{Message.COMP_SEP}" +
				$"{message.ReceiverUniqueID}{Message.COMP_SEP}" +
				$"{(int)message.Receivers}{Message.COMP_SEP}" +
				$"{message.Tag}{Message.COMP_SEP}" +
				$"{message.Content}";
		}
		private static List<Message> StringToMessages(string message)
		{
			var result = new List<Message>();
			var split = message.Split(Message.SEP, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < split.Length; i++)
			{
				if (split[i].Length < 10)
					continue;
				var comps = split[i].Split(Message.COMP_SEP);
				result.Add(new Message()
				{
					type = (Message.ChoiceType)int.Parse(comps[0]),
					SenderUniqueID = comps[1],
					ReceiverUniqueID = comps[2],
					Receivers = (Message.ChoiceToward)int.Parse(comps[3]),
					Tag = comps[4],
					Content = comps[5],
				});
			}
			return result;
		}
		private static bool MessageDisconnected()
		{
			if (ClientIsConnected == false && ServerIsRunning == false)
			{
				Log("Cannot send a message while disconnected.");
				return true;
			}
			return false;
		}
		private static string ConnectedClients() => $"Connected clients: {clientIDs.Count}.";
		private static void DecodeMessages(Guid sessionID, string rawMessages)
		{
			var messages = StringToMessages(rawMessages);
			if (ServerIsRunning)
			{
				var messageBack = "";
				for (int i = 0; i < messages.Count; i++)
				{
					var msg = messages[i];
					switch (msg.type)
					{
						case Message.ChoiceType.Connection: // A client just connected and sent his ID & unique name
							{
								if (clientIDs.Contains(msg.SenderUniqueID)) // Is the unique name free?
								{
									msg.SenderUniqueID = ChangeID(msg.SenderUniqueID);
									// Send a message back with a free one toward the same ID so the client can recognize it's for him
									var freeUidMsg = new Message(
										Message.ChoiceToward.Client, null, msg.Content, receiverClientUniqueID: msg.SenderUniqueID)
									{ type = Message.ChoiceType.ChangeID };
									messageBack += MessageToString(freeUidMsg);

									string ChangeID(string ID)
									{
										var i = 0;
										while (true)
										{
											i++;
											if (clientIDs.Contains(ID + i) == false) break;
										}
										return $"{ID}{i}";
									}
								}
								clientRealIDs[sessionID] = msg.SenderUniqueID;
								clientIDs.Add(msg.SenderUniqueID);

								// Sticking another message to update the newcoming client about online clients
								var onlineMsg = new Message(Message.ChoiceToward.Client, null, null, receiverClientUniqueID: msg.SenderUniqueID)
								{ type = Message.ChoiceType.ClientOnline };
								for (int j = 0; j < clientIDs.Count; j++)
								{
									if (onlineMsg.Content == null)
									{
										onlineMsg.Content = clientIDs[j];
										continue;
									}
									onlineMsg.Content += $"{Message.TEMP_SEP}{clientIDs[j]}";
								}
								messageBack += MessageToString(onlineMsg);

								// Sticking a third message to update online clients about the newcomer.
								var newComMsg = new Message(Message.ChoiceToward.AllClients, null, msg.SenderUniqueID)
								{ type = Message.ChoiceType.ClientConnected };
								messageBack += MessageToString(newComMsg);
								Log($"Client '{msg.SenderUniqueID}' connected. {ConnectedClients()}");
								OnClientConnect?.Invoke(msg.SenderUniqueID);
								break;
							}
						case Message.ChoiceType.ClientToAll: // A client wants to send a message to everyone
							{
								messageBack += MessageToString(msg);
								break;
							}
						case Message.ChoiceType.ClientToClient: // A client wants to send a message to another client
							{
								messageBack += MessageToString(msg);
								break;
							}
						case Message.ChoiceType.ClientToServer: // A client sent me (the server) a message
							{
								Event.Trigger(Message.OnReceive, new Message.ParamsReceive() { Message = msg });
								break;
							}
						case Message.ChoiceType.ClientToAllAndServer: // A client is sending me (the server) and all other clients a message
							{
								Event.Trigger(Message.OnReceive, new Message.ParamsReceive() { Message = msg });
								messageBack += MessageToString(msg);
								break;
							}
					}
				}
				if (messageBack != "") server.Multicast(messageBack);
			}
			else
			{
				for (int i = 0; i < messages.Count; i++)
				{
					var msg = messages[i];
					switch (msg.type)
					{
						case Message.ChoiceType.ChangeID: // Server said someone's ID is taken and sent a free one
							{
								if (msg.Content == sessionID.ToString()) // Is this for me? (UID is still old so ID check)
								{
									var oldID = ClientUniqueID;
									var newID = msg.ReceiverUniqueID;
									clientIDs.Remove(oldID);
									clientIDs.Add(newID);
									ClientUniqueID = newID;

									Log($"Client UID '{oldID}' is taken. New Client UID is '{newID}'.");
									OnClientUniqueIDTaken?.Invoke(oldID);
								}
								break;
							}
						case Message.ChoiceType.ClientConnected: // Server said some client connected
							{
								if (msg.Content != ClientUniqueID) // If not me
								{
									clientIDs.Add(msg.Content);
									Log($"Client '{msg.Content}' connected. {ConnectedClients()}");
									OnClientConnect?.Invoke(msg.Content);
								}
								// when it's me it's handled in Client.OnConnected overriden method
								break;
							}
						case Message.ChoiceType.ClientDisconnected: // Server said some client disconnected
							{
								clientIDs.Remove(msg.Content);
								Log($"Client '{msg.Content}' disconnected. {ConnectedClients()}");
								OnClientDisconnect?.Invoke(msg.Content);
								break;
							}
						case Message.ChoiceType.ClientOnline: // Someone just connected and is getting updated on who is already online
							{
								if (msg.ReceiverUniqueID != ClientUniqueID)
									break; // Not for me? Not interested.

								var clientUIDs = msg.Content.Split(Message.TEMP_SEP, StringSplitOptions.RemoveEmptyEntries);
								for (int j = 0; j < clientUIDs.Length; j++)
								{
									if (clientIDs.Contains(clientUIDs[j]))
										continue;
									clientIDs.Add(clientUIDs[j]);
								}
								Log(ConnectedClients());
								break;
							}
						case Message.ChoiceType.ClientToAll: // A client is sending a message to all clients
							{
								if (msg.SenderUniqueID == ClientUniqueID)
									break; // Is this my message coming back to me?
								Event.Trigger(Message.OnReceive, new Message.ParamsReceive() { Message = msg });
								break;
							}
						case Message.ChoiceType.ClientToAllAndServer: // A client is sending a message to the server and all clients
							{
								if (msg.SenderUniqueID == ClientUniqueID)
									break; // Is this my message coming back to me?
								Event.Trigger(Message.OnReceive, new Message.ParamsReceive() { Message = msg });
								break;
							}
						case Message.ChoiceType.ClientToClient: // A client is sending a message to another client
							{
								if (msg.ReceiverUniqueID != ClientUniqueID)
									break; // Not for me? Not interested.
								if (msg.SenderUniqueID == ClientUniqueID)
									return; // Is this my message coming back to me? (unlikely)
								Event.Trigger(Message.OnReceive, new Message.ParamsReceive() { Message = msg });
								break;
							}
						case Message.ChoiceType.ServerToAll: // The server sent everyone a message
							{
								Event.Trigger(Message.OnReceive, new Message.ParamsReceive() { Message = msg });
								break;
							}
						case Message.ChoiceType.ServerToClient: // The server sent some client a message
							{
								if (msg.ReceiverUniqueID != ClientUniqueID)
									return; // Not for me?
								Event.Trigger(Message.OnReceive, new Message.ParamsReceive() { Message = msg });
								break;
							}
					}
				}
			}
		}
		private static void OpenPort()
		{
			var tNetFwPolicy2 = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
			var fwPolicy2 = (INetFwPolicy2)Activator.CreateInstance(tNetFwPolicy2);
			var currentProfiles = fwPolicy2.CurrentProfileTypes;

			var inboundRule = (INetFwRule2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
			inboundRule.Enabled = true;
			inboundRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
			inboundRule.Protocol = 6; // TCP
			inboundRule.Name = $"SMPL Multiplayer";
			inboundRule.Profiles = currentProfiles;

			var firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
			firewallPolicy.Rules.Add(inboundRule);
		}
		private static void Log(string message)
		{
			window.List.TryAddStringNewLine(message, true);
		}
	}
}
