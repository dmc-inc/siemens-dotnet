using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Siemens.Common.Interfaces;
using Dmc.Siemens.Common.PLC.Types;

namespace Dmc.Siemens.Portal.Base
{
	public interface IPortalPlc : IPlc
	{

		IEnumerable<IPlcTagTable> TagTables { get; }

		T GetConstantValue<T>(Constant<T> constant) where T : struct;

	}
}
