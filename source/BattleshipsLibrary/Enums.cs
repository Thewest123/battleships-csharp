using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BattleshipsLibrary
{
    public class Enums
    {
        //Navrácení popisů enumu, místo jména
        //https://www.codingame.com/playgrounds/2487/c---how-to-display-friendly-names-for-enumerations
        public static string GetDescription(Enum enumName)
        {
            Type enumNameType = enumName.GetType();
            MemberInfo[] memberInfo = enumNameType.GetMember(enumName.ToString());
            if ((memberInfo.Length > 0))
            {
                var _Attribs = memberInfo[0].GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
                if ((_Attribs.Count() > 0))
                {
                    return ((DescriptionAttribute)_Attribs.ElementAt(0)).Description;
                }
            }
            return enumName.ToString();
        }
    }

    //Typ lodi
    public enum ShipType
    {
        [Description("Nic nevybráno!")]
        None = 0,

        [Description("Torpédoborec")]
        Destroyer = 1,

        [Description("Ponorka")]
        Submarine = 2,

        [Description("Křižník")]
        Cruiser = 3,

        [Description("Bitevní loď")]
        Battleship = 4,

        [Description("Letadlová loď")]
        Carrier = 5
    }

    //Texture dlaždice
    public enum TileType
    {
        Water,
        ShipCenterHorizontal,
        ShipCenterVertical,
        ShipEndLeft,
        ShipEndRight,
        ShipEndUp,
        ShipEndDown,
        ShipSolo
    }

    //Typ položení lodi
    public enum PlacementType
    {
        Solo,
        Horizontal,
        Vertical,
        Invalid,
        Occupied
    }

    //Aktualizace gridu hráče nebo nepřítele (křížky, kolečka)
    public enum UpdateType
    {
        PlayerGrid,
        EnemyGrid
    }

    //Odpověď serveru na žádost o připojení
    public enum ResponseType
    {
        Accepted,
        Rejected
    }
}
