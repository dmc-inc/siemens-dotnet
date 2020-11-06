﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Dmc.Siemens.Common.Base;
using Dmc.Siemens.Common.Interfaces;
using Dmc.Siemens.Common.Plc.Interfaces;
using Dmc.Siemens.Portal.Base;

namespace Dmc.Siemens.Common.Plc
{
    public class DataBlock : DataEntity, IBlock
    {

        private bool _isOptimized;
        [SourceMetadata("S7_Optimized_Access", '=', "[^\"']+")]
        public bool IsOptimized
        {
            get => this._isOptimized;
            set => this.SetProperty(ref this._isOptimized, value);
        }

        private bool _isInstance;
        public bool IsInstance
        {
            get => this._isInstance;
            set => this.SetProperty(ref this._isInstance, value);
        }

        private bool _isDataType;
        public bool IsDataType
        {
            get => this._isDataType;
            set => this.SetProperty(ref this._isDataType, value);
        }

        private string _instanceName;
        public string InstanceName
        {
            get => this._instanceName;
            set => this.SetProperty(ref this._instanceName, value);
        }

        private string _title;
        [SourceMetadata("TITLE", '=')]
        public string Title
        {
            get => this._title;
            set => this.SetProperty(ref this._title, value);
        }

        public BlockType Type => BlockType.DataBlock;

        public override string DataHeader => this.IsOptimized ? "VAR" : "STRUCT";

        public override void ReadXml(XmlReader reader)
		{
			throw new NotImplementedException();
		}

		public override void WriteXml(XmlWriter writer)
		{
			throw new NotImplementedException();
		}

		public override void Export(string path)
		{
			throw new NotImplementedException();
		}

	}
}
