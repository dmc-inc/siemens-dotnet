using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Siemens.Common.Interfaces;
using Dmc.Siemens.Common.Plc;
using Dmc.Siemens.SimaticManager.Plc.Types;
using Dmc.Wpf.Base;

namespace Dmc.Siemens.SimaticManager.Plc
{
	public class Symbol : DataObject
	{

		#region Constructors

		public Symbol(string name = "", SymbolTableType symbolType = SymbolTableType.Memory, DataType dataType = DataType.UNKNOWN, string comment = null) : base(name, dataType, comment, null)
		{
			this.SymbolType = symbolType;

			if (!Symbol.SupportedDataTypes.Contains(DataType))
				throw new ArgumentException("Datatype " + dataType.ToString() + " not supported by the symbol table.", nameof(dataType));
		}

		#endregion

		#region Public Properties

		private SymbolTableType _SymbolType;
		public SymbolTableType SymbolType
		{
			get
			{
				return this._SymbolType;
			}
			set
			{
				this.SetProperty(ref this._SymbolType, value);
			}
		}

		#endregion

		#region Private Properties

		private static readonly HashSet<DataType> SupportedDataTypes = new HashSet<DataType>()
		{
			DataType.BOOL,
			DataType.BYTE,
			DataType.CHAR,
			DataType.DINT,
			DataType.DWORD,
			DataType.INT,
			DataType.WORD
		};

		#endregion

	}
}
