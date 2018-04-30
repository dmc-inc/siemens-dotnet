using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Wpf.Base;
using Dmc.Siemens.Common.Plc.Base;
using Dmc.Siemens.Portal.Base;
using Dmc.IO;
using System.IO;
using SpreadsheetLight;
using Dmc.Wpf.Collections;
using System.Collections.ObjectModel;

namespace Dmc.Siemens.Portal.Plc
{
	public class PlcTagTable : NotifyPropertyChanged, ITagTable
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

		public ICollection<PlcTag> PlcTags { get; } = new ObservableHashSet<PlcTag>();

		public ICollection<ConstantsEntry> Constants { get; } = new ObservableHashSet<ConstantsEntry>();

		#endregion

		#region Public Methods

		public static IEnumerable<PlcTagTable> FromFile(string path)
		{
			return ExcelEngine.PlcTagTableFromFile(path);
		}

		#endregion

	}
}
