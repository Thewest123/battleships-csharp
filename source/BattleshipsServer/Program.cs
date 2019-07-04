using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using BattleshipsLibrary;
using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections;

namespace BattleshipsServer
{
    class Program
    {    
        public static List<Client> Clients = new List<Client>();
        public static List<Game> Games = new List<Game>();
        public static string ServerName = "Wolfram";

        private static int _gamesCountId = 0;

        static void Main(string[] args)
        {
            //Nastavení jiného jména z parametru konzole
            if (args.Length == 1) ServerName = args[0];

            Console.Title = $"{ServerName} | BattleShips Server";

            //Packet pointery
            NetworkComms.AppendGlobalIncomingPacketHandler<string>("ConnectUser", ConnectUser);
            NetworkComms.AppendGlobalIncomingPacketHandler<GameStartRequest>("GameStartRequest", GameStartRequest);
            NetworkComms.AppendGlobalIncomingPacketHandler<GameFireInfo>("GameFireInfo", GameFireInfo);
            NetworkComms.AppendGlobalIncomingPacketHandler<ChatMessage>("BroadcastChatMessage", BroadcastChatMessageDelgatePointer);
            NetworkComms.AppendGlobalIncomingPacketHandler<string>("ChallengeRequest", ChallengeRequestDelgatePointer);
            NetworkComms.AppendGlobalIncomingPacketHandler<string>("Disconnect", DisconnectDelgatePointer);
            //NetworkComms.AppendGlobalConnectionCloseHandler(ConnectionShutdownDelegate);

            //Naslouchání na adresách počítače
            Connection.StartListening(ConnectionType.TCP, new IPEndPoint(IPAddress.Any, 20123));

            //Výpis adres
            Console.WriteLine($"Server čeká TCP připojení na adresách:");
            foreach (var endPoint in Connection.ExistingLocalListenEndPoints(ConnectionType.TCP))
            {
                var localEndPoint = (IPEndPoint) endPoint;
                Console.WriteLine(" -> {0} : {1}", localEndPoint.Address, localEndPoint.Port);
            }

            //Zavření po stisku klávesy
            Console.WriteLine();
            Console.WriteLine("Stiskem libovolné klávesy vypnete server");
            Console.WriteLine("-----------------------------------\n");
            Console.ReadKey(true);

            //Ukončení procesu připojení
            NetworkComms.Shutdown();
        }

        //Odpojení klienta na základě žádosti
        private static void DisconnectDelgatePointer(PacketHeader packetheader, Connection connection, string enemyName)
        {
            //IP Klienta
            IPEndPoint clientEndPoint = (IPEndPoint)connection.ConnectionInfo.RemoteEndPoint;

            Client enemy = Clients.Find(x => x.Name == enemyName);
            Client client = Clients.Find(x => x.Ip == clientEndPoint.Address.ToString() && x.Port == clientEndPoint.Port);

            //Odeslání protivníkovi, že se klient odpojil
            if (enemy != null) NetworkComms.SendObject("Disconnect", enemy.Ip, enemy.Port, true);

            Console.WriteLine($"({client.Name}) se odpojil!");

            Game game = Games.Find(x => x.HasClient(enemy) || x.HasClient(client));

            //Pokud existují, odstranění záznamů z listů
            if (game != null) Games.Remove(game);
            Clients.Remove(client);

            foreach (Client c in Clients)
            {
                NetworkComms.SendObject("UpdateListInfo", c.Ip, c.Port, Clients);
            }
        }

        //Žádost o vyzvání hráče
        private static void ChallengeRequestDelgatePointer(PacketHeader packetheader, Connection connection, string name)
        {
            //Vyzvaný hráč - protivník
            Client enemy = Clients.Find(x => x.Name == name);
            bool isEnemyInGame = Games.Any(x => x.HasClient(enemy));

            IPEndPoint clientEndPoint = (IPEndPoint) connection.ConnectionInfo.RemoteEndPoint;

            //Pokud existuje a není ve hře
            if (enemy != null && !isEnemyInGame)
            {
                //Vyzyvatel
                Client player = Clients.Find(x => x.Ip == clientEndPoint.Address.ToString() && x.Port == clientEndPoint.Port);

                Console.WriteLine($"({player.Name}) vyzývá ke hře uživatele {enemy.Name}");
                
                try
                {
                    //Odeslání žádosti, čekání 20 sekund na odpověď
                    bool response = NetworkComms.SendReceiveObject<string, bool>("ChallengeAcceptRequest", enemy.Ip, enemy.Port, "ChallengeAcceptInfo", 20000, player.Name);

                    //Akceptováno
                    if (response)
                    {
                        Console.WriteLine($"({enemy.Name}) přijal výzvu od {player.Name}");
                        NetworkComms.SendObject("ChallengeAccepted", clientEndPoint.Address.ToString(), clientEndPoint.Port, enemy);
                        NetworkComms.SendObject("ChallengeAccepted", enemy.Ip, enemy.Port, player);
                    }
                    //Zamítnuto
                    else
                    {
                        Console.WriteLine($"({enemy.Name}) zamítl výzvu od {player.Name}");
                        NetworkComms.SendObject("ChallengeFailed", clientEndPoint.Address.ToString(), clientEndPoint.Port, $"Uživatele zamítl Vaši žádost ke hře!");
                    }
                }
                //Chyba, bez reakce
                catch
                {
                    Console.WriteLine($"({enemy.Name}) nereaguje na výzvu od {player.Name}");
                    NetworkComms.SendObject("ChallengeFailed", clientEndPoint.Address.ToString(), clientEndPoint.Port,$"Uživatele nestihl na Vaši žádost zareagovat včas!");
                }
            }

            //Hráč už je ve hře
            if (isEnemyInGame)
            {
                NetworkComms.SendObject("ChallengeFailed", clientEndPoint.Address.ToString(), clientEndPoint.Port, $"Uživatel {name} momentálně soupeří s někým jiným!");
            }

            //Hráč nenalezen
            if (enemy == null)
            {
                NetworkComms.SendObject("ChallengeFailed", clientEndPoint.Address.ToString(), clientEndPoint.Port, $"Uživatele se nepodařilo najít!");

            }
        }

