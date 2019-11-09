using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Siemens.Common.Interfaces;
using Dmc.Siemens.Common.Plc;
using Dmc.Siemens.SimaticManager.Plc.Types;
using Dmc.Wpf;

namespace Dmc.Siemens.SimaticManager.Plc
{
	public class Symbol : DataObject
	{

        private SymbolTableType _symbolType;
		public SymbolTableType SymbolType
        {
            get => this._symbolType;
            set => this.SetProperty(ref this._symbolType, value);
        }

        private static readonly HashSet<DataType> s_supportedDataTypes = new HashSet<DataType>()
		{
			DataType.BOOL,
			DataType.BYTE,
			DataType.CHAR,
			DataType.DINT,
			DataType.DWORD,
			DataType.INT,
			DataType.WORD
		};

        public Symbol(string name = "", SymbolTableType symbolType = SymbolTableType.Memory, DataType dataType = DataType.UNKNOWN, string comment = null) : base(name, dataType, comment, null)
        {
            this.SymbolType = symbolType;

            if (!Symbol.s_supportedDataTypes.Contains(this.DataType))
                throw new ArgumentException("Datatype " + dataType.ToString() + " not supported by the symbol table.", nameof(dataType));
        }

    }
}
