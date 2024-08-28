using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BekeritesM.Model
{
    public class BekeritesEventArgs
    {
        private Int32 _redPoints;
        private Int32 _bluePoints;

        private HashSet<KeyValuePair<Int32, Int32>> _coordinates;

        public Int32 RedPoints { get { return _redPoints; } }

        public Int32 BluePoints { get { return _bluePoints; } }

        public HashSet<KeyValuePair<Int32, Int32>> Coordinates { get { return _coordinates; } }

        public BekeritesEventArgs(Int32 redPoints, Int32 bluePoints, HashSet<KeyValuePair<Int32, Int32>> coordinates)
        {
            _redPoints = redPoints;
            _bluePoints = bluePoints;

            _coordinates = coordinates;
        }
    }
}
