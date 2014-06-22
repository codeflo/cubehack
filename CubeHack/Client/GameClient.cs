// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using CubeHack.Game;
using CubeHack.GameData;
using CubeHack.Util;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeHack.Client
{
    sealed class GameClient : AbstractGameClient
    {
        public GameClient(IChannel channel)
            : base(channel)
        {
        }

        public void UpdateState(bool hasFocus)
        {
            var keyboardState = Keyboard.GetState();
            UpdateState(gameKey =>
                {
                    if (!hasFocus)
                    {
                        return false;
                    }

                    switch (gameKey)
                    {
                        case GameKey.Jump:
                            return keyboardState.IsKeyDown(Key.Space);
                        case GameKey.Forwards:
                            return keyboardState.IsKeyDown(Key.W);
                        case GameKey.Left:
                            return keyboardState.IsKeyDown(Key.A);
                        case GameKey.Backwards:
                            return keyboardState.IsKeyDown(Key.S);
                        case GameKey.Right:
                            return keyboardState.IsKeyDown(Key.D);
                        default:
                            return false;
                    }
                });
        }
    }
}
