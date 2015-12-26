// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.FrontEnd.Ui.Framework.Controls;
using System.Collections.Generic;

namespace CubeHack.FrontEnd.Ui.Menu
{
    internal class MainMenu : Control
    {
        private readonly GameConnectionManager _connectionManager;

        private readonly OutOfGameMenu _outOfGameMenu;
        private readonly PauseMenu _pauseMenu;

        [DependencyInjected]
        public MainMenu(GameConnectionManager connectionManager, OutOfGameMenu outOfGameMenu, PauseMenu pauseMenu)
        {
            _connectionManager = connectionManager;
            _outOfGameMenu = outOfGameMenu;
            _pauseMenu = pauseMenu;
        }

        protected override IEnumerable<Control> GetChildren()
        {
            if (_connectionManager.IsConnected)
            {
                yield return _pauseMenu;
            }
            else
            {
                yield return _outOfGameMenu;
            }
        }
    }
}
