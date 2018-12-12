using EasyPlaylist.ViewModels;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyPlaylist.Events
{
    class SelectedItemChangedEvent : PubSubEvent<MenuItemViewModel>
    {
    }
}
