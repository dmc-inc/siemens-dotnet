using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dmc.Siemens.SimaticManager.Plc.Types
{
	public enum SymbolTableType
	{
		Memory,
		Input,
		Output,
		PeripheralInput,
		PeripheralOutput,
		OrganizationBlock,
		Function,
		FunctionBlock,
		UserDataType,
		VariableTable,
		SystemFunction,
		SystemFunctionBlock
	}
}
