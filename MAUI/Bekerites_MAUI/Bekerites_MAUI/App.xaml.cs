using BekeritesM.Persistence;
using Bekerites_MAUI.Persistence;
using BekeritesM.Model;
using Bekerites_MAUI.ViewModel;


namespace Bekerites_MAUI;

public partial class App : Application
{

    /// <summary>
    /// Erre az útvonalra mentjük a félbehagyott játékokat
    /// </summary>
    private const string SuspendedGameSavePath = "SuspendedGame";

    private readonly AppShell _appShell;
    private readonly IBekeritesDataAccess _bekeritesDataAccess;
    private readonly BekeritesGameModel _bekeritesGameModel;
    private readonly IStore _bekeritesStore;
    private readonly BekeritesViewModel _bekeritesViewModel;


    public App()
	{
		InitializeComponent();

		_bekeritesStore = new BekeritesStore();
        _bekeritesDataAccess = new BekeritesFileDataAccess(FileSystem.AppDataDirectory);

        _bekeritesGameModel = new BekeritesGameModel(_bekeritesDataAccess);
        _bekeritesViewModel = new BekeritesViewModel(_bekeritesGameModel);

        _appShell = new AppShell(_bekeritesStore, _bekeritesDataAccess, _bekeritesGameModel, _bekeritesViewModel)
        {
            BindingContext = _bekeritesViewModel
        };
        MainPage = _appShell;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        Window window = base.CreateWindow(activationState);

        // az alkalmazás indításakor
        window.Created += (s, e) =>
        {
            // új játékot indítunk
            _bekeritesGameModel.NewGame();
        };

        // amikor az alkalmazás fókuszba kerül
        window.Activated += (s, e) =>
        {
            if (!File.Exists(Path.Combine(FileSystem.AppDataDirectory, SuspendedGameSavePath)))
                return;

            Task.Run(async () =>
            {
                // betöltjük a felfüggesztett játékot, amennyiben van
                try
                {
                    await _bekeritesGameModel.LoadGameAsync(SuspendedGameSavePath);
                }
                catch
                {
                }
            });
        };

        // amikor az alkalmazás fókuszt veszt
        window.Deactivated += (s, e) =>
        {
            Task.Run(async () =>
            {
                try
                {
                    // elmentjük a jelenleg folyó játékot
                    await _bekeritesGameModel.SaveGameAsync(SuspendedGameSavePath);
                }
                catch
                {
                }
            });
        };

        return window;
    }

}
