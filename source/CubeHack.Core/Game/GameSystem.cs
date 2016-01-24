// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

namespace CubeHack.Game
{
    internal abstract class GameSystem
    {
        protected GameSystem(GameHost host)
        {
            Host = host;
        }

        public GameHost Host { get; }
    }
}
