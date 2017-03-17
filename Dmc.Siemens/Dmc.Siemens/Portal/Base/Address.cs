using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dmc.Siemens.Portal.Base
{
	public struct Address
	{

		public Address(int byteOffset, int bitOffset)
		{
			this.ByteOffset = byteOffset;
			this.BitOffset = bitOffset;
		}

		public int ByteOffset { get; set; }

		public int BitOffset { get; set; }

	}
}
