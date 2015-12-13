﻿// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.FrontEnd.Ui.Framework;
using CubeHack.FrontEnd.Ui.Framework.Drawing;
using CubeHack.FrontEnd.Ui.Framework.Input;
using CubeHack.Game;
using CubeHack.Tcp;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Linq;
using System.Threading;

namespace CubeHack.FrontEnd
{
    public sealed class GameApp : IGameController
    {
        private static GameApp _instance;

        private readonly Ui.Framework.Input.KeyboardState _uiKeyboardState = new Ui.Framework.Input.KeyboardState();
        private readonly GameControl _gameControl = new GameControl();

        private GameWindow _gameWindow;
        private GameClient _gameClient;
        private OpenTK.Input.KeyboardState _keyboardState;
        private OpenTK.Input.MouseState _mouseState;
        private bool _wasWindowGrabbed = false;
        private System.Drawing.Point _nonGrabbedMousePosition;
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
                    _gameWindow.KeyUp += OnKeyUp;

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
            else
            {
                var key = new Ui.Framework.Input.Key(e.Key.ToString());
                _uiKeyboardState[key] = true;
                _gameControl.HandleKeyPress(new KeyPress(key, null));
            }
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
                    _gameClient?.MouseLook(x, y);
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

            if (_gameClient != null)
            {
                using (_gameClient.TakeRenderLock())
                {
                    _gameClient.UpdateState();

                    Renderer.Render(_gameClient, renderInfo.Width, renderInfo.Height);
                    UiRenderer.Render(renderInfo, _gameControl, null, GetInputState());
                }
            }
            else
            {
                GL.ClearColor(0f, 0f, 0f, 1f);
                GL.Clear(ClearBufferMask.ColorBufferBit);
                UiRenderer.Render(renderInfo, _gameControl, _statusText ?? "Not connected", GetInputState());
            }
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