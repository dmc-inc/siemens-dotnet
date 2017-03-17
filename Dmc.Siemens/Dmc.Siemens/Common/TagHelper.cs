using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Siemens.Common.PLC;

namespace Dmc.Siemens.Common
{
	public static class TagHelper
	{

		/// <summary>
		/// Calculates the size of the DataType specified.
		/// </summary>
		/// <returns>Size in bytes, -1 if not primitive.</returns>
		public static int GetPrimitiveByteSize(DataType dataType, int stringLength = 0)
		{
			switch (dataType)
			{
				case DataType.BOOL:
					return 0;
				case DataType.BYTE:
				case DataType.CHAR:
					return 1;
				case DataType.WORD:
				case DataType.INT:
				case DataType.DATE:
				case DataType.S5TIME:
				case DataType.COUNTER:
				case DataType.TIMER:
					return 2;
				case DataType.DWORD:
				case DataType.DINT:
				case DataType.TIME:
				case DataType.REAL:
				case DataType.TIME_OF_DAY:
					return 4;
				case DataType.POINTER:
					return 6;
				case DataType.DATE_AND_TIME:
					return 8;
				case DataType.ANY:
					return 10;
				case DataType.STRING:
					return (2 + stringLength);
				default:
					return -1;
			}
		}

	}
}
