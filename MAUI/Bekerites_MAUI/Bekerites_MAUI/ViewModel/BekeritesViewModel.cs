using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BekeritesM.Model;

namespace Bekerites_MAUI.ViewModel
{
    public class BekeritesViewModel : ViewModelBase
    {
        #region Fields

        private BekeritesGameModel _model;

        private Int32 _size;

        private GameModeViewModel _mapSize = null!;

        private Int32 _occupiedAreaR = 0;
        private Int32 _occupiedAreaB = 0;

        private BekeritesField? currentField = null;
        private BekeritesField? previouslyPainted = null;


        #endregion

        #region Properties

        // Új játék kezdése parancs lekérdezése.
        public DelegateCommand NewGameCommand { get; private set; }

        // Játék betöltése parancs lekérdezése.
        public DelegateCommand LoadGameCommand { get; private set; }

        // Játék mentése parancs lekérdezése.
        public DelegateCommand SaveGameCommand { get; private set; }

        // Kilépés parancs lekérdezése.
        public DelegateCommand ExitCommand { get; private set; }

        public DelegateCommand FinishMovement { get; private set; }

        // Játékmező gyűjtemény lekérdezése.
        public ObservableCollection<BekeritesField> Fields { get; set; }

        /// <summary>
        /// Táblaméretek
        /// </summary>
        public ObservableCollection<GameModeViewModel> MapSizeLevels { get; set; }

        public Int32 OccupiedAreaR { get { return _occupiedAreaR; } set { _occupiedAreaR = value; OnPropertyChanged(nameof(OccupiedAreaR)); } }
        public Int32 OccupiedAreaB { get { return _occupiedAreaB; } set { _occupiedAreaB = value; OnPropertyChanged(nameof(OccupiedAreaB)); } }

        public GameModeViewModel MapSize
        { 
            get => _mapSize;
            set 
            {
                _mapSize = value;
                _model.GameMode = value.MapSize;
                OnPropertyChanged();
            }
        }

        public int Size
        {
            get => _size;
            set
            {
                _size = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(GameTableRows));
                OnPropertyChanged(nameof(GameTableColumns));
            }
        }

        /// <summary>
        /// Segédproperty a tábla méretezéséhez
        /// </summary>
        public RowDefinitionCollection GameTableRows
        {
            get => new RowDefinitionCollection(Enumerable.Repeat(new RowDefinition(GridLength.Star), Size).ToArray());
        }


        /// <summary>
        /// Segédproperty a tábla méretezéséhez
        /// </summary>
        public ColumnDefinitionCollection GameTableColumns
        {
            get => new ColumnDefinitionCollection(Enumerable.Repeat(new ColumnDefinition(GridLength.Star), Size).ToArray());
        }


        #endregion

        #region Events

        // Új játék eseménye.
        public event EventHandler? NewGame;

        // Játék betöltésének eseménye.
        public event EventHandler? LoadGame;

        // Játék mentésének eseménye.
        public event EventHandler? SaveGame;

        // Játékból való kilépés eseménye.
        public event EventHandler? ExitGame;

        #endregion

        #region Constructors

        // Bekerites nézetmodell példányosítása
        public BekeritesViewModel(BekeritesGameModel model)
        {
            // játék csatlakoztatása.
            _model = model;
            _model.GameAdvanced += new EventHandler<BekeritesEventArgs>(Model_GameAdvanced);
            _model.GameWon += new EventHandler<GameWonEventArgs>(Model_GameWon);
            _model.GameOver += new EventHandler(Model_GameOver);
            _model.RotatedSecondBlock += new EventHandler<FieldChangedEventArgs>(Model_RotatedSecondBlock);
            _model.GameCreated += new EventHandler(Model_GameCreated);

            OccupiedAreaR = _model.OccupiedAreaR;
            OccupiedAreaB = _model.OccupiedAreaB;

            // parancsok kezelése.
            NewGameCommand = new DelegateCommand(param => OnNewGame());
            LoadGameCommand = new DelegateCommand(param => OnLoadGame());
            SaveGameCommand = new DelegateCommand(param => OnSaveGame());
            ExitCommand = new DelegateCommand(param => OnExitGame());
            FinishMovement = new DelegateCommand(param => OnFinishMovement());

            // tábla méretek
            MapSizeLevels = new ObservableCollection<GameModeViewModel>
            {
                new GameModeViewModel{ MapSize = GameMode.x6 },
                new GameModeViewModel{ MapSize = GameMode.x8 },
                new GameModeViewModel{ MapSize = GameMode.x10 }
            };
            MapSize = MapSizeLevels[1]; // x8 -ra állítja
            Size = _model.Table.Size;

            // Játéktábla létrehozása.
            Fields = new ObservableCollection<BekeritesField>();
            for (Int32 i = 0; i < _model.Table.Size; i++) // inicializáljuk a mezőket
            {
                for (Int32 j = 0; j < _model.Table.Size; j++)
                {
                    Fields.Add(new BekeritesField
                    {
                        FieldColor = System.Drawing.Color.White,
                        X = i,
                        Y = j,
                        Number = i * _model.Table.Size + j, // a gomb sorszáma, amelyet felhasználunk az azonosításhoz
                        StepCommand = new DelegateCommand(param => Step(Convert.ToInt32(param)))
                        // ha egy mezőre léptek, akkor jelezzük a léptetést, változtatjuk a lépésszámot  !!!NEKEM A SZÍNT KELL VÁLTOZTATNI
                    });
                }
            }
            RefreshTable();


        }

