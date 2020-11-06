using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dmc.Siemens.Common.Plc.Types
{
	public enum ReferenceScope
	{
		Input,
		Output,
		InputOuput,
		Static,
		Temporary,
		PreviousTemporary
	}
}
