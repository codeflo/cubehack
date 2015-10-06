// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CubeHack.EditorModel
{
    public abstract class NotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetAndNotify<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            if (!object.Equals(field, value))
            {
                field = value;
                Notify(propertyName);
            }
        }

        protected void Notify([CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
