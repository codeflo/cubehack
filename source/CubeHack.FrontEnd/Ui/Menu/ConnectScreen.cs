// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Data;
using CubeHack.FrontEnd.Ui.Framework.Controls;
using CubeHack.FrontEnd.Ui.Framework.Drawing;
using CubeHack.FrontEnd.Ui.Framework.Input;

namespace CubeHack.FrontEnd.Ui.Menu
{
    internal sealed class ConnectScreen : Control
    {
        private readonly GameConnectionManager _connectionManager;

        private string _address = string.Empty;

        [DependencyInjected]
        public ConnectScreen(GameConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        protected override bool OnKeyDown(Key key)
        {
            if (key == Key.BackSpace)
            {
                if (_address.Length > 0) _address = _address.Substring(0, _address.Length - 1);
                return true;
            }

            if (key == Key.Enter)
            {
                if (!string.IsNullOrWhiteSpace(_address))
                {
                    var notAwaited = _connectionManager.ConnectAsync(_address);
                }

                return true;
            }

            return false;
        }

        protected override bool OnTextInput(string text)
        {
            _address += text;
            return true;
        }

        protected override void RenderForeground(Canvas canvas)
        {
            var text = "Enter address:";
            var style = new Font(30, new Color(0.5f, 0.5f, 0.5f));
            canvas.Print(
                style,
                0.5f * (canvas.Width - canvas.MeasureText(style, text)), canvas.Height * 0.5f - 15 - 40, text);

            text = _address + "_";
            style = new Font(30, new Color(1, 1, 1));
            canvas.Print(
                style,
                0.5f * (canvas.Width - canvas.MeasureText(style, text)), canvas.Height * 0.5f - 15 + 40, text);
        }
    }
}
