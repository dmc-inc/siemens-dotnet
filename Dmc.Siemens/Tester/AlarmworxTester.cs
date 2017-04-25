using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Siemens.Common.Export;
using Dmc.Siemens.Common.Plc;
using Dmc.Siemens.Common.Plc.Interfaces;

namespace Tester
{
	public class AlarmworxTester : ITester
	{
		public void Run()
		{
			string path = @"C:\Projects\DMC\text.csv";

			DataBlock block = new DataBlock()
			{
				Name = "dbTest",
				Number = 4
			};
			block.Children.AddLast(new DataEntry("bTest1", DataType.BOOL, "Test comment 1"));
			block.Children.AddLast(new DataEntry("bTest2", DataType.BOOL, "Test comment 2"));
			block.Children.AddLast(new DataEntry("bTest3", DataType.BOOL, "Test comment 3"));
			block.Children.AddLast(new DataEntry("bTest4", DataType.BOOL, "Test comment 4"));
			var structure = new DataEntry("stTest1", DataType.STRUCT, "whatever");
			structure.Children.AddLast(new DataEntry("bTest5", DataType.BOOL, "Test comment 5"));
			block.Children.AddLast(structure);
			
			AlarmWorxConfiguration.Create(block, path, @"Kepware.KEPServerEx.V5\TestPlc.Whatever", null);
		}
	}
}
