// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.FrontEnd.Ui.Framework.Controls;
using CubeHack.FrontEnd.Ui.Framework.Input;
using CubeHack.FrontEnd.Ui.Menu;
using System.Collections.Generic;

namespace CubeHack.FrontEnd
{
    internal sealed class GameControl : Control
    {
        private readonly MainMenu _mainMenu;

        public GameControl(MainMenu mainMenu)
        {
            _mainMenu = mainMenu;
        }

        protected override IEnumerable<Control> GetChildren()
        {
            yield return _mainMenu;
        }

        protected override MouseMode OnGetMouseMode()
        {
            return MouseMode.Grabbed;
        }
    }
}
