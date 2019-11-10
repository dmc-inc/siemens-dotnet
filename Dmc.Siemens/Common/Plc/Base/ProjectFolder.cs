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

        private string _name;
        public string Name
        {
            get => this._name;
            set => this.SetProperty(ref this._name, value);
        }

        private ObservableCollection<ProjectFolder> _subFolders = new ObservableCollection<ProjectFolder>();
        public ObservableCollection<ProjectFolder> SubFolders
        {
            get => this._subFolders;
            set => this.SetProperty(ref this._subFolders, value);
        }

        private ProjectFolder _parentFolder;
        public ProjectFolder ParentFolder
        {
            get => this._parentFolder;
            set => this.SetProperty(ref this._parentFolder, value);
        }

        public string Path
        {
            get
            {
                if (this.ParentFolder != null)
                {
                    return this.ParentFolder.Path + "\\" + this.Name;
                }
                return this.Name;
            }
        }

        #endregion

    }
}
