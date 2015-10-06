// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Game;
using OpenTK.Input;

namespace CubeHack.Client
{
    internal sealed class GameClient : AbstractGameClient
    {
        public GameClient(IChannel channel)
            : base(channel)
        {
        }

        public void UpdateState(bool hasFocus)
        {
            var keyboardState = Keyboard.GetState();
            var mouseState = Mouse.GetState();

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

                        case GameKey.Primary:
                            return mouseState.LeftButton == ButtonState.Pressed;

                        case GameKey.Secondary:
                            return mouseState.RightButton == ButtonState.Pressed;

                        default:
                            return false;
                    }
                });
        }
    }
}
