using Newtonsoft.Json;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EasyPlaylist.ViewModels
{
    class ExplorerViewModel : HierarchicalTreeViewModel
    {

        #region Properties

        [JsonIgnore]
        public override bool IsPlaylist { get { return false; } }

        #endregion

        public ExplorerViewModel(IEventAggregator eventAggregator, string name = "Unnamed") : base(eventAggregator, name)
        {

        }

        #region Events

        [JsonIgnore]
        public ICommand DisplayRecentFiles
        {
            get
            {
                return new DelegateCommand((parameter) =>
                {
                    DoSearch(x => x.IsRecent);
                });
            }
        }

        #endregion

        #region Public methods

        #endregion

        #region Private methods

        #endregion
    }
}
