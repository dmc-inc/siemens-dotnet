using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Wpf;

namespace Dmc.Siemens.Common.Plc.Base
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
            get => this._Name;
            set => this.SetProperty(ref this._Name, value);
        }

        private DataType _DataType;
        public DataType DataType
        {
            get => this._DataType;
            set => this.SetProperty(ref this._DataType, value);
        }

        private object _Value;
        public object Value
        {
            get => this._Value;
            set => this.SetProperty(ref this._Value, value);
        }

        #endregion

    }
}
