// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using Newtonsoft.Json.Linq;

namespace CubeHack.EditorModel
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
