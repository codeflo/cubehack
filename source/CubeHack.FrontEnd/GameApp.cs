// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.FrontEnd.Graphics.Rendering;
using CubeHack.FrontEnd.Ui.Framework;
using CubeHack.FrontEnd.Ui.Framework.Input;
using CubeHack.Game;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace CubeHack.FrontEnd
{
    public sealed class GameApp
    {
        private readonly Ui.Framework.Input.KeyboardState _uiKeyboardState = new Ui.Framework.Input.KeyboardState();

        private readonly GameLoop _gameLoop;
        private readonly WorldRenderer _renderer;
        private readonly UiRenderer _uiRenderer;
        private readonly GameControl _gameControl;
        private readonly GameController _gameController;
        private readonly GameConnectionManager _connectionManager;

        private GameWindow _gameWindow;
        private OpenTK.Input.KeyboardState _keyboardState;
        private OpenTK.Input.MouseState _mouseState;
        private bool _wasWindowGrabbed = false;
        private System.Drawing.Point _nonGrabbedMousePosition;

        [DependencyInjected]
        internal GameApp(GameLoop gameLoop, WorldRenderer renderer, UiRenderer uiRenderer, GameControl gameControl, GameController gameController, GameConnectionManager connectionManager)
        {
            _gameLoop = gameLoop;
            _renderer = renderer;
            _uiRenderer = uiRenderer;
            _gameControl = gameControl;
            _gameController = gameController;
            _connectionManager = connectionManager;

            _gameController.IsKeyPressedCallback = IsKeyPressed;

            _gameLoop.Reset();
        }

        public void Run()
        {
            _gameWindow = new GameWindow(1280, 720);

            try
            {
                _gameWindow.Title = "CubeHack";
                _gameWindow.VSync = VSyncMode.Off;

                /* This sequence seems necessary to bring the window to the front reliably. */
                _gameWindow.WindowState = WindowState.Maximized;
                _gameWindow.WindowState = WindowState.Minimized;
                _gameWindow.Visible = true;
                _gameWindow.WindowState = WindowState.Maximized;

                _gameWindow.KeyDown += OnKeyDown;
                _gameWindow.KeyPress += OnKeyPress;
                _gameWindow.KeyUp += OnKeyUp;

                _gameLoop.RenderFrame += RenderFrame;
                try
                {
                    _gameLoop.Run(_gameWindow);
                }
                finally
                {
                    _gameLoop.RenderFrame -= RenderFrame;
                }
            }
            finally
            {
                _gameWindow.Dispose();
                _gameWindow = null;
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out System.Drawing.Point point);

        private bool IsKeyPressed(GameKey gameKey)
        {
            if (!_gameWindow.Focused) return false;

            switch (gameKey)
            {
                case GameKey.Jump:
                    return _keyboardState.IsKeyDown(OpenTK.Input.Key.Space);

                case GameKey.Forwards:
                    return _keyboardState.IsKeyDown(OpenTK.Input.Key.W);

                case GameKey.Left:
                    return _keyboardState.IsKeyDown(OpenTK.Input.Key.A);

                case GameKey.Backwards:
                    return _keyboardState.IsKeyDown(OpenTK.Input.Key.S);

                case GameKey.Right:
                    return _keyboardState.IsKeyDown(OpenTK.Input.Key.D);

                case GameKey.Primary:
                    return _mouseState.LeftButton == ButtonState.Pressed;

                case GameKey.Secondary:
                    return _mouseState.RightButton == ButtonState.Pressed;

                default:
                    return false;
            }
        }

        private void OnKeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (e.Key == OpenTK.Input.Key.F10)
            {
                _gameLoop.Quit();
            }
            else
            {
                var key = new Ui.Framework.Input.Key(e.Key.ToString());
                _uiKeyboardState[key] = true;
                _gameControl.HandleKeyDown(key);
            }
        }

        private void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            _gameControl.HandleTextInput(new string(e.KeyChar, 1));
        }

        private void OnKeyUp(object sender, KeyboardKeyEventArgs e)
        {
            _uiKeyboardState[new Ui.Framework.Input.Key(e.Key.ToString())] = false;
        }

        private void UpdateMouse()
        {
            bool isWindowGrabbed = _gameWindow.Focused && _gameControl.HandleGetMouseMode() == MouseMode.Grabbed;

            System.Drawing.Point point;
            GetCursorPos(out point);

            if (isWindowGrabbed)
            {
                if (!_wasWindowGrabbed)
                {
                    _nonGrabbedMousePosition = point;
                }

                var center = _gameWindow.PointToScreen(new System.Drawing.Point(_gameWindow.Width / 2, _gameWindow.Height / 2));

                // Use Win32 GetCursorPos/SetCursorPos to "grab" the mouse cursor and get "infinite" mouse movement.
                // This feels a bit jaggy; there must be a better way to implement this.

                if (_gameWindow.CursorVisible)
                {
                    _gameWindow.CursorVisible = false;
                }
                else
                {
                    var x = point.X - center.X;
                    var y = point.Y - center.Y;
                    _connectionManager.Client?.MouseLook(x, y);
                }

                SetCursorPos(center.X, center.Y);
            }
            else if (_wasWindowGrabbed)
            {
                SetCursorPos(_nonGrabbedMousePosition.X, _nonGrabbedMousePosition.Y);

                _gameWindow.CursorVisible = true;
            }

            _wasWindowGrabbed = isWindowGrabbed;
        }

        private void RenderFrame(RenderInfo renderInfo)
        {
            UpdateMouse();
            GL.Viewport(0, 0, renderInfo.Width, renderInfo.Height);

            _keyboardState = Keyboard.GetState();
            _mouseState = Mouse.GetState();

            var client = _connectionManager.Client;
            if (client != null && client.IsConnected)
            {
                using (client.TakeRenderLock())
                {
                    client.UpdateState();
                    _renderer.Render(client, renderInfo);
                }
            }
            else
            {
                GL.ClearColor(0f, 0f, 0f, 1f);
                GL.Clear(ClearBufferMask.ColorBufferBit);
            }

            _uiRenderer.Render(renderInfo, _wasWindowGrabbed ? MouseMode.Grabbed : MouseMode.Free, _gameControl, GetInputState());
        }

        private InputState GetInputState()
        {
            System.Drawing.Point mousePoint;
            GetCursorPos(out mousePoint);
            mousePoint = _gameWindow.PointToClient(mousePoint);

            var inputState = new InputState();
            inputState.Mouse = new Ui.Framework.Input.MouseState()
            {
                Position = new Point(mousePoint.X, mousePoint.Y),
                LeftButtonPressed = _mouseState.LeftButton == ButtonState.Pressed,
            };

            inputState.Keyboard = new Ui.Framework.Input.KeyboardState(_uiKeyboardState);

            return inputState;
        }
    }
}
