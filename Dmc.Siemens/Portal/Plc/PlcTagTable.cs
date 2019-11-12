using System.Collections.Generic;
using Dmc.Wpf;
using Dmc.Siemens.Common.Plc.Base;
using Dmc.Siemens.Portal.Base;
using Dmc.Wpf.Collections;

namespace Dmc.Siemens.Portal.Plc
{
	public class PlcTagTable : NotifyPropertyChanged, ITagTable
	{

		public PlcTagTable(string name = null)
		{
			this.Name = name;
		}

		private string _name;
		public string Name
        {
            get => this._name;
            set => this.SetProperty(ref this._name, value);
        }

        public ICollection<PlcTag> PlcTags { get; } = new ObservableHashSet<PlcTag>();

		public ICollection<ConstantsEntry> Constants { get; } = new ObservableHashSet<ConstantsEntry>();

		public static IEnumerable<PlcTagTable> FromFile(string path)
		{
            return string.IsNullOrEmpty(path) ? null : (IEnumerable<PlcTagTable>)null;
			//return ExcelEngine.PlcTagTableFromFile(path);
		}

	}
}
