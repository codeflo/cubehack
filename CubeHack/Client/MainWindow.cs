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

        public void Run()
        {
            string host = Microsoft.VisualBasic.Interaction.InputBox("Enter server address:\n\n(Leave blank for single player.)", "CubeHack");

            Universe universe = null;

            _gameWindow = new GameWindow(1280, 720);

            try
            {
                _gameWindow.Title = "CubeHack -- Press F10 to quit";
                _gameWindow.VSync = VSyncMode.Adaptive;

                _gameWindow.Visible = true;

                _gameWindow.KeyDown += OnKeyDown;

                if (string.IsNullOrEmpty(host))
                {
                    universe = new Universe(DataLoader.LoadMod("Core"));
                    host = "localhost";
                }

                new TcpServer(universe);
                var channel = new TcpChannel("localhost", TcpConstants.Port);

                _gameConnection = new GameConnection(channel);

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

        private void OnKeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (e.Key == OpenTK.Input.Key.F10)
            {
                _willQuit = true;
            }
        }

        void Render()
        {
            if (_gameConnection != null)
            {
                using (_gameConnection.TakeRenderLock(_gameWindow.Focused))
                {
                    Renderer.Render(_gameConnection, _gameWindow.Width, _gameWindow.Height);
                }
            }
        }
    }
}
