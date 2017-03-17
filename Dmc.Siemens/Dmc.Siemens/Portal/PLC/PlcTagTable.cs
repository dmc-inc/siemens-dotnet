using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Wpf.Base;
using Dmc.Siemens.Common.PLC.Base;
using Dmc.Siemens.Portal.Base;

namespace Dmc.Siemens.Portal.PLC
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
