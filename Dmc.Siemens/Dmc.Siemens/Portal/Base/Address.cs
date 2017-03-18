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

		public int ByteOffset { get; private set; }

		public int BitOffset { get; private set; }

		public static Address operator +(Address address, Address addressOffset)
		{
			return new Address(address.ByteOffset + addressOffset.ByteOffset, address.BitOffset + addressOffset.BitOffset);
		}

		public static Address operator +(Address address, int byteOffset)
		{
			return new Address(address.ByteOffset + byteOffset, address.BitOffset);
		}

	}
}
