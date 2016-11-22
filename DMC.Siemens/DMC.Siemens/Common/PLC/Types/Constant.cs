using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMC.Siemens.Common.PLC.Types
{
    public class Constant<T> where T : struct
    {

        #region Constructors

        public Constant()
        {
        }

        public Constant(T value)
        {
            this.Value = value;
        }

        public Constant(string name)
        {
            this.Name = name;
        }

        #endregion

        #region Public Properties

        public T? Value { get; set; }

        public string Name { get; set; }

        public bool HasValue
        {
            get
            {
                return this.Value.HasValue;
            }
        }

        #endregion

    }
}
