using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Siemens.Common.Plc;
using Dmc.Siemens.Portal.Plc.Logic.Base;

namespace Dmc.Siemens.Portal.Plc.Logic.Instructions
{
	public class Move : InstructionBase
	{

		#region Public Properties

		private DataEntry _In;
		public DataEntry In
		{
			get
			{
				return this._In;
			}
			set
			{
				this.SetProperty(ref this._In, value);
			}
		}

		public ObservableCollection<DataEntry> Out { get; } = new ObservableCollection<DataEntry>();

		#endregion

		#region Protected Properties

		protected override string _Name => "Move";

		#endregion

	}
}
