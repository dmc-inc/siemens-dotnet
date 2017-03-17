using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Wpf.Base;
using Dmc.Siemens.Common.Base;

namespace Dmc.Siemens.Common.PLC
{
    public abstract class Block : NotifyPropertyChanged, IParsableSource
    {
        #region Public Properties

        private BlockType _Type;
        public BlockType Type
        {
            get
            {
                return this._Type;
            }
            set
            {
                this.SetProperty(ref this._Type, value);
            }
        }

        private BlockLanguage _Language;
        public BlockLanguage Language
        {
            get
            {
                return this._Language;
            }
            set
            {
                this.SetProperty(ref this._Language, value);
            }
        }

        private int _Number;
        public int Number
        {
            get
            {
                return this._Number;
            }
            set
            {
                this.SetProperty(ref this._Number, value);
            }
        }

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

        private string _Version;
        public string Version
        {
            get
            {
                return this._Version;
            }
            set
            {
                this.SetProperty(ref this._Version, value);
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

        #endregion

        #region Protected Properties

        protected abstract string DataHeader { get; }

        #endregion

        #region Public Methods

        public abstract IParsableSource ParseSource(TextReader reader);

        #endregion
        
    }
}
