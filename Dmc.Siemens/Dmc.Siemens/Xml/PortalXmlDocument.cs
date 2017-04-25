using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dmc.IO;
using Dmc.Wpf.Base;
using Dmc.Wpf.Collections;

namespace Dmc.Siemens.Xml
{
	public class PortalXmlDocument : NotifyPropertyChanged
	{
		
		#region Constructor

		internal PortalXmlDocument(FileInfo fileInfo)
		{
			this.LinkedFile = fileInfo;
		}

		internal PortalXmlDocument(string fileContents)
		{
			this.FileContents = fileContents;
		}

		#endregion

		#region Public Properties

		private ObservableDictionary<string, XmlTokenReplacement> _Tokens = new ObservableDictionary<string, XmlTokenReplacement>();
		public ObservableDictionary<string, XmlTokenReplacement> Tokens
		{
			get
			{
				return this._Tokens;
			}
			set
			{
				this.SetProperty(ref this._Tokens, value);
			}
		}

		#endregion

		#region Private Properties

		private FileInfo LinkedFile { get; }
		private string FileContents { get; }

		private static readonly Regex TokenRegex = new Regex(@"{(.*?)}", RegexOptions.Compiled);

		#endregion

		#region Public Methods

		public static PortalXmlDocument FromFile(string path, XmlTokenFileOptions options)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			if (!File.Exists(path))
				throw new FileNotFoundException("Path: " + path);

			string contents = File.ReadAllText(path);

			PortalXmlDocument document;
			if (options == XmlTokenFileOptions.InMemory)
			{
				document = new PortalXmlDocument(contents);
			}
			else
			{
				document = new PortalXmlDocument(new FileInfo(path));
			}
			
			var matches = PortalXmlDocument.TokenRegex.Matches(contents);

			if (matches?.Count > 0)
			{
				foreach (var match in matches.Cast<Match>())
				{
					document.Tokens.Add(match.Value, null);
				}
			}

			return document;
		}

		public void Export(string path)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			if (!FileHelpers.CheckValidFilePath(path, ".xml"))
				throw new ArgumentException("Invalid file path for XML export", nameof(path));
			if (this.Tokens.Any(t => t.Value?.Value == null))
				throw new InvalidOperationException("Cannot export " + nameof(PortalXmlDocument) + " with null replacement tokens");

			string result;
			if ((result = this.FileContents) == null)
				result = File.ReadAllText(this.LinkedFile.FullName);

			foreach (var token in this.Tokens)
			{
				result.Replace("{" + token.Key + "}", token.Value.Value);
			}

			File.WriteAllText(path, result);
		}

		#endregion

	}
}
