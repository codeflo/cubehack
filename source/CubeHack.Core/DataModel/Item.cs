// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace CubeHack.DataModel
{
    public abstract class Item : NotifyPropertyChanged
    {
        public const int ExpansionLimit = 5;

        private bool _isExpanded;

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { SetAndNotify(ref _isExpanded, value); }
        }

        public static Item Create(Type type)
        {
            if (type == typeof(string))
            {
                return new StringItem();
            }
            if (type == typeof(float))
            {
                return new FloatItem();
            }
            if (type == typeof(double))
            {
                return new DoubleItem();
            }
            else if (type.GetCustomAttributes(typeof(CubeHack.Data.EditorDataAttribute), false).Length != 0)
            {
                return new ObjectItem(type);
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>) && type.GetGenericArguments()[0] == typeof(string))
            {
                return new DictionaryItem(type.GetGenericArguments()[1]);
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                return new ListItem(type.GetGenericArguments()[0]);
            }
            else
            {
                throw new ArgumentException("type");
            }
        }

        public abstract JToken Save();

        public abstract void Load(JToken data);

        public abstract object GetObject();
    }
}
