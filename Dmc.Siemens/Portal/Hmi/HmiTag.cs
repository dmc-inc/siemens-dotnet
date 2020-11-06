using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Siemens.Common.Plc;
using Dmc.Siemens.Portal.Base;

namespace Dmc.Siemens.Portal.Hmi
{
	public class HmiTag : ITag
	{
		public string Name { get; set; }
		public DataType DataType { get; set; }
		public string Comment { get; set; }
		public ITagTable TagTable { get; set; }
	}
}
