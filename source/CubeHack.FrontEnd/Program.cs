// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Util;

namespace CubeHack.FrontEnd
{
    internal static class Program
    {
        public static void Main()
        {
            using (var container = new DependencyInjectionContainer())
            {
                var gameApp = container.Resolve<GameApp>();
                gameApp.Run();
            }
        }
    }
}
