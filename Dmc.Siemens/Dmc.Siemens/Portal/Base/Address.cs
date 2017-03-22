using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dmc.Siemens.Portal.Base
{
	public struct Address
	{

		public Address(int @byte, int bit = 0)
		{
			this.Byte = @byte;
			this.Bit = bit;
		}

		public int Byte { get; private set; }

		public int Bit { get; private set; }

		public static Address operator +(Address address, Address addressOffset)
		{
			int bitOffset = address.Bit + addressOffset.Bit;
			int byteOffset = address.Byte + addressOffset.Byte + (bitOffset / 8);

			return new Address(byteOffset, bitOffset % 8);
		}

		public static Address operator +(Address address, int byteOffset)
		{
			return new Address(address.Byte + byteOffset, address.Bit);
		}

	}
}
