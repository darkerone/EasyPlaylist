using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyPlaylist.Events
{
    class EnableLoaderPayload
    {
        /// <summary>
        /// Définit si le loader doit être activée ou désactivée
        /// </summary>
        public bool EnableLoader { get; set; }

        /// <summary>
        /// Définit qui a fait la demande
        /// </summary>
        public object Sender { get; set; }

        /// <summary>
        /// Définit si la demande outrepasse toutes les autres
        /// </summary>
        public bool Force { get; set; }

        /// <summary>
        /// Définit le message affiché lors du chargement
        /// </summary>
        public string LoaderMessage { get; set; }

        public EnableLoaderPayload(object sender, bool enableLoader, string loaderMessage = null, bool force = false)
        {
            Sender = sender;
            EnableLoader = enableLoader;
            Force = force;
            LoaderMessage = loaderMessage;
        }
    }
}
