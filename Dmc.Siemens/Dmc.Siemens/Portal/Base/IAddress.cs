using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMC.Siemens.Portal.Base
{
	public interface IAddress
	{

		int ByteOffset { get; set; }

		int BitOffset { get; set; }

	}
}
