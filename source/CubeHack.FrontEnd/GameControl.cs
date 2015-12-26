// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.FrontEnd.Ui.Framework.Controls;
using CubeHack.FrontEnd.Ui.Framework.Input;
using CubeHack.FrontEnd.Ui.Hud;
using CubeHack.FrontEnd.Ui.Menu;
using System.Collections.Generic;

namespace CubeHack.FrontEnd
{
    internal sealed class GameControl : Control
    {
        private readonly MainMenu _mainMenu;
        private readonly HudControl _hudControl;

        [DependencyInjected]
        public GameControl(MainMenu mainMenu, HudControl hudControl)
        {
            _mainMenu = mainMenu;
            _hudControl = hudControl;
        }

        protected override IEnumerable<Control> GetChildren()
        {
            yield return _mainMenu;
            yield return _hudControl;
        }

        protected override MouseMode OnGetMouseMode()
        {
            return MouseMode.Grabbed;
        }
    }
}
