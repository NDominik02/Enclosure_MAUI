using BekeritesM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bekerites_MAUI.ViewModel
{
    public class GameModeViewModel : ViewModelBase
    {
        private GameMode _mapSize;

        public GameMode MapSize
        {
            get => _mapSize;
            set 
            {
                _mapSize = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MapSizeText));
            }
        }

        public string MapSizeText => _mapSize.ToString();
    }
}
