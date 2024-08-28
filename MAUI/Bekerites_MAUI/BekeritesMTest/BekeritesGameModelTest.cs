using BekeritesM.Model;
using BekeritesM.Persistence;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
namespace BekeritesMTest
{
    [TestClass]
    public class BekeritesGameModelTest
    {
        private BekeritesGameModel _model = null!; // a tesztelendõ modell
        private BekeritesTable _mockedTable = null!; // mockolt játéktábla
        private Mock<IBekeritesDataAccess> _mock = null!; // az adatelérés mock-ja

        [TestInitialize]

        public void Initialize()
        {
            _mockedTable = new BekeritesTable(8);
            _mockedTable.SetValue(0, 0, Player.PlayerRed);
            _mockedTable.SetValue(1, 0, Player.PlayerRed);
            _mockedTable.SetValue(5, 5, Player.PlayerBlue);
            _mockedTable.SetValue(5, 4, Player.PlayerBlue);
            // elõre definiálunk egy játéktáblát a perzisztencia mockolt teszteléséhez

            _mock = new Mock<IBekeritesDataAccess>();
            _mock.Setup(mock => mock.LoadAsync(It.IsAny<String>()))
                .Returns(() => Task.FromResult(_mockedTable));
            // a mock a LoadAsync mûveletben bármilyen paraméterre az elõre beállított játéktáblát fogja visszaadni

            _model = new BekeritesGameModel(_mock.Object);
            // példányosítjuk a modellt a mock objektummal

            _model.GameWon += new EventHandler<GameWonEventArgs>(Model_GameWon);
            _model.GameOver += new EventHandler(Model_GameOver);

        }

        [TestMethod]
        public void BekeritesGameModelNewGameEightTest()
        {
            _model.NewGame();

            Assert.AreEqual(8, _model.Table.Size);
            Assert.AreEqual(0, _model.OccupiedAreaR);
            Assert.AreEqual(0, _model.OccupiedAreaB);
        }

        [TestMethod]
        public void BekeritesGameModelNewGameSixTest()
        { 
            _model.GameMode = GameMode.x6;
            _model.NewGame();

            Assert.AreEqual(6, _model.Table.Size);
            Assert.AreEqual(0, _model.OccupiedAreaR);
            Assert.AreEqual(0, _model.OccupiedAreaB);
        }

        [TestMethod]
        public void BekeritesGameModelNewGameTenTest()
        { 
            _model.GameMode = GameMode.x10;
            _model.NewGame();

            Assert.AreEqual(10, _model.Table.Size);
            Assert.AreEqual(0, _model.OccupiedAreaR);
            Assert.AreEqual(0, _model.OccupiedAreaB);
        }

        [TestMethod]
        public void BekeritesGameModelStepTest()
        {
            _model.NewGame();
            _model.SecondBlock(0, 0); // (0, 0), (0, 1)
            _model.StepGame(0, 0);

            Assert.AreEqual(Player.PlayerRed, _model.Table[0, 0]);
            Assert.AreEqual(Player.PlayerRed, _model.Table[0, 1]);
            
            _model.Table.CurrentPlayer = Player.PlayerRed;

            _model.SecondBlock(0, 2); // (0, 2), (1, 2)
            _model.SecondBlock(0, 2);
            _model.SecondBlock(0, 2);
            _model.StepGame(0, 2);

            _model.Table.CurrentPlayer = Player.PlayerRed;

            _model.SecondBlock(2, 2); // (2, 2), (2, 1)
            _model.SecondBlock(2, 2);
            _model.SecondBlock(2, 2);
            _model.SecondBlock(2, 2);
            _model.StepGame(2, 2);

            _model.Table.CurrentPlayer = Player.PlayerRed; // (2, 0), (1, 0)
            _model.SecondBlock(2, 0);
            _model.StepGame(2, 0);

            Assert.AreEqual(Player.PlayerRedOwned, _model.Table[1, 1]);

        }

        [TestMethod]
        public async Task BekeritesGameModelLoadTest()
        {
            _model.NewGame();

            await _model.LoadGameAsync(String.Empty);
            for (Int32 i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Assert.AreEqual(_mockedTable.GetValue(i, j), _model.Table.GetValue(i, j));
                    // ellenõrizzük, valamennyi mezõ értéke megfelelõ-e

                }
            }

            _mock.Verify(dataAccess => dataAccess.LoadAsync(String.Empty), Times.Once());

        }

        private void Model_GameOver(Object? sender, EventArgs e)
        {
            Assert.IsTrue(_model.IsGameOver); // biztosan vége van a játéknak

        }

        private void Model_GameWon(Object? sender, GameWonEventArgs e)
        {
            Assert.IsTrue(_model.IsGameOver); // biztosan vége van a játéknak
            Assert.IsFalse(_model.OccupiedAreaR == _model.OccupiedAreaB);
        }


    }
}