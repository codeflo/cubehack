// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Util;
using System;

namespace CubeHack.FrontEnd.Ui.Framework.Properties
{
    internal class AnimatedProperty : IProperty<float>
    {
        private GameTime _valueTime = GameTime.Now();
        private float _value;

        public AnimatedProperty(float value)
        {
            _value = value;
        }

        public AnimatedProperty()
        {
        }

        public IProperty<float> AnimationSpeed { get; set; } = Property.Get(1f);

        public IProperty<float> TargetValue { get; set; }

        public float Value
        {
            get
            {
                UpdateValueTime();
                return _value;
            }
        }

        private void UpdateValueTime()
        {
            var deltaTime = (float)GameTime.Update(ref _valueTime).Seconds;
            var animationSpeed = AnimationSpeed?.Value ?? 0;
            var targetValue = TargetValue?.Value ?? 0;

            if (!(animationSpeed > 0)) return;

            var deltaValue = targetValue - _value;
            var deltaValueAbs = Math.Abs(deltaValue);
            var deltaValueSign = Math.Sign(deltaValue);
            var possibleDeltaValue = deltaTime * animationSpeed;

            if (possibleDeltaValue >= deltaValueAbs)
            {
                _value = targetValue;
            }
            else
            {
                _value += deltaValueSign * possibleDeltaValue;
            }
        }
    }
}
