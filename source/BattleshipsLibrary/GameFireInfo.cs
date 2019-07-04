using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace BattleshipsLibrary
{
    //Informace, kam je mířený zásah, ID hry

    [ProtoContract]
    public class GameFireInfo
    {
        [ProtoMember(1)]
        public int GameId { get; set; }
        [ProtoMember(2)]
        public GridPosition Position { get; set; }

        public GameFireInfo()
        {

        }

        public GameFireInfo(int gameId, GridPosition position)
        {
            GameId = gameId;
            Position = position;
        }
    }
}
