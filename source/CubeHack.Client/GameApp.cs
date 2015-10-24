// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Client.UiFramework;
using CubeHack.Game;
using CubeHack.Tcp;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Linq;
using System.Threading;

namespace CubeHack.Client
{
    public sealed class GameApp : IGameController
    {
        private static GameApp _instance;
        private GameWindow _gameWindow;
        private GameClient _gameClient;
        private KeyboardState _keyboardState;
        private MouseState _mouseState;
        private bool _mouseLookActive = true;
        private string _statusText;

        public GameApp()
        {
            GameLoop.Reset();
            CharMap = new CharMap();
            TextureAtlas = new TextureAtlas();
            Renderer = new Renderer();
        }

        public static GameApp Instance => _instance;

        internal CharMap CharMap { get; }

        internal TextureAtlas TextureAtlas { get; }

        internal Renderer Renderer { get; }

        public void Run()
        {
            if (Interlocked.CompareExchange(ref _instance, this, null) != null)
            {
                throw new InvalidOperationException();
            }

            try
            {
                _gameWindow = new GameWindow(1280, 720);

                try
                {
                    _gameWindow.Title = "CubeHack";
                    _gameWindow.VSync = VSyncMode.Adaptive;

                    /* This sequence seems necessary to bring the window to the front reliably. */
                    _gameWindow.WindowState = WindowState.Maximized;
                    _gameWindow.WindowState = WindowState.Minimized;
                    _gameWindow.Visible = true;
                    _gameWindow.WindowState = WindowState.Maximized;

                    _gameWindow.KeyDown += OnKeyDown;
                    _gameWindow.Mouse.ButtonDown += OnMouseButtonDown;

                    GameLoop.RenderFrame += RenderFrame;
                    try
                    {
                        GameLoop.Run(_gameWindow);
                    }
                    finally
                    {
                        GameLoop.RenderFrame -= RenderFrame;
                    }
                }
                finally
                {
                    _gameWindow.Dispose();
                    _gameWindow = null;

                    _gameClient?.Dispose();
                    _gameClient = null;
                }
            }
            finally
            {
                Volatile.Write(ref _instance, null);
            }
        }

        public void Connect(string host, int port)
        {
            if (string.IsNullOrWhiteSpace(host)) host = "localhost";

            GameLoop.Post(
                async () =>
                {
                    _statusText = "Connecting...";
                    var channel = new TcpChannel(host, port);
                    await channel.ConnectAsync();
                    ConnectInternal(channel);
                });
        }

        public void Connect(IChannel channel)
        {
            GameLoop.Post(() => ConnectInternal(channel));
        }

        public bool IsKeyPressed(GameKey gameKey)
        {
            if (!_gameWindow.Focused) return false;

            switch (gameKey)
            {
                case GameKey.Jump:
                    return _keyboardState.IsKeyDown(Key.Space);

                case GameKey.Forwards:
                    return _keyboardState.IsKeyDown(Key.W);

                case GameKey.Left:
                    return _keyboardState.IsKeyDown(Key.A);

                case GameKey.Backwards:
                    return _keyboardState.IsKeyDown(Key.S);

                case GameKey.Right:
                    return _keyboardState.IsKeyDown(Key.D);

                case GameKey.Primary:
                    return _mouseState.LeftButton == ButtonState.Pressed;

                case GameKey.Secondary:
                    return _mouseState.RightButton == ButtonState.Pressed;

                default:
                    return false;
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out System.Drawing.Point point);

        private async void ConnectInternal(IChannel channel)
        {
            var gameClient = new GameClient(this, channel);

            _statusText = "Loading textures...";
            foreach (var texture in channel.ModData.Materials.Select(m => m.Texture))
            {
                TextureAtlas.Register(texture);
            }

            await TextureAtlas.BuildAsync();

            _gameClient = gameClient;
            _statusText = null;
        }

        private void OnKeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (e.Key == OpenTK.Input.Key.F10)
            {
                GameLoop.Quit();
            }

            if (e.Key == OpenTK.Input.Key.Escape)
            {
                _mouseLookActive = !_mouseLookActive;
            }
        }

        private void OnMouseButtonDown(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            if (e.Button == OpenTK.Input.MouseButton.Left)
            {
                if (!_mouseLookActive)
                {
                    _mouseLookActive = true;
                }
            }
        }

        private void UpdateMouse()
        {
            if (_mouseLookActive && _gameWindow.Focused && _gameClient != null)
            {
                var center = _gameWindow.PointToScreen(new System.Drawing.Point(_gameWindow.Width / 2, _gameWindow.Height / 2));

                // Use Win32 GetCursorPos/SetCursorPos to "grab" the mouse cursor and get "infinite" mouse movement.
                // This feels a bit jaggy; there must be a better way to implement this.

                if (_gameWindow.CursorVisible)
                {
                    _gameWindow.CursorVisible = false;
                }
                else
                {
                    System.Drawing.Point point;
                    if (GetCursorPos(out point))
                    {
                        var x = point.X - center.X;
                        var y = point.Y - center.Y;
                        _gameClient.MouseLook(x, y);
                    }
                }

                SetCursorPos(center.X, center.Y);
            }
            else
            {
                _gameWindow.CursorVisible = true;
            }
        }

        private void RenderFrame(RenderInfo renderInfo)
        {
            UpdateMouse();
            GL.Viewport(0, 0, renderInfo.Width, renderInfo.Height);

            if (_gameClient != null)
            {
                using (_gameClient.TakeRenderLock())
                {
                    _keyboardState = Keyboard.GetState();
                    _mouseState = Mouse.GetState();
                    _gameClient.UpdateState();

                    Renderer.Render(_gameClient, renderInfo.Width, renderInfo.Height);
                    UiRenderer.Render(renderInfo, _mouseLookActive, null);
                }
            }
            else
            {
                UiRenderer.Render(renderInfo, false, _statusText ?? "Not connected");
            }
        }
    }
}