        #endregion

        #region Private methods

        // Tábla frissítése
        private void RefreshTable()
        {
            OccupiedAreaR = _model.OccupiedAreaR;
            OnPropertyChanged(nameof(OccupiedAreaR));
            OccupiedAreaB = _model.OccupiedAreaB;
            OnPropertyChanged(nameof(OccupiedAreaB));

            if (Size != _model.Table.Size)
            {
                Fields.Clear();
                for (Int32 i = 0; i < _model.Table.Size; i++)
                {
                    for (Int32 j = 0; j < _model.Table.Size; j++)
                    {
                        Fields.Add(new BekeritesField
                        {
                            FieldColor = System.Drawing.Color.White,
                            X = i,
                            Y = j,
                            Number = i * _model.Table.Size + j,
                            StepCommand = new DelegateCommand(param => Step(Convert.ToInt32(param)))
                        });
                    }
                }
            }
            Size = _model.Table.Size;

            foreach (BekeritesField field in Fields) // inicializálni kell a mezőket is
            {
                if (_model.Table[field.X, field.Y] == Player.PlayerRed)
                {
                    field.FieldColor = System.Drawing.Color.Red;
                }
                if (_model.Table[field.X, field.Y] == Player.PlayerBlue)
                {
                    field.FieldColor = System.Drawing.Color.Blue;
                }
                if (_model.Table[field.X, field.Y] == Player.PlayerRedOwned)
                {
                    field.FieldColor = System.Drawing.Color.IndianRed;
                }
                if (_model.Table[field.X, field.Y] == Player.PlayerBlueOwned)
                {
                    field.FieldColor = System.Drawing.Color.DeepSkyBlue;
                }
                if (_model.Table[field.X, field.Y] == Player.NoPlayer)
                {
                    field.FieldColor = System.Drawing.Color.White;
                }
            }
        }

        // Játék léptetése - eseménykiváltás
        private void Step(Int32 index)
        {
            BekeritesField field = Fields[index];
            //_model.StepGame(field.X, field.Y);

            if (_model.Table[field.X, field.Y] == Player.PlayerRed)
            {
                field.FieldColor = System.Drawing.Color.Red;
            }
            if (_model.Table[field.X, field.Y] == Player.PlayerBlue)
            {
                field.FieldColor = System.Drawing.Color.Blue;
            }

            if (_model.Table[field.X, field.Y] != Player.NoPlayer)
            {
                return;
            }

            if (currentField == field)
            {
                _model.SecondBlock(field.X, field.Y);
            }
            else
            {
                if (currentField != null)
                {
                    currentField.FieldColor = System.Drawing.Color.White;
                }
                if (previouslyPainted != null)
                {
                    previouslyPainted.FieldColor = System.Drawing.Color.White;
                }
                previouslyPainted = null;
                field.FieldColor = _model.Table.CurrentPlayer == Player.PlayerRed ? System.Drawing.Color.Red : System.Drawing.Color.Blue;
                _model.RotateCount = 0;

            }

            currentField = field;
        }

        #endregion

        #region Game event handlers

        private void Model_GameOver(object? sender, EventArgs e)
        {
            _model.NewGame();
            RefreshTable();
        }

        private void Model_GameWon(object? sender, GameWonEventArgs e)
        {
            _model.NewGame();
            RefreshTable();

        }

        private void Model_GameAdvanced(object? sender, BekeritesEventArgs e)
        {
            foreach (var coordinate in e.Coordinates)
            {
                Fields[coordinate.Key * _model.Table.Size + coordinate.Value].FieldColor = _model.Table.CurrentPlayer == Player.PlayerRed ? System.Drawing.Color.IndianRed : System.Drawing.Color.DeepSkyBlue;
            }

            OccupiedAreaR = _model.OccupiedAreaR;
            OccupiedAreaB = _model.OccupiedAreaB;

        }

        private void Model_RotatedSecondBlock(object? sender, FieldChangedEventArgs e)
        {
            Fields[e.X * _model.Table.Size + e.Y].FieldColor = _model.Table.CurrentPlayer == Player.PlayerRed ? System.Drawing.Color.Red : System.Drawing.Color.Blue;

            if (previouslyPainted != null)
            {
                previouslyPainted.FieldColor = System.Drawing.Color.White;

            }

            previouslyPainted = Fields[e.X * _model.Table.Size + e.Y];

        }
        /// <summary>
        /// Játék létrehozásának eseménykezelője.
        /// </summary>
        private void Model_GameCreated(object? sender, EventArgs e)
        {
            RefreshTable();

        }


        #endregion

        #region Event methods

        //Új játék indításának eseménykiváltása.
        private void OnNewGame()
        {
            NewGame?.Invoke(this, EventArgs.Empty);
        }

        // Játék betöltésének eseménykiváltása.
        private void OnLoadGame()
        {
            LoadGame?.Invoke(this, EventArgs.Empty);
            RefreshTable();
        }

        // Játék mentésének eseménykiváltása.
        private void OnSaveGame()
        {
            SaveGame?.Invoke(this, EventArgs.Empty);
        }

        // Játék bezárásának eseménykiváltása.
        private void OnExitGame()
        {
            ExitGame?.Invoke(this, EventArgs.Empty);
        }

        private void OnFinishMovement()
        {
            if (previouslyPainted == null || currentField == null)
            {
                return;
            }

            _model.StepGame(currentField.X, currentField.Y);

            currentField = null;
            previouslyPainted = null;


        }


        #endregion
    }
}
