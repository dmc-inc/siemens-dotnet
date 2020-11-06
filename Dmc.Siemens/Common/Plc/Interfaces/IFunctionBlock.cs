using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dmc.Siemens.Common.Plc.Interfaces
{
	public interface IFunctionBlock : IFunction
	{

		IList<DataEntry> StaticData { get; }

	}
}
