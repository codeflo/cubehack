// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using CubeHack.Game;
using CubeHack.Tcp;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Linq;

namespace CubeHack.Client
{
    internal sealed class MainWindow
    {
        private GameWindow _gameWindow;
        private GameClient _gameClient;

        private bool _mouseLookActive = true;

        public void Run(string host)
        {
            if (host == null)
            {
                host = Microsoft.VisualBasic.Interaction.InputBox("Enter server address:\n\n(Leave blank for single player.)", "CubeHack", " ");
                if (host == null || host.Length == 0)
                {
                    return;
                }
            }

            Universe universe = null;

            _gameWindow = new GameWindow(1280, 720);

            try
            {
                _gameWindow.Title = "CubeHack";
                _gameWindow.VSync = VSyncMode.Adaptive;
                _gameWindow.WindowState = WindowState.Maximized;

                _gameWindow.Visible = true;

                _gameWindow.KeyDown += OnKeyDown;
                _gameWindow.Mouse.ButtonDown += OnMouseButtonDown;

                if (host == "localhost" || string.IsNullOrWhiteSpace(host))
                {
                    host = "localhost";
                    universe = new Universe(DataLoader.LoadMod("Core"));
                    new TcpServer(universe);
                }

                GameLoop.Post(() => Connect(host));
                GameLoop.RenderFrame += RenderFrame;
                GameLoop.Run(_gameWindow);
            }
            finally
            {
                _gameWindow.Dispose();
                _gameWindow = null;

                if (universe != null)
                {
                    universe.Dispose();
                }
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out System.Drawing.Point point);

        private async void Connect(string host)
        {
            var channel = new TcpChannel(host, TcpConstants.Port);
            await channel.ConnectAsync();
            var gameClient = new GameClient(channel);

            foreach (var texture in channel.ModData.Materials.Select(m => m.Texture))
            {
                TextureAtlas.Register(texture);
            }

            await TextureAtlas.BuildAsync();

            _gameClient = gameClient;
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
                    _gameClient.UpdateState(_gameWindow.Focused);

                    Renderer.Render(_gameClient, renderInfo.Width, renderInfo.Height);
                    UiRenderer.Render(renderInfo.Width, renderInfo.Height, _mouseLookActive, null);
                }
            }
            else
            {
                UiRenderer.Render(renderInfo.Width, renderInfo.Height, false, "Connecting...");
            }
        }
    }
}
