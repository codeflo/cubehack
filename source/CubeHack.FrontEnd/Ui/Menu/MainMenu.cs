// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Data;
using CubeHack.FrontEnd.Ui.Framework;
using CubeHack.FrontEnd.Ui.Framework.Controls;
using CubeHack.FrontEnd.Ui.Framework.Properties;
using System.Collections.Generic;

namespace CubeHack.FrontEnd.Ui.Menu
{
    internal class MainMenu : Control
    {
        private AnimatedProperty _backgroundOpacity;

        private Button _continueButton;

        public MainMenu()
        {
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
                Top = Property.Get(100f),
            };
        }

        public bool IsVisible { get; set; }

        protected override void RenderBackground(Canvas canvas)
        {
            canvas.DrawRectangle(new Color(0f, 0f, 0f, _backgroundOpacity.Value), 0, 0, canvas.Width, canvas.Height);
        }

        protected override IEnumerable<Control> GetChildren()
        {
            yield return _continueButton;
        }
    }
}
