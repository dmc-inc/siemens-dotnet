using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Siemens.Portal.Base;

namespace Dmc.Siemens.Portal.Hmi
{
	public class HmiTagTable : ITagTable
	{
		
		public string Name { get; set; }

		public IEnumerable<HmiTag> HmiTags { get; }

	}
}
