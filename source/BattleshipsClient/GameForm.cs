﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using BattleshipsLibrary;
using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections;

namespace BattleshipsClient
{
    public partial class GameForm : Form
    {
        #region Proměnné

        //Info o serveru
        public string ServerIp;
        public int    ServerPort;

        //Info o sobě
        public string PlayerName;

        //Info o protivníkovi
        public string EnemyIp;
        public int    EnemyPort;
        public string EnemyName;

        //Nastavení lodí
        private ShipType _selectedShipType;
        private bool     _isDestroyerSet;
        private bool     _isSubmarineSet;
        private bool     _isCruiserSet;
        private bool     _isBattleshipSet;
        private bool     _isCarrierSet;

        //Označené pozice
        private GridPosition _selectedPosition;

        //Pozice mých lodí
        private List<GridPosition> _occupiedPositions = new List<GridPosition>();

        //Zavření okna uživatelem/kódem
        private bool _closeFromCode;

        //ID Aktuální hry
        private int _gameId;

        #endregion

        public GameForm()
        {
            InitializeComponent();
            MinimumSize = MaximumSize = Size;
            
            _selectedShipType = ShipType.None;

            //Vykreslení mřížek
            InitGrid(tlpPlayerGrid);
            InitGrid(tlpEnemyGrid, true);

            //Nastavení pointerů pro příchozí packety
            NetworkComms.AppendGlobalIncomingPacketHandler<GameStartInfo>("GameStartInfo", GameStartInfoDelegatePointer);
            NetworkComms.AppendGlobalIncomingPacketHandler<GamePositionUpdateInfo>("GamePositionUpdateInfo", GamePositionUpdateInfoDelegatePointer);
            NetworkComms.AppendGlobalIncomingPacketHandler<bool>("EndGameInfo", EndGameInfoDelegatePointer);
            NetworkComms.AppendGlobalIncomingPacketHandler<bool>("Disconnect", DisconnectDelegatePointer);
        }

        #region Hlavní metody

        //Nastavení mřížek
        private void InitGrid(TableLayoutPanel grid, bool isEnemy = false)
        {
            for (int column = 0; column < grid.ColumnCount; column++)
            {
                for (int row = 0; row < grid.RowCount; row++)
                {
                    var gridTile = new GridTile
                    {
                        Dock = DockStyle.Fill,
                        BackgroundImageLayout = ImageLayout.Center,
                        Margin = new Padding(0),

                        ShipType = ShipType.None,
                        GridPosition = new GridPosition(column, row)
                    };

                    gridTile.SetTile(TileType.Water);

                    grid.Controls.Add(gridTile, column, row);

                    gridTile.MouseDown += isEnemy ? new MouseEventHandler(EnemyGrid_Click) : (PlayerGrid_Click);
                }
            }
        }

        //Označení umístění lodí - PlayerGrid
        private GridTile _firstClick;
        private GridTile _secondClick;
        private void PlayerGrid_Click(object sender, MouseEventArgs e)
        {
            GridTile s = sender as GridTile;

            if (_firstClick == null)
            {
                Debug.WriteLine("1 click");
                rtbInfo.Text = "Klikněte na druhou pozici";
                _firstClick = s;
            }
            else
            {
                Debug.WriteLine("2 click");
                _secondClick = s;

                GridPosition start = _firstClick.GridPosition;
                GridPosition end = _secondClick.GridPosition;

                //Prohození hodnot
                if (start.X > end.X || start.Y > end.Y)
                {
                    var swap = start;
                    start = end;
                    end = swap;
                }

                //Zjištění druhu umístění
                PlacementType placement = CanBePlaced(_occupiedPositions, start, end, _selectedShipType);

                if (placement == PlacementType.Solo)
                {
                    GridTile gridTile = (GridTile)tlpPlayerGrid.GetControlFromPosition(start.X, start.Y);
                    gridTile.SetTile(TileType.ShipSolo);
                    _occupiedPositions.Add(gridTile.GridPosition);

                    SetShip(_selectedShipType);
                }
                else if (placement == PlacementType.Horizontal)
                {
                    for (int i = start.X; i <= end.X; i++)
                    {
                        GridTile gridTile = (GridTile)tlpPlayerGrid.GetControlFromPosition(i, start.Y);

                        if (i == start.X) gridTile.SetTile(TileType.ShipEndLeft);
                        else if (i == end.X) gridTile.SetTile(TileType.ShipEndRight);
                        else gridTile.SetTile(TileType.ShipCenterHorizontal);

                        _occupiedPositions.Add(gridTile.GridPosition);
                    }

                    SetShip(_selectedShipType);
                }
                else if (placement == PlacementType.Vertical)
                {
                    for (int i = start.Y; i <= end.Y; i++)
                    {
                        GridTile gridTile = (GridTile)tlpPlayerGrid.GetControlFromPosition(start.X, i);

                        if (i == start.Y) gridTile.SetTile(TileType.ShipEndUp);
                        else if (i == end.Y) gridTile.SetTile(TileType.ShipEndDown);
                        else gridTile.SetTile(TileType.ShipCenterVertical);

                        _occupiedPositions.Add(gridTile.GridPosition);
                    }
                    SetShip(_selectedShipType);
                }
                else if (placement == PlacementType.Invalid) MessageBox.Show("Není možné vložit loď!");
                else if (placement == PlacementType.Occupied) MessageBox.Show("Toto místo je již zabráno");

                Debug.WriteLine("Reset");
                _firstClick = null;
                _secondClick = null;
            }
        }

