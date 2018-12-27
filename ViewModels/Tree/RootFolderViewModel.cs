using EasyPlaylist.Events;
using Newtonsoft.Json;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyPlaylist.ViewModels
{
    class RootFolderViewModel : FolderViewModel
    {
        #region Properties 

        private HierarchicalTreeViewModel _parentHierarchicalTree;
        /// <summary>
        /// Référence vers l'arbre parent
        /// </summary>
        [JsonIgnore]
        public HierarchicalTreeViewModel ParentHierarchicalTree { get { return _parentHierarchicalTree; } }

        #endregion

        [JsonConstructor]
        public RootFolderViewModel(IEventAggregator eventAggregator, string title, HierarchicalTreeViewModel parentHierarchicalTree) : base(eventAggregator, "", title)
        {
            _parentHierarchicalTree = parentHierarchicalTree;
        }
    }
}
