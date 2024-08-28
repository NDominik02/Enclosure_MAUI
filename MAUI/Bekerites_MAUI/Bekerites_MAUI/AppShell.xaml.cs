using BekeritesM.Model;
using BekeritesM.Persistence;
using Bekerites_MAUI.ViewModel;
using Bekerites_MAUI.View;

namespace Bekerites_MAUI;

public partial class AppShell : Shell
{
    #region Fields

    private IBekeritesDataAccess _bekeritesDataAccess;
    private readonly BekeritesGameModel _bekeritesGameModel;
    private readonly BekeritesViewModel _bekeritesViewModel;

    private readonly IStore _store;
    private readonly StoredGameBrowserModel _storedGameBrowserModel;
    private readonly StoredGameBrowserViewModel _storedGameBrowserViewModel;

    #endregion

    #region Application methods

    public AppShell(IStore bekeritesStore, IBekeritesDataAccess bekeritesDataAccess, BekeritesGameModel bekeritesGameModel, BekeritesViewModel bekeritesViewModel)
    {
        InitializeComponent();

        // játék összeállítása
        _store = bekeritesStore;
        _bekeritesDataAccess = bekeritesDataAccess;
        _bekeritesGameModel = bekeritesGameModel;
        _bekeritesViewModel = bekeritesViewModel;

        _bekeritesGameModel.GameOver += BekeritesGameModel_GameOver;
        _bekeritesGameModel.GameWon += BekeritesGameModel_GameWon;
        // többi nemtom kell e még

        _bekeritesViewModel.NewGame += BekeritesViewModel_NewGame;
        _bekeritesViewModel.LoadGame += BekeritesViewModel_LoadGame;
        _bekeritesViewModel.SaveGame += BekeritesViewModel_SaveGame;
        _bekeritesViewModel.ExitGame += BekeritesViewModel_ExitGame;

        // a játékmentések kezelésének összeállítása
        _storedGameBrowserModel = new StoredGameBrowserModel(_store);
        _storedGameBrowserViewModel = new StoredGameBrowserViewModel(_storedGameBrowserModel);
        _storedGameBrowserViewModel.GameLoading += StoredGameBrowserViewModel_GameLoading;
        _storedGameBrowserViewModel.GameSaving += StoredGameBrowserViewModel_GameSaving;

    }


    #endregion

    // Internal methods

    #region Model event handlers

    private async void BekeritesGameModel_GameOver(object? sender, EventArgs e)
    {

        await DisplayAlert("Bekerítés játék", "Döntetlen!", "OK");
    }

    private async void BekeritesGameModel_GameWon(object? sender, GameWonEventArgs e)
    {
        switch (e.Player)
        {
            case Player.PlayerRed:
                await DisplayAlert("A piros játékos győzött!", "Játék vége!", "OK");
                break;
            case Player.PlayerBlue:
                await DisplayAlert("A kék játékos győzött!", "Játék vége!", "OK");
                break;
        }
    }


    #endregion

    #region ViewModel event handlers

    /// <summary>
    ///     Új játék indításának eseménykezelője.
    /// </summary>
    private void BekeritesViewModel_NewGame(object? sender, EventArgs e)
    {
        _bekeritesGameModel.NewGame();
    }

    /// <summary>
    ///     Játék betöltésének eseménykezelője.
    /// </summary>
    private async void BekeritesViewModel_LoadGame(object? sender, EventArgs e)
    {
        await _storedGameBrowserModel.UpdateAsync(); // frissítjük a tárolt játékok listáját
        await Navigation.PushAsync(new LoadGamePage
        {
            BindingContext = _storedGameBrowserViewModel
        }); // átnavigálunk a lapra
    }

    /// <summary>
    ///     Játék mentésének eseménykezelője.
    /// </summary>
    private async void BekeritesViewModel_SaveGame(object? sender, EventArgs e)
    {
        await _storedGameBrowserModel.UpdateAsync(); // frissítjük a tárolt játékok listáját
        await Navigation.PushAsync(new SaveGamePage
        {
            BindingContext = _storedGameBrowserViewModel
        }); // átnavigálunk a lapra
    }

    private async void BekeritesViewModel_ExitGame(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new SettingsPage
        {
            BindingContext = _bekeritesViewModel
        }); // átnavigálunk a beállítások lapra
    }


    /// <summary>
    ///     Betöltés végrehajtásának eseménykezelője.
    /// </summary>
    private async void StoredGameBrowserViewModel_GameLoading(object? sender, StoredGameEventArgs e)
    {
        await Navigation.PopAsync(); // visszanavigálunk

        // betöltjük az elmentett játékot, amennyiben van
        try
        {
            await _bekeritesGameModel.LoadGameAsync(e.Name);

            // sikeres betöltés
            await Navigation.PopAsync(); // visszanavigálunk a játék táblára
            await DisplayAlert("Bekerítés játék", "Sikeres betöltés.", "OK");

            // csak akkor indul az időzítő, ha sikerült betölteni a játékot
        }
        catch
        {
            await DisplayAlert("Bekerítés játék", "Sikertelen betöltés.", "OK");
        }
    }

    /// <summary>
    ///     Mentés végrehajtásának eseménykezelője.
    /// </summary>
    private async void StoredGameBrowserViewModel_GameSaving(object? sender, StoredGameEventArgs e)
    {
        await Navigation.PopAsync(); // visszanavigálunk

        await _bekeritesGameModel.SaveGameAsync(e.Name);
        await DisplayAlert("Bekerítés játék", "Sikeres mentés.", "OK");

        try
        {
            // elmentjük a játékot
        }
        catch
        {
            await DisplayAlert("Bekerítés játék", "Sikertelen mentés.", "OK");
        }
    }

    #endregion


}
