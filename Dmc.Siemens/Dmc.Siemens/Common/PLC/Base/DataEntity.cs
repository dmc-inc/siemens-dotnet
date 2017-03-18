using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Siemens.Base;
using Dmc.Siemens.Common.Base;
using Dmc.Siemens.Portal.Base;

namespace Dmc.Siemens.Common.PLC
{
    public abstract class DataEntity : Block, IEnumerable<DataEntry>
    {

        #region Public Properties

        public LinkedList<DataEntry> Data { get; set; } = new LinkedList<DataEntry>();
		
		#endregion

		#region Public Methods
		
		//public int CalculateByteSize(IProject project)
		//{
		//	if (this.Data?.Count <= 0)
		//		return 0;

		//	int size = 0;
		//	foreach (var entry in this)
		//	{
		//		size += entry.CalculateByteSize(project);
		//	}

		//	return size;
		//}

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
                            this.Data.AddLast(DataEntry.FromString(line, reader));
                        }
                    }
                }

            }

            return this;

        }

		public IEnumerator<DataEntry> GetEnumerator()
		{
			return ((IEnumerable<DataEntry>)this.Data).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<DataEntry>)this.Data).GetEnumerator();
		}

		#endregion

		#region Private Methods

		

		#endregion

	}
}