        //Označení cíle - EnemyGrid
        private void EnemyGrid_Click(object sender, MouseEventArgs e)
        {
            //Zrušení předchozího označení, pokud existuje
            if (_selectedPosition != null)
            {
                GridTile oldTile = (GridTile)tlpEnemyGrid.GetControlFromPosition(_selectedPosition.X, _selectedPosition.Y);
                oldTile.GridPosition.IsSelected = false;
                oldTile.Invalidate();
            }

            //Nové označení
            GridTile gridTile = (GridTile)sender;
            _selectedPosition = gridTile.GridPosition;
            gridTile.GridPosition.IsSelected = true;
            gridTile.Invalidate();
        }

        //Vložení lodí
        private void SetShip(ShipType shipType)
        {
            if (shipType == ShipType.Destroyer)
            {
                _isDestroyerSet = true;
                btnSelectDestroyer.Enabled = false;
            }

            if (shipType == ShipType.Submarine)
            {
                _isSubmarineSet = true;
                btnSelectSubmarine.Enabled = false;
            }

            if (shipType == ShipType.Cruiser)
            {
                _isCruiserSet = true;
                btnSelectCruiser.Enabled = false;
            }

            if (shipType == ShipType.Battleship)
            {
                _isBattleshipSet = true;
                btnSelectBattleship.Enabled = false;
            }

            if (shipType == ShipType.Carrier)
            {
                _isCarrierSet = true;
                btnSelectCarrier.Enabled = false;
            }

            _selectedShipType = ShipType.None;
            rtbInfo.Text = "Kliknutím na (+) vyberte loď";

            //Všechny lodě vloženy
            if (_isDestroyerSet && _isSubmarineSet && _isCruiserSet && _isBattleshipSet && _isCarrierSet)
            {
                AcceptButton = btnSubmitGrid;
                btnSubmitGrid.Enabled = true;
            }
        }

        //Ověření kolizí a umístění lodi
        public PlacementType CanBePlaced(List<GridPosition> occupied, GridPosition start, GridPosition end, ShipType shipType)
        {
            List<GridPosition> newGridPositions = new List<GridPosition>();
            PlacementType response;
            int length = 0;

            //Solo
            if (start.Equals(end))
            {
                length = 1;
                newGridPositions.Add(new GridPosition(start.X, start.Y));
                response = PlacementType.Solo;
            }
            //Horizontal
            else if (start.Y == end.Y)
            {
                for (int x = start.X; x <= end.X; x++)
                {
                    newGridPositions.Add(new GridPosition(x, start.Y));
                    length++;
                }

                response = PlacementType.Horizontal;
            }
            //Vertical
            else if (start.X == end.X)
            {
                for (int y = start.Y; y <= end.Y; y++)
                {
                    newGridPositions.Add(new GridPosition(start.X, y));
                    length++;
                }

                response = PlacementType.Vertical;
            }
            //Invalid
            else
            {
                response = PlacementType.Invalid;
            }

            //Check if already exists
            if (occupied.Intersect(newGridPositions).Any())
            {
                response = PlacementType.Occupied;
            }

            //Check length
            if (shipType == ShipType.None) response = PlacementType.Invalid;
            if (shipType == ShipType.Destroyer && length != 1) response = PlacementType.Invalid;
            if (shipType == ShipType.Submarine && length != 2) response = PlacementType.Invalid;
            if (shipType == ShipType.Cruiser && length != 3) response = PlacementType.Invalid;
            if (shipType == ShipType.Battleship && length != 4) response = PlacementType.Invalid;
            if (shipType == ShipType.Carrier && length != 5) response = PlacementType.Invalid;

            return response;
        }

        //Přesun zpět do lobby - delegát
        private delegate void GoToLobbyFormDelegate();

        //Přesun zpět do lobby
        private void GoToLobbyForm()
        {
            _closeFromCode = true;
            Close();
            LoginForm.Lobby.Show();
        }

        //Nastavení, kdo je na řadě - delegát
        private delegate void NextTurnDelegate(bool isMyTurn);

