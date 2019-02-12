using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Debugger
{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        int val;

        public event PropertyChangedEventHandler PropertyChanged;
        public String ID { get; private set; }
        public String Value { get { return val.ToString(); } }

        public RegisterViewModel(int id)
        {
            ID = id.ToString();
        }

        public void SetValue(int value)
        {
            if(val != value)
            {
                val = value;
                OnPropertyChanged("Value");
            }
        }

        void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
