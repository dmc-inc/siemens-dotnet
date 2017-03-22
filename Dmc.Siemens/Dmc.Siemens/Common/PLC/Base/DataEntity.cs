using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Siemens.Base;
using Dmc.Siemens.Common.Base;
using Dmc.Siemens.Common.Interfaces;
using Dmc.Siemens.Portal.Base;

namespace Dmc.Siemens.Common.PLC
{
    public abstract class DataEntity : Block, IDataEntry
    {

		#region Public Properties

		public DataType DataType => DataType.STRUCT;

		public LinkedList<DataEntry> Children { get; set; } = new LinkedList<DataEntry>();
		
		#endregion

		#region Public Methods
		
		public override IParsableSource ParseSource(TextReader reader)
        {
            string line;
            string[] split;
            bool isInData = false;

            while ((line = reader.ReadLine()) != null)
            {
                if (!isInData)
                {
                    if (line.Contains("VERSION"))
                    {
                        split = line.Split(':');
                        if (split.Length > 1)
                        {
                            this.Version = split[1].Trim();
                        }
                    }
                    else if (line.Contains(this.DataHeader))
                    {
                        isInData = true;
                    }
                }
                else
                {
                    if (!String.IsNullOrWhiteSpace(line))
                    {
                        if (line.Contains("END_" + this.DataHeader))
                        {
                            isInData = false;
                            break;
                        }
                        else
                        {
                            this.Children.AddLast(DataEntry.FromString(line, reader));
                        }
                    }
                }

            }

            return this;

        }

		public IEnumerator<DataEntry> GetEnumerator()
		{
			return ((IEnumerable<DataEntry>)this.Children).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<DataEntry>)this.Children).GetEnumerator();
		}

		public IDictionary<DataEntry, Address> CalcluateAddresses(IPlc plc)
		{
			return TagHelper.CalcluateAddresses(this, plc);
		}

		public Address CalculateSize(IPlc plc)
		{
			return TagHelper.CalculateSize(this, plc);
		}

		#endregion

		#region Private Methods



		#endregion

	}
}
