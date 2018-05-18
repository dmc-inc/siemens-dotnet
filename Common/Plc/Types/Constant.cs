using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dmc.Siemens.Common.Plc.Types
{
    public struct Constant<T> where T : struct
    {

        #region Constructors

        public Constant(T value, string name = null)
        {
            this._Value = value;
            this._HasValue = true;
            this._Name = name;
        }

        public Constant(string name)
        {
            this._Value = default(T);
            this._Name = name;
            this._HasValue = false;
        }

        #endregion

        #region Public Properties

        private T _Value;
        public T Value
        {
            get
            {
                if (!this.HasValue)
                {
                    throw new InvalidOperationException("Cannot retrieve a constant with no value.");
                }
                return _Value;
            }
        }

        private string _Name;
        public string Name
        {
            get
            {
                return this._Name;
            }
            private set
            {
                this._Name = value;
            }
        }

        private bool _HasValue;
        public bool HasValue
        {
            get
            {
                return this._HasValue;
            }
            private set
            {
                this._HasValue = value;
            }
        }

        #endregion

        #region Public Methods

        public override bool Equals(object obj)
        {
            if (!this.HasValue) return (obj == null);
            else if (obj == null) return false;
            return this.Value.Equals(obj);
        }

        public override int GetHashCode()
        {
			unchecked
			{
				int hash = (int)2166136261;
				if (this.HasValue)
					hash = (hash * 16777619) ^ this.Value.GetHashCode();
				if (this.Name != null)
					hash = (hash * 16777619) ^ this.Name.GetHashCode();

				return hash;
			}
        }

        public override string ToString()
        {
            return HasValue ? this.Value.ToString() : "";
        }

		public static implicit operator Constant<T>(T value)
		{
			return new Constant<T>(value);
		}

        public static explicit operator T(Constant<T> value)
        {
            return value.Value;
        }

        #endregion

    }
}
