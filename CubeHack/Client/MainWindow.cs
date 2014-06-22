// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using CubeHack.Game;
using CubeHack.Tcp;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeHack.Client
{
    sealed class MainWindow
    {
        GameWindow _gameWindow;
        GameConnection _gameConnection;

        volatile bool _willQuit = false;
        bool _mouseLookActive = false;

        public void Run()
        {
            string host = Microsoft.VisualBasic.Interaction.InputBox("Enter server address:\n\n(Leave blank for single player.)", "CubeHack", " ");
            if (host.Length == 0)
            {
                return;
            }

            Universe universe = null;

            _gameWindow = new GameWindow(1280, 720);

            try
            {
                _gameWindow.Title = "CubeHack";
                _gameWindow.VSync = VSyncMode.Adaptive;

                _gameWindow.Visible = true;

                _gameWindow.KeyDown += OnKeyDown;
                _gameWindow.Mouse.ButtonDown += OnMouseButtonDown;

                if (string.IsNullOrWhiteSpace(host))
                {
                    universe = new Universe(DataLoader.LoadMod("Core"));
                    host = "localhost";
                }

                new TcpServer(universe);
                var channel = new TcpChannel(host, TcpConstants.Port);
                channel.ConnectAsync().Wait();

                _gameConnection = new GameConnection(channel);

                TextureAtlas.Build(channel.ModData.Textures);

                while (true)
                {
                    if (ProcessEvents()) break;
                    Render();

                    if (ProcessEvents()) break;
                    _gameWindow.SwapBuffers();
                }
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

        bool ProcessEvents()
        {
            if (_willQuit) return true;

            if (_gameWindow != null)
            {
                _gameWindow.ProcessEvents();
            }

            return _gameWindow == null || _gameWindow.IsExiting;
        }

        void OnKeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (e.Key == OpenTK.Input.Key.F10)
            {
                _willQuit = true;
            }

            if (e.Key == OpenTK.Input.Key.Escape)
            {
                _mouseLookActive = !_mouseLookActive;
            }
        }

        void OnMouseButtonDown(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            if (e.Button == OpenTK.Input.MouseButton.Left)
            {
                if (!_mouseLookActive)
                {
                    _mouseLookActive = true;
                }
            }
        }

        void UpdateMouse()
        {
            if (_mouseLookActive && _gameWindow.Focused)
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
                        _gameConnection.MouseLook(x, y);
                    }
                }

                SetCursorPos(center.X, center.Y);
            }
            else
            {
                _gameWindow.CursorVisible = true;
            }
        }

        void Render()
        {
            if (_gameConnection != null)
            {
                using (_gameConnection.TakeRenderLock())
                {
                    UpdateMouse();
                    _gameConnection.UpdateState(_gameWindow.Focused, _mouseLookActive);

                    Renderer.Render(_gameConnection, _gameWindow.Width, _gameWindow.Height);
                    UiRenderer.Render(_gameWindow.Width, _gameWindow.Height, _mouseLookActive);
                }
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        static extern bool GetCursorPos(out System.Drawing.Point point);
    }
}
