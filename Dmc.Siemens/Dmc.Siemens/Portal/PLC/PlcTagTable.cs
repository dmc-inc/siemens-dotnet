using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Wpf.Base;
using Dmc.Siemens.Common.PLC.Base;
using Dmc.Siemens.Portal.Base;
using Dmc.IO;
using System.IO;
using Dmc.Siemens.Base;
using SpreadsheetLight;

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

		public ICollection<IPlcTag> PlcTags { get; } = new HashSet<IPlcTag>();

		public ICollection<ConstantsEntry> Constants { get; } = new HashSet<ConstantsEntry>();

		#endregion

		#region Public Methods

		public static IEnumerable<IPlcTagTable> FromFile(string path)
		{
			return ExcelEngine.PlcTagTableFromFile(path);
		}

		#endregion

	}
}