        //Odeslání chat zprávy všem klientům
        private static void BroadcastChatMessageDelgatePointer(PacketHeader packetheader, Connection connection, ChatMessage message)
        {

            foreach (Client client in Clients)
            {
                NetworkComms.SendObject("DisplayChatMessage", client.Ip, client.Port, message);
            }

            Console.WriteLine($"({message.PlayerName}) odeslal zprávu: {message.Message}");
        }

        //Info o střelbě na klienta
        private static void GameFireInfo(PacketHeader packetheader, Connection connection, GameFireInfo gfi)
        {
            Game game = Games.Find(x => x.Id == gfi.GameId);

            IPEndPoint clientEndPoint = (IPEndPoint) connection.ConnectionInfo.RemoteEndPoint;

            if (game.Client1.Ip == clientEndPoint.Address.ToString() && game.Client1.Port == clientEndPoint.Port)
            {
                Console.WriteLine($"(Hra #{game.Id}) Uživatel {game.Client1.Name} střílí");
                game.FireOnClient2(gfi.Position);
            }

            if (game.Client2.Ip == clientEndPoint.Address.ToString() && game.Client2.Port == clientEndPoint.Port)
            {
                Console.WriteLine($"(Hra #{game.Id}) Uživatel {game.Client2.Name} střílí");
                game.FireOnClient1(gfi.Position);
            }

            game.ResetIfEnd();
        }

        //Začátek hry
        private static void GameStartRequest(PacketHeader packetheader, Connection connection, GameStartRequest gsr)
        {
            IPEndPoint clientEndPoint = (IPEndPoint)connection.ConnectionInfo.RemoteEndPoint;
            Client client = Clients.Find(x => x.Ip == clientEndPoint.Address.ToString() && x.Port == clientEndPoint.Port);
            Client enemyClient = Clients.Find(x => x.Ip == gsr.EnemyIp && x.Port == gsr.EnemyPort);
            Game game = Games.Find(x => x.HasClient(enemyClient));

            //Hra již existuje, vytvořil ji druhý hráč
            if (game != null && client != game.Client1)
            {
                game.Client2 = client;
                game.Client2ShipPositions = gsr.ShipsPositions;

                game.StartGame(_gamesCountId);

                _gamesCountId++;
            }
            //Hra neexistuje, prvotní vytvoření
            else
            {
                Game g = new Game
                {
                    Client1 = client,
                    Client1ShipPositions = gsr.ShipsPositions
                };
                Games.Add(g);
                Console.WriteLine($"(Hra #{g.Id}) mezi {client.Name} a {enemyClient.Name} začala!");
            }

            Console.WriteLine($"({client.Name}) je připraven ke hře s uživatelem {enemyClient.Name}");

        }

        //Připojení klienta k serveru
        private static void ConnectUser(PacketHeader packetheader, Connection connection, string name)
        {
            IPEndPoint clientEndPoint = (IPEndPoint)connection.ConnectionInfo.RemoteEndPoint;

            //Uživatel se stejným jménem již existuje
            if (Clients.Any(x => x.Name == name))
            {
                string reason = string.Format($"Uživatelské jméno {name} je již zabrané!");
                
                ConnectResponse response = new ConnectResponse(ResponseType.Rejected, ServerName, reason);
                connection.SendObject("ConnectInfo", response);

                Console.WriteLine($"{name} se nepodařilo připojit k serveru z důvodu: {reason}");

            }
            //Přidání hráče
            else
            {
                Clients.Add(new Client(name, clientEndPoint.Address.ToString(), clientEndPoint.Port));

                ConnectResponse response = new ConnectResponse(ResponseType.Accepted, ServerName, Clients);
                connection.SendObject("ConnectInfo", response);

                Console.WriteLine($"({name}) se připojil k serveru (celkem  {Clients.Count} hráčů)");
            }

            //Aktualizace online klientů
            foreach (Client client in Clients)
            {
                NetworkComms.SendObject("UpdateListInfo", client.Ip, client.Port, Clients);
            }
        }
    }
}