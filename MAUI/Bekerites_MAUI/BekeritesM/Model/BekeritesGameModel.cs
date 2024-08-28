using BekeritesM.Persistence;
namespace BekeritesM.Model
{
    public enum GameMode { x6, x8, x10 }
    public class BekeritesGameModel
    {
        #region Fields

        private IBekeritesDataAccess _dataAccess; // adatelérés
        private BekeritesTable _table;
        private GameMode _gameMode; // játékmód

        private Int32 _occupiedAreaR = 0; //Piros játékos elfoglalt területei
        private Int32 _occupiedAreaB = 0; // Kék játékos elfoglalt területei

        private Int32 rotateCount = 0; // nézi hogy merre tud forogni a 2. blokk
        #endregion

        #region Properties

        public BekeritesTable Table { get { return _table; } }

        public Int32 RotateCount { get { return rotateCount; } set { rotateCount = value; } }

        /// <summary>
        /// Játék végének lekérdezése
        /// </summary>
        public Boolean IsGameOver { get { return _table.IsFilled; } }

        public Int32 OccupiedAreaR { get { return _occupiedAreaR; } set { _occupiedAreaR = value; } }
        public Int32 OccupiedAreaB { get { return _occupiedAreaB; } set { _occupiedAreaB = value; } }

        public GameMode GameMode { get { return _gameMode; } set { _gameMode = value; } }

        #endregion

        #region Events

        public event EventHandler<GameWonEventArgs>? GameWon;

        public event EventHandler? GameOver;

        public event EventHandler<BekeritesEventArgs>? GameAdvanced;

        public event EventHandler<FieldChangedEventArgs>? RotatedSecondBlock;
        /// <summary>
        /// Játék létrehozásának eseménye.
        /// </summary>
        public event EventHandler? GameCreated;

        #endregion

        #region Constructor

        public BekeritesGameModel(IBekeritesDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
            _table = new BekeritesTable(8);
            _gameMode = GameMode.x8;

        }

        #endregion

        #region Public game methods

        public void NewGame()
        {
            _table.CurrentPlayer = Player.PlayerRed;

            switch (_gameMode)
            {
                case GameMode.x6:
                    _table = new BekeritesTable(6);
                    break;
                case GameMode.x8:
                    _table = new BekeritesTable(8);
                    break;
                case GameMode.x10:
                    _table = new BekeritesTable(10);
                    break;
            }

            OccupiedAreaB = 0;
            OccupiedAreaR = 0;

            OnGameCreated();
        }

        public void StepGame(Int32 x, Int32 y)
        {
            if (IsGameOver)
                return;

            HashSet<KeyValuePair<Int32, Int32>> visited = new HashSet<KeyValuePair<int, int>>();

            _table.StepValue(x, y); // CurrentPlayer lesz az[x,y]
           
            int[] dx = { -1, 0, 1, 0 };
            int[] dy = { 0, 1, 0, -1 };

            _table.StepValue(dx[(rotateCount + 3) % 4] + x, dy[(rotateCount + 3) % 4] + y);

            bool wentOutOfMap = false;

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    wentOutOfMap = DealWithSurrounded(x + i, y + j, ref visited);

                    if (!wentOutOfMap)
                    {
                        foreach (var coordinate in visited)
                        {
                            SetPlayerAfterPaint(coordinate.Key, coordinate.Value, Table.CurrentPlayer);
                        }

                        OccupiedAreaCalc();

                        OnGameAdvanced(OccupiedAreaR, OccupiedAreaB, visited);
                    }


                    wentOutOfMap = DealWithSurrounded((dx[(rotateCount + 3) % 4] + x) + i, (dy[(rotateCount + 3) % 4] + y) + j, ref visited);

                    if (!wentOutOfMap)
                    {
                        foreach (var coordinate in visited)
                        {
                            SetPlayerAfterPaint(coordinate.Key, coordinate.Value, Table.CurrentPlayer);
                        }

                        OccupiedAreaCalc();

                        OnGameAdvanced(OccupiedAreaR, OccupiedAreaB, visited);

                    }

                }

            }

            _table.CurrentPlayer = _table.CurrentPlayer == Player.PlayerRed ? Player.PlayerBlue : Player.PlayerRed;
            //következő játékos jön

