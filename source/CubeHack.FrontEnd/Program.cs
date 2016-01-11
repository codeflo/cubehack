// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Util;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace CubeHack.FrontEnd
{
    internal static class Program
    {
        [STAThread]
        public static void Main()
        {
            try
            {
                ThreadPoolConfiguration.Init();

                using (var container = new DependencyInjectionContainer())
                {
                    var gameApp = container.Resolve<GameApp>();
                    gameApp.Run();
                }
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }

                MessageBox.Show(ex.ToString(), "CubeHack", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
