using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Siemens.Common.Plc;
using Dmc.Siemens.Common.Plc.Interfaces;
using Dmc.Siemens.Common.Plc.Types;

namespace Dmc.Siemens.Common.Interfaces
{
	public interface IPlc : IAutomationObject
	{

		IDictionary<BlockType, ICollection<IBlock>> Blocks { get; }

		ICollection<UserDataType> UserDataTypes { get; }

	}
}
