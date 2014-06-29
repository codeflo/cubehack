using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeHack.DataModel
{
    public abstract class ValueItem<T> : Item
    {
        private T _value;
        public T Value
        {
            get { return _value; }
            set { SetAndNotify(ref _value, value); }
        }

        public override JToken Save()
        {
            if (_value == null)
            {
                return null;
            }

            return JToken.FromObject(_value);
        }

        public override void Load(JToken data)
        {
            Value = data.ToObject<T>();
        }

        public override object GetObject()
        {
            return _value;
        }
    }
}
