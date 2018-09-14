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
	public class PortalXmlTemplate : PortalXmlDocument
	{

		#region Properties / Fields

		private ObservableHashSet<ReplacementToken> _Tokens = new ObservableHashSet<ReplacementToken>();
		public ObservableHashSet<ReplacementToken> Tokens
		{
			get
			{
				return this._Tokens;
			}
		}

		protected static readonly Regex TokenRegex = new Regex(@"{(.*?)}", RegexOptions.Compiled);

		protected bool IsReplaced { get; set; } = false;

		#endregion

		#region Constructors

		public PortalXmlTemplate(string fileContents) : base(fileContents)
		{
		}

		public PortalXmlTemplate(FileInfo fileInfo) : base(fileInfo)
		{
		}

		#endregion

		#region Public Methods

		public static PortalXmlTemplate FromFile(string path, PortalXmlFileOptions options = PortalXmlFileOptions.InMemory)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			if (options == PortalXmlFileOptions.CopyLocal)
				throw new NotSupportedException("CopyLocal option not supported by PortalXmlTemplate");
			if (!File.Exists(path))
				throw new FileNotFoundException("Path: " + path);

			string contents = File.ReadAllText(path);

			PortalXmlTemplate document;
			if (options == PortalXmlFileOptions.InMemory)
			{
				document = new PortalXmlTemplate(contents);
			}
			else
			{
				document = new PortalXmlTemplate(new FileInfo(path));
			}

			var matches = PortalXmlTemplate.TokenRegex.Matches(contents);

			if (matches?.Count > 0)
			{
				foreach (var match in matches.Cast<Match>())
				{
					document.Tokens.Add(new ReplacementToken(match?.Value, match.Value));
				}
			}

			return document;
		}

		public PortalXmlTemplate ReplaceTokens(IEnumerable<ReplacementToken> tokens = null)
		{
			if (this.IsReplaced)
				return this;
			
			if (this.FileContents == null)
				this.FileContents = File.ReadAllText(this.LinkedFile.FullName);

			if (tokens != null && tokens.Any())
			{
				foreach (var token in tokens)
				{
					this.FileContents.Replace("{" + token.Key + "}", token.Value);
				}
			}
			
			foreach (var token in this.Tokens)
			{
				this.FileContents.Replace("{" + token.Key + "}", token.Value);
			}
			
			this.IsReplaced = true;

			return this;
		}

		public override void Export(string path)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			if (!FileHelpers.CheckValidFilePath(path, "xml"))
				throw new ArgumentException("Invalid file path for XML export", nameof(path));
			
			File.WriteAllText(path, this.ReplaceTokens().FileContents);
		}

		#endregion

	}
}
