using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Wpf;

namespace Dmc.Siemens.Common.Plc
{
    public class ProjectFolder : NotifyPropertyChanged
    {

        #region Constructors

        public ProjectFolder()
        {

        }

        #endregion

        #region Public Properties

        private string _Name;
        public string Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                this.SetProperty(ref this._Name, value);
            }
        }

        private ObservableCollection<ProjectFolder> _SubFolders = new ObservableCollection<ProjectFolder>();
        public ObservableCollection<ProjectFolder> SubFolders
        {
            get
            {
                return this._SubFolders;
            }
            set
            {
                this.SetProperty(ref this._SubFolders, value);
            }
        }

        private ProjectFolder _ParentFolder;
        public ProjectFolder ParentFolder
        {
            get
            {
                return this._ParentFolder;
            }
            set
            {
                this.SetProperty(ref this._ParentFolder, value);
            }
        }

        public string Path
        {
            get
            {
                if (ParentFolder != null)
                {
                    return ParentFolder.Path + "\\" + Name;
                }
                return Name;
            }
        }

        #endregion

    }
}
