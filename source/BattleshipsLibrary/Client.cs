using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace BattleshipsLibrary
{
    [ProtoContract]
    public class Client
    {
        //Proměnné
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public string Ip { get; set; }
        [ProtoMember(3)]
        public int Port { get; set; }

        //Prázdný konstruktor kvůli serializéru
        public Client()
        {
            
        }

        public Client(string name, string ip, int port)
        {
            Name = name;
            Ip = ip;
            Port = port;
        }
    }
}
