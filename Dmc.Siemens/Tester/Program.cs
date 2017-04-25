using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tester
{
	class Program
	{
		static void Main(string[] args)
		{

			ITester[] testArray = new ITester[]
			{
				new AlarmworxTester()
			};

			foreach (var tester in testArray)
			{
				tester.Run();
			}
		}
	}
}
