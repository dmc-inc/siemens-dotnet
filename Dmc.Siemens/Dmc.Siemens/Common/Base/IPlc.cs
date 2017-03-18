using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Siemens.Common.PLC;

namespace Dmc.Siemens.Common.Base
{
	public interface IPlc : IAutomationObject
	{

		IEnumerable<Block> Blocks { get; }

	}
}
