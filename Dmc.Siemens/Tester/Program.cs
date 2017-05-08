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

			//ITester[] testArray = new ITester[]
			//{
			//	new AlarmworxTester()
			//};

			//foreach (var tester in testArray)
			//{
			//	tester.Run();
			//}

			Program.TaskHelper();

			Console.ReadLine();
		}

		static async Task TaskHelper()
		{
			var task = Program.GetTrue();
			var task2 = Program.GetTrue();

			var result = await Task.WhenAll(task, task2);

			Console.ReadLine();
		}

		static async Task<bool> GetTrue()
		{
			await Task.Delay(5000);
			return true;
		}

	}
}
