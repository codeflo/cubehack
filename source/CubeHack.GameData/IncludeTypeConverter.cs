// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeHack.GameData
{
    public class IncludeTypeConverter : TypeConverter
    {
        public static Func<string, object> Resolver { get; set; }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (Resolver != null && value is string)
            {
                var r = Resolver(((string)value).Trim());
                return r;
            }

            return base.ConvertFrom(value);
        }
    }
}
