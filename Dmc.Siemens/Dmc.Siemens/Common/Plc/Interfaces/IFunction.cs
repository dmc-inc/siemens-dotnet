using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Wpf.Collections;

namespace Dmc.Siemens.Common.Plc.Interfaces
{
	public interface IFunction : IBlock
	{

		IList<DataEntry> Inputs { get; }

		IList<DataEntry> Outputs { get; }

		IList<DataEntry> InputOutputs { get; }

		IList<DataEntry> TemporaryData { get; }

	}
}
