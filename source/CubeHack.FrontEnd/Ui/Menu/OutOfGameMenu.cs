// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Data;
using CubeHack.FrontEnd.Ui.Framework.Controls;
using CubeHack.FrontEnd.Ui.Framework.Drawing;
using CubeHack.FrontEnd.Ui.Framework.Input;
using CubeHack.FrontEnd.Ui.Framework.Properties;
using System;
using System.Collections.Generic;

namespace CubeHack.FrontEnd.Ui.Menu
{
    internal sealed class OutOfGameMenu : Control
    {
        private readonly GameConnectionManager _connectionManager;

        private readonly ConnectScreen _connectScreen;

        private readonly Button _startGameButton;
        private readonly Button _connectButton;
        private Control _activeScreen = null;

        [DependencyInjected]
        public OutOfGameMenu(GameConnectionManager connectionManager, ConnectScreen connectScreen)
        {
            _connectionManager = connectionManager;
            _connectScreen = connectScreen;

            _startGameButton = new Button
            {
                Top = Property.Get(20f),
                Left = Property.Get(20f),
                Text = Property.Get("Start Game"),
            };
            _startGameButton.Click += OnStartGameButtonClick;

            _connectButton = new Button
            {
                Top = Property.Get(20f),
                Left = Property.Get(2 * 20f + Button.Width),
                Text = Property.Get("Connect"),
            };
            _connectButton.Click += () => _activeScreen = connectScreen;
        }

        protected override MouseMode OnGetMouseMode()
        {
            return MouseMode.Free;
        }

        protected override IEnumerable<Control> GetChildren()
        {
            if (!_connectionManager.IsConnecting && !_connectionManager.IsConnected)
            {
                yield return _activeScreen;

                yield return _startGameButton;
                yield return _connectButton;
            }
        }

        protected override void RenderForeground(Canvas canvas)
        {
            if (_connectionManager.IsConnecting)
            {
                var status = "Connecting...";
                var style = new Font(30, new Color(1, 1, 1)) { Animation = FontAnimation.Wave };
                canvas.Print(
                    style,
                    0.5f * (canvas.Width - canvas.MeasureText(style, status)), canvas.Height * 0.5f - 15, status);
            }
        }

        private void OnConnectButtonClick()
        {
            throw new NotImplementedException();
        }

        private async void OnStartGameButtonClick()
        {
            _activeScreen = null;
            await _connectionManager.LoadGameAsync(Storage.SaveDirectory.DebugGame);
        }
    }
}
