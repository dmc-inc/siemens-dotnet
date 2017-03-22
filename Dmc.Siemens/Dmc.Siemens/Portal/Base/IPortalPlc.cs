using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Siemens.Common.Interfaces;

namespace Dmc.Siemens.Portal.Base
{
	public interface IPortalPlc : IPlc
	{

		IEnumerable<IPlcTagTable> TagTables { get; }

	}
}
