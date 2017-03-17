using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Wpf.Base;

namespace Dmc.Siemens.Common.PLC.Base
{
    public class ConstantsEntry : NotifyPropertyChanged
    {

        #region Constructors

        public ConstantsEntry()
        {
        }

        public ConstantsEntry(string name, object value, DataType dataType = DataType.UNKNOWN)
        {
            this.Name = name;
            this.DataType = dataType;
            this.Value = value;
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

        private object _Value;
        public object Value
        {
            get
            {
                return this._Value;
            }
            set
            {
                this.SetProperty(ref this._Value, value);
            }
        }

        #endregion

    }
}
