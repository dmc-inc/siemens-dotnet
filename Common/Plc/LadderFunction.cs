using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using Dmc.Siemens.Common.Base;
using Dmc.Siemens.Common.Interfaces;
using Dmc.Siemens.Common.Plc.Base;
using Dmc.Siemens.Common.Plc.Interfaces;

namespace Dmc.Siemens.Common.Plc
{
	public class LadderFunction : BlockBase, IFunction
	{

		#region Constructors

		public LadderFunction()
		{

		}

		#endregion

		#region Public Properties

		public override BlockType Type => BlockType.Function;

		public override string DataHeader => throw new SiemensException("Function does not contain data.");

		private IList<DataEntry> _Inputs = new ObservableCollection<DataEntry>();
		public IList<DataEntry> Inputs
		{
			get
			{
				return this._Inputs;
			}
		}

		private IList<DataEntry> _Outputs = new ObservableCollection<DataEntry>();
		public IList<DataEntry> Outputs
		{
			get
			{
				return this._Outputs;
			}
		}

		private IList<DataEntry> _InputOutputs = new ObservableCollection<DataEntry>();
		public IList<DataEntry> InputOutputs
		{
			get
			{
				return this._InputOutputs;
			}
		}

		private IList<DataEntry> _TemporaryData = new ObservableCollection<DataEntry>();
		public IList<DataEntry> TemporaryData
		{
			get
			{
				return this._TemporaryData;
			}
		}

		#endregion

		#region Public Methods

		public override void Export(string path)
		{
			throw new NotImplementedException();
		}

		public override IParsableSource ParseSource(TextReader reader)
		{
			throw new NotImplementedException();
		}

		public override void ReadXml(XmlReader reader)
		{
			throw new NotImplementedException();
		}

		public override void WriteXml(XmlWriter writer)
		{
			throw new NotImplementedException();
		}

		#endregion

	}
}
