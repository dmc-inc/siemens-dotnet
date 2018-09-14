using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dmc.IO;
using Dmc.Wpf;
using Dmc.Wpf.Collections;

namespace Dmc.Siemens.Xml
{
	public abstract class PortalXmlDocument : NotifyPropertyChanged
	{
		
		#region Constructor
		
		protected PortalXmlDocument(FileInfo fileInfo)
		{
			this.LinkedFile = fileInfo;
		}

		protected PortalXmlDocument(string fileContents)
		{
			this.FileContents = fileContents;
		}

		#endregion

		#region Protected Properties

		protected FileInfo LinkedFile { get; }
		protected string FileContents { get; set; }
		
		#endregion

		#region Public Methods

		public abstract void Export(string path);

		#endregion

	}
}
