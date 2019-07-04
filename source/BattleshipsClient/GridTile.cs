using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BattleshipsLibrary;

namespace BattleshipsClient
{
    class GridTile : PictureBox
    {
        //Pozice (X, Y)
        public GridPosition GridPosition { get; set; }
        public TileType TileType { get; set; }
        public ShipType ShipType { get; set; }

        private bool _isOnFire;

        //Nastavení textury včetně rotace
        public void SetTile(TileType tileType)
        {
            TileType = tileType;

            if (tileType == TileType.ShipCenterHorizontal) Image = Properties.Resources.ShipCenter;
            if (tileType == TileType.ShipCenterVertical) Image = Utils.RotateImage(Properties.Resources.ShipCenter, RotateFlipType.Rotate90FlipNone);
            if (tileType == TileType.ShipEndLeft) Image = Properties.Resources.ShipEnd;
            if (tileType == TileType.ShipEndRight) Image = Utils.RotateImage(Properties.Resources.ShipEnd, RotateFlipType.RotateNoneFlipX);
            if (tileType == TileType.ShipEndUp) Image = Utils.RotateImage(Properties.Resources.ShipEnd, RotateFlipType.Rotate90FlipNone);
            if (tileType == TileType.ShipEndDown) Image = Utils.RotateImage(Properties.Resources.ShipEnd, RotateFlipType.Rotate270FlipNone);
            if (tileType == TileType.ShipSolo) Image = Properties.Resources.ShipSolo;
            if (tileType == TileType.Water) Image = Properties.Resources.Water;
        }

        public void SetOnFire()
        {
            _isOnFire = true;
        }

        //Vykreslení křížku, kolečka, označení
        protected override void OnPaint(PaintEventArgs e)
        {
            //Vykreslení PictureBoxu
            base.OnPaint(e);

            if (GridPosition.IsHit && !_isOnFire) Drawing.DrawHit(this, e.Graphics);
            if (_isOnFire) Drawing.DrawFire(this, e.Graphics);
            if (GridPosition.IsMissed) Drawing.DrawMiss(this, e.Graphics);
            if (GridPosition.IsSelected) Drawing.DrawSelected(this, e.Graphics);
        }
    }
}
