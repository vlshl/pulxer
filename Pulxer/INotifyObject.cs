using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Pulxer
{
    public class INotifyObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
