// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Game;
using System;

namespace CubeHack.FrontEnd
{
    internal class GameController : IGameController
    {
        [DependencyInjected]
        public GameController()
        {
        }

        public Func<GameKey, bool> IsKeyPressedCallback { get; set; }

        public bool IsKeyPressed(GameKey key)
        {
            return IsKeyPressedCallback?.Invoke(key) ?? false;
        }
    }
}
