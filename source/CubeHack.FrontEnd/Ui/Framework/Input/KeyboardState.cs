// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using System.Collections.Generic;

namespace CubeHack.FrontEnd.Ui.Framework.Input
{
    internal sealed class KeyboardState
    {
        private readonly HashSet<Key> _pressedKeys;

        public KeyboardState()
        {
            _pressedKeys = new HashSet<Key>();
        }

        public KeyboardState(KeyboardState copy)
        {
            _pressedKeys = new HashSet<Key>(copy._pressedKeys);
        }

        public bool this[Key key]
        {
            get { return _pressedKeys.Contains(key); }

            set
            {
                if (value)
                {
                    _pressedKeys.Add(key);
                }
                else
                {
                    _pressedKeys.Remove(key);
                }
            }
        }
    }
}
