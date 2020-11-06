using System;

namespace Dmc.Siemens.Common.Base
{
    [Serializable]
	public class SiemensException : Exception
	{

		public SiemensException(string message) : base(message)
		{
		}

		public SiemensException(string message, Exception innerException) : base(message, innerException)
		{
		}

        public SiemensException()
        {
        }
    }
}
