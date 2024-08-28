using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BekeritesM.Model
{
    public class GameWonEventArgs : EventArgs
    {
        public Player Player { get; private set; }

        public GameWonEventArgs(Player player) { Player = player; }
    }
}
