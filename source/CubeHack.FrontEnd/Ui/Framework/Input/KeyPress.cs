// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

namespace CubeHack.FrontEnd.Ui.Framework.Input
{
    internal sealed class KeyPress
    {
        public KeyPress(Key key, string text)
        {
            Key = key;
            Text = text;
        }

        public Key Key { get; }

        public string Text { get; }

        public bool IsHandled { get; set; }
    }
}
