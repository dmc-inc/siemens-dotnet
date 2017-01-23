using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Wpf.Base;
using DMC.Siemens.Common.PLC.Base;
using DMC.Siemens.Portal.Base;

namespace DMC.Siemens.Portal.PLC
{
    public class PlcTagTable : NotifyPropertyChanged, IPlcTagTable
    {

        #region Constructors

        public PlcTagTable(string name = null)
        {
            this.Name = name;
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

        public IEnumerable<IPlcTag> PlcTags { get; } = new HashSet<IPlcTag>();

        public IEnumerable<ConstantsEntry> Constants { get; } = new HashSet<ConstantsEntry>();

        #endregion

    }
}
