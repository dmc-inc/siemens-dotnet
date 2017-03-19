using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Siemens.Common.PLC;
using Dmc.Siemens.Portal.Base;

namespace Dmc.Siemens.Common.Interfaces
{
	public interface IDataEntry : IAutomationObject, IEnumerable<DataEntry>
	{

		#region Public Properties

		DataType DataType { get; }

		string Comment { get; set; }

		LinkedList<DataEntry> Children { get; }

		#endregion

		#region Public Methods

		IDictionary<DataEntry, Address> CalcluateAddresses(IPlc plc);

		int CalculateByteSize(IPlc plc);

		#endregion

	}
}
