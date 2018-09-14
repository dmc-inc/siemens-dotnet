using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Wpf;
using Dmc.Siemens.Common.Plc;
using Dmc.Siemens.Portal.Base;

namespace Dmc.Siemens.Portal.Plc
{
    public class PlcTag : NotifyPropertyChanged, ITag
    {

        #region Constructors

        public PlcTag(ITagTable parentTagTable, string name = null, string address = null, string comment = null, DataType dataType = DataType.UNKNOWN, bool isHmiVisible = true, bool isHmiAccessible = true)
        {
            this.TagTable = parentTagTable;
            this.Name = name;
            this.DataType = dataType;
            this.LogicalAddress = address;
            this.Comment = comment;
            this.IsHmiVisible = isHmiVisible;
            this.IsHmiAccessible = isHmiAccessible;
        }

        #endregion

        #region Public Properties

        private ITagTable _TagTable;
        public ITagTable TagTable
        {
            get
            {
                return this._TagTable;
            }
            set
            {
                this.SetProperty(ref this._TagTable, value);
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

        private DataType _DataType;
        public DataType DataType
        {
            get
            {
                return this._DataType;
            }
            set
            {
                this.SetProperty(ref this._DataType, value);
            }
        }

        private string _LogicalAddress;
        public string LogicalAddress
        {
            get
            {
                return this._LogicalAddress;
            }
            set
            {
                this.SetProperty(ref this._LogicalAddress, value);
            }
        }

        private string _Comment;
        public string Comment
        {
            get
            {
                return this._Comment;
            }
            set
            {
                this.SetProperty(ref this._Comment, value);
            }
        }

        private bool _IsHmiVisible;
        public bool IsHmiVisible
        {
            get
            {
                return this._IsHmiVisible;
            }
            set
            {
                this.SetProperty(ref this._IsHmiVisible, value);
            }
        }

        private bool _IsHmiAccessible;
        public bool IsHmiAccessible
        {
            get
            {
                return this._IsHmiAccessible;
            }
            set
            {
                this.SetProperty(ref this._IsHmiAccessible, value);
            }
        }

        #endregion

    }
}
