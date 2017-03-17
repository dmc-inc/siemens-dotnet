using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dmc.Siemens.Base
{
	public class SiemensException : Exception
	{

		public SiemensException(string message) : base(message)
		{
		}

		public SiemensException(string message, Exception innerException) : base(message, innerException)
		{
		}

	}
}
