using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bekerites_MAUI.ViewModel
{
    public class BekeritesField : ViewModelBase
    {
        private System.Drawing.Color _fieldColor = System.Drawing.Color.White;

        public System.Drawing.Color FieldColor
        {
            get { return _fieldColor; }
            set
            {
                if (_fieldColor != value)
                {
                    _fieldColor = value;
                    OnPropertyChanged();
                }
            }
        }

        public Int32 X { get; set; }

        public Int32 Y { get; set; }

        public Int32 Number { get; set; }

        public DelegateCommand? StepCommand { get; set; }

    }
}
