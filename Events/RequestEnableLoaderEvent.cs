using EasyPlaylist.ViewModels;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyPlaylist.Events
{
    /// <summary>
    /// Evenement permettant de demander à activer ou désactiver l'ihm.
    /// </summary>
    class RequestEnableLoaderEvent : PubSubEvent<EnableLoaderPayload>
    {
    }
}