            if (_table.IsFilled)
            {
                if (OccupiedAreaB > OccupiedAreaR)
                {
                    OnGameWon(Player.PlayerBlue);

                }
                else if (OccupiedAreaR > OccupiedAreaB)
                {
                    OnGameWon(Player.PlayerRed);
                }
                else
                {
                    OnGameOver();
                }
            }
        }

        public void SetPlayerAfterPaint(Int32 x, Int32 y, Player p)
        {
            if (p == Player.PlayerRed)
            {
                Table.SetValue(x, y, Player.PlayerRedOwned);
            }
            else
            {
                Table.SetValue(x, y, Player.PlayerBlueOwned);
            }

        }

        public void OccupiedAreaCalc()
        {
            OccupiedAreaR = 0;
            OccupiedAreaB = 0;

            for (int i = 0; i < Table.Size-1; i++)
            {
                for (int j = 0; j < Table.Size-1; j++)
                {
                    if (Table[i, j] == Player.PlayerRedOwned)
                    {
                        OccupiedAreaR++;
                    }
                    else if (Table[i, j] == Player.PlayerBlueOwned)
                    {
                        OccupiedAreaB++;
                    }
                }
            }
        }

        private bool DealWithSurrounded(Int32 x, Int32 y, ref HashSet<KeyValuePair<Int32, Int32>> visited)
        {
            KeyValuePair<Int32, Int32> coordinate = new KeyValuePair<int, int>(x, y);
            if (x < 0 || x > Table.Size -1 || y < 0 || y > Table.Size - 1)
            {
                return true; //talált rossz értéket
            }
            if (visited.Contains(coordinate) ||
                Table.CurrentPlayer == Table[x, y])
            {
                return false;
            }
            visited.Add(coordinate);
            bool wentOutOfMap = DealWithSurrounded(x + 1, y, ref visited) ||
                                DealWithSurrounded(x - 1, y, ref visited) ||
                                DealWithSurrounded(x, y + 1, ref visited) ||
                                DealWithSurrounded(x, y - 1, ref visited) ||

                                DealWithSurrounded(x + 1, y + 1, ref visited) ||
                                DealWithSurrounded(x - 1, y - 1, ref visited) ||
                                DealWithSurrounded(x - 1, y + 1, ref visited) ||
                                DealWithSurrounded(x + 1, y - 1, ref visited);
            if (wentOutOfMap)
            {
                visited.Clear();
            }
            return wentOutOfMap;
        }

        public void SecondBlock(Int32 x, Int32 y)
        {
            int[] dx = { -1, 0, 1, 0 };
            int[] dy = { 0, 1, 0, -1 };

            int value = rotateCount;
            for (int j = value; j < value + 4; j++)
            {
                rotateCount = (rotateCount + 1) % 4;
                int i = j % 4;
                if (x + dx[i] >= 0 && x + dx[i] < _table.Size && y + dy[i] >= 0 && y + dy[i] < _table.Size && _table[x + dx[i], y + dy[i]] == Player.NoPlayer)
                {
                    OnRotatedSecondBlock(x + dx[i], y + dy[i]);

                    break;
                }
            }
        }

        public async Task LoadGameAsync(String path)
        {
            if (_dataAccess == null)
            {
                throw new InvalidOperationException("No data access is provided.");
            }

            _table = await _dataAccess.LoadAsync(path);

            switch (Table.Size)
            {
                case 6: GameMode = GameMode.x6;
                    break;
                case 8: GameMode = GameMode.x8; 
                    break;
                case 10: GameMode = GameMode.x10;
                    break;
            }

            OccupiedAreaCalc();
            OnGameCreated();

        }

        public async Task SaveGameAsync(String path)
        {
            if (_dataAccess == null)
            {
                throw new InvalidOperationException("No data access is provided.");
            }

            await _dataAccess.SaveAsync(path, _table);
        }

        #endregion

        #region Private event methods

        private void OnGameWon(Player player)
        {
            if (GameWon != null)
            {
                GameWon(this, new GameWonEventArgs(player));
            }
        }

        private void OnGameOver()
        {
            if (GameOver != null)
            {
                GameOver(this, EventArgs.Empty);
            }
        }

        private void OnGameAdvanced(Int32 redPoints, Int32 bluePoints, HashSet<KeyValuePair<Int32, Int32>> coordinates)
        {
            if (GameAdvanced != null)
            {
                GameAdvanced(this, new BekeritesEventArgs(redPoints, bluePoints, coordinates));
            }
        }

        private void OnRotatedSecondBlock(Int32 x, Int32 y)
        {
            if (RotatedSecondBlock != null)
            {
                RotatedSecondBlock(this, new FieldChangedEventArgs(x, y, Player.NoPlayer));
            }
        }

        private void OnGameCreated()
        {
            if (GameCreated != null)
            {
                GameCreated(this, EventArgs.Empty);
            }
        }


        #endregion
    }
}