using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BekeritesM.Model;

namespace BekeritesM.Persistence
{
    public class BekeritesTable
    {
        #region Fields

        private Player[,] _gameTable;
        private Player _currentPlayer; //aktuális játékos
        

        #endregion

        #region Properties

        /// <summary>
        /// Játéktábla kitöltöttségének lekérdezése
        /// </summary>
        public Boolean IsFilled
        {
            get
            {
                for (int i = 0; i < _gameTable.GetLength(0); i++)
                {
                    for (int j = 0; j < _gameTable.GetLength(1); j++)
                    {
                        if (_gameTable[i, j] == Player.NoPlayer) // ha fehér
                        {
                            if (i + 1 < _gameTable.GetLength(0) && _gameTable[i + 1, j] == Player.NoPlayer)
                            {
                                return false;
                            }
                            if (i - 1 >= 0 && _gameTable[i - 1, j] == Player.NoPlayer)
                            {
                                return false;
                            }
                            if (j + 1 < _gameTable.GetLength(0) && _gameTable[i, j + 1] == Player.NoPlayer)
                            {
                                return false;
                            }
                            if (j - 1 >= 0 && _gameTable[i, j - 1] == Player.NoPlayer)
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
        }

        public Int32 Size { get { return _gameTable.GetLength(0); } }

        public Player CurrentPlayer { get { return _currentPlayer; }  set { _currentPlayer = value; } }

        public Player this[Int32 x, Int32 y] { get { return GetValue(x, y); } }

        #endregion

        #region public methods
        public Player GetValue(Int32 x, Int32 y)
        {
            if (x < 0 || x >= _gameTable.GetLength(0))
                throw new ArgumentOutOfRangeException(nameof(x), "The X coordinate is out of range.");
            if (y < 0 || y >= _gameTable.GetLength(1))
                throw new ArgumentOutOfRangeException(nameof(y), "The Y coordinate is out of range.");

            return _gameTable[x, y];
        }

        public void SetValue(Int32 x, Int32 y, Player player)
        {
            if (x < 0 || x >= _gameTable.GetLength(0))
                throw new ArgumentOutOfRangeException(nameof(x), "The X coordinate is out of range.");
            if (y < 0 || y >= _gameTable.GetLength(1))
                throw new ArgumentOutOfRangeException(nameof(y), "The Y coordinate is out of range.");

            _gameTable[x, y] = player;
        }
        #endregion

        public void StepValue(Int32 x, Int32 y)
        {
            if (x < 0 || x >= _gameTable.GetLength(0))
                throw new ArgumentOutOfRangeException(nameof(x), "The X coordinate is out of range.");
            if (y < 0 || y >= _gameTable.GetLength(1))
                throw new ArgumentOutOfRangeException(nameof(y), "The Y coordinate is out of range.");

            _gameTable[x, y] = CurrentPlayer; //pozíció rögzítése

        }

        public BekeritesTable(Int32 tableSize)
        {
            if (tableSize < 0)
                throw new ArgumentOutOfRangeException(nameof(tableSize), "The table size is less than 0.");

            _gameTable = new Player[tableSize, tableSize];
            _currentPlayer = Player.PlayerRed;

        }

    }
}
