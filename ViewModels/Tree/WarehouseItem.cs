using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyPlaylist.ViewModels
{
    class WarehouseItemViewModel : BaseViewModel
    {
        private string name;
        private int count;
        private ObservableCollection<WarehouseItemViewModel> items;

        public WarehouseItemViewModel(string name, int count)
        {
            this.Name = name;
            this.Count = count;
            this.Items = new ObservableCollection<WarehouseItemViewModel>();
        }
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                if (value != this.name)
                {
                    this.name = value;
                    this.OnPropertyChanged("Name");
                }
            }
        }
        public ObservableCollection<WarehouseItemViewModel> Items
        {
            get
            {
                return this.items;
            }
            set
            {
                if (value != this.items)
                {
                    this.items = value;
                    this.OnPropertyChanged("Items");
                }
            }
        }
        public int Count
        {
            get
            {
                return this.count;
            }
            set
            {
                if (value != this.count)
                {
                    this.count = value;
                    this.OnPropertyChanged("Count");
                }
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
