using System;

namespace Dmc.Siemens.Common.Plc.Types
{
    public struct Constant<T> where T : struct
    {

        public Constant(T value, string name = null)
        {
            this._value = value;
            this.HasValue = true;
            this.Name = name;
        }

        public Constant(string name)
        {
            this._value = default;
            this.Name = name;
            this.HasValue = false;
        }

        private readonly T _value;
        public T Value
        {
            get
            {
                if (!this.HasValue)
                {
                    throw new InvalidOperationException("Cannot retrieve a constant with no value.");
                }
                return this._value;
            }
        }

        public string Name { get; private set; }
        public bool HasValue { get; private set; }

        public override bool Equals(object obj)
        {
            if (!this.HasValue) return obj == null;
            else if (obj == null) return false;
            return this.Value.Equals(obj);
        }

        public override int GetHashCode()
        {
			unchecked
			{
				var hash = (int)2166136261;
				if (this.HasValue)
					hash = (hash * 16777619) ^ this.Value.GetHashCode();
				if (this.Name != null)
					hash = (hash * 16777619) ^ this.Name.GetHashCode();

				return hash;
			}
        }

        public override string ToString()
        {
            return this.HasValue ? this.Value.ToString() : "";
        }

		public static implicit operator Constant<T>(T value)
		{
			return new Constant<T>(value);
		}

        public static explicit operator T(Constant<T> value)
        {
            return value.Value;
        }

    }
}
