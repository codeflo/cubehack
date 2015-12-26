// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Data;
using CubeHack.FrontEnd.Ui.Framework.Controls;
using CubeHack.FrontEnd.Ui.Framework.Drawing;
using CubeHack.FrontEnd.Ui.Framework.Input;
using CubeHack.FrontEnd.Ui.Framework.Properties;
using System.Collections.Generic;

namespace CubeHack.FrontEnd.Ui.Menu
{
    internal sealed class PauseMenu : Control
    {
        private readonly AnimatedProperty _backgroundOpacity;
        private readonly GameConnectionManager _connectionManager;

        private readonly Button _continueButton;
        private readonly Button _disconnectButton;

        [DependencyInjected]
        public PauseMenu(GameConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;

            _backgroundOpacity = new AnimatedProperty
            {
                TargetValue = DelegateProperty.Get(() => IsVisible ? 0.75f : 0f),
                AnimationSpeed = Property.Get(8f),
            };

            var buttonLeft = new AnimatedProperty(-Button.Width - 1)
            {
                TargetValue = DelegateProperty.Get(() => IsVisible ? 25f : -Button.Width - 1),
                AnimationSpeed = Property.Get(16 * Button.Width),
            };

            _continueButton = new Button
            {
                Text = Property.Get("Continue"),
                Left = buttonLeft,
                Top = Property.Get(60f),
            };

            _disconnectButton = new Button
            {
                Text = Property.Get("Disconnect"),
                Left = buttonLeft,
                Top = Property.Get(100f),
            };

            _continueButton.Click += () =>
            {
                IsVisible = false;
            };

            _disconnectButton.Click += () =>
            {
                _connectionManager.Disconnect();
            };
        }

        public bool IsVisible { get; private set; }

        protected override MouseMode OnGetMouseMode()
        {
            return IsVisible ? MouseMode.Free : MouseMode.Any;
        }

        protected override bool OnKeyDown(Key key)
        {
            if (key == Key.Escape)
            {
                IsVisible = !IsVisible;
                return true;
            }

            return false;
        }

        protected override IEnumerable<Control> GetChildren()
        {
            yield return _continueButton;
            yield return _disconnectButton;
        }

        protected override void RenderBackground(Canvas canvas)
        {
            canvas.DrawRectangle(new Color(0f, 0f, 0f, _backgroundOpacity.Value), 0, 0, canvas.Width, canvas.Height);
        }
    }
}
