using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleshipsClient
{
    public static class Utils
    {
        //Rotace obrázku a jeho následné vrácení
        public static Image RotateImage(Bitmap img, RotateFlipType rotateFlipType)
        {
            img.RotateFlip(rotateFlipType);
            return img;
        }
    }
}