        //Nastavení, kdo je na řadě
        private void NextTurn(bool isMyTurn)
        {
            if (isMyTurn)
            {
                btnFire.Enabled = true;
                rtbInfo.Text = "Jste na řadě!";
            }
            else
            {
                btnFire.Enabled = false;
                _selectedPosition = null;
                rtbInfo.Text = "Čekání na protivníka...";
            }
        }

        #endregion

        #region Delegate pointery packetů

        //Začátek hry
        private void GameStartInfoDelegatePointer(PacketHeader packetheader, Connection connection, GameStartInfo info)
        {
            _gameId = info.GameId;

            if (info.IsStarting)
            {
                MessageBox.Show("Hra začala, jste na řadě!");
                Invoke(new NextTurnDelegate(NextTurn), true);
            }
            else
            {
                MessageBox.Show("Hra začala, protihráč střílí první!");
            }
        }

        //Update po střelbě
        private void GamePositionUpdateInfoDelegatePointer(PacketHeader packetheader, Connection connection, GamePositionUpdateInfo info)
        {
            if (info.UpdateType == UpdateType.PlayerGrid)
            {
                GridTile tile = (GridTile)tlpPlayerGrid.GetControlFromPosition(info.GridPosition.X, info.GridPosition.Y);
                int posIndex = _occupiedPositions.FindIndex(x => x.Equals(info.GridPosition));
                if (posIndex >= 0) _occupiedPositions[posIndex] = info.GridPosition;
                tile.GridPosition = info.GridPosition;
                if (tile.GridPosition.IsHit) tile.SetOnFire();
                tile.Invalidate();
            }

            if (info.UpdateType == UpdateType.EnemyGrid)
            {
                GridTile tile = (GridTile)tlpEnemyGrid.GetControlFromPosition(info.GridPosition.X, info.GridPosition.Y);
                tile.GridPosition = info.GridPosition;
                tile.MouseDown -= EnemyGrid_Click;
                tile.Invalidate();
            }

            Invoke(new NextTurnDelegate(NextTurn), info.IsOnTurn);
        }

        //Konec hry
        private void EndGameInfoDelegatePointer(PacketHeader packetheader, Connection connection, bool isWinner)
        {
            MessageBox.Show(isWinner ? "Vyhráli jste!" : "Prohráli jste!");
            Invoke(new GoToLobbyFormDelegate(GoToLobbyForm));
        }

        //Odpojení protivníka
        private void DisconnectDelegatePointer(PacketHeader packetheader, Connection connection, bool incomingobject)
        {
            MessageBox.Show("Protivník se odpojil!");
            Invoke(new GoToLobbyFormDelegate(GoToLobbyForm));
        }

        #endregion

        #region Form Eventy

        //Zavření okna
        private void GameForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_closeFromCode) return;
            NetworkComms.SendObject("Disconnect", ServerIp, ServerPort, EnemyName);
        }

        //Odeslání umístění mých lodí serveru, připraven ke hře
        private void btnSubmitGrid_Click(object sender, EventArgs e)
        {
            GameStartRequest gsr = new GameStartRequest(_occupiedPositions, EnemyIp, EnemyPort);
            NetworkComms.SendObject("GameStartRequest", ServerIp, ServerPort, gsr);
            btnSubmitGrid.Enabled = false;
            AcceptButton = btnFire;
        }

        //Výběr lodi podle tlačítek
        private void btnSelectDestroyer_Click(object sender, EventArgs e)   => SelectShip(ShipType.Destroyer);
        private void btnSelectSubmarine_Click(object sender, EventArgs e)   => SelectShip(ShipType.Submarine);
        private void btnSelectCruiser_Click(object sender, EventArgs e)     => SelectShip(ShipType.Cruiser);
        private void btnSelectBattleship_Click(object sender, EventArgs e)  => SelectShip(ShipType.Battleship);
        private void btnSelectCarrier_Click(object sender, EventArgs e)     => SelectShip(ShipType.Carrier);

        //Výběr lodi
        private void SelectShip(ShipType type)
        {
            _selectedShipType = type;
            rtbInfo.Text = $"Vybráno: {Enums.GetDescription(_selectedShipType)}\nKlikněte na první pozici";
        }

        //Útok
        private void btnFire_Click(object sender, EventArgs e)
        {
            if (_selectedPosition != null)
            {
                GameFireInfo gfi = new GameFireInfo(_gameId, _selectedPosition);
                NetworkComms.SendObject("GameFireInfo", ServerIp, ServerPort, gfi);

                NextTurn(false);
            }
            else
            {
                MessageBox.Show("Nejdříve musíte vybrat pozici!");
            }
        }

        //Aktualizace labelů při spuštění
        private void GameForm_Shown(object sender, EventArgs e)
        {
            lblMyWaters.Text += $" ({PlayerName})";
            lblEnemyWaters.Text += $" ({EnemyName})";
            Text = $"Bitva (#{_gameId})";
        }

        #endregion
    }
}
