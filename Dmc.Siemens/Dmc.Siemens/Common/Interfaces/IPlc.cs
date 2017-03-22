using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Siemens.Common.PLC;
using Dmc.Siemens.Common.PLC.Types;

namespace Dmc.Siemens.Common.Interfaces
{
	public interface IPlc : IAutomationObject
	{

		IDictionary<BlockType, IEnumerable<Block>> Blocks { get; }

		T GetConstantValue<T>(Constant<T> constant) where T : struct;

		IDataEntry GetUdtStructure(string udtName);

	}
}
