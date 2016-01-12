// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Util;
using System;

namespace CubeHack.Randomization
{
    /// <summary>
    /// Implements a deterministic smooth noise function that can be used to create infinite terrain or textures.
    /// <para>
    /// For any given seed, repeated evaluation of the noise function at the same coordinate will produce the same output.
    /// </para>
    /// <para>
    /// The noise is "smooth" in the sense that it's band-limited in terms of frequency.
    /// For images, it's more intuitive to think about the inverse frequency, or 1/frequency, which we call "feature size".
    /// The size of the features visible in the noise is limited to a certain range specified in the constructor.
    /// </para>
    /// <para>
    /// See http://graphics.pixar.com/library/WaveletNoise/paper.pdf for the theory, although our implementation is considerably less sophisticated.
    /// </para>
    /// </summary>
    internal class Noise2D
    {
        private const double CutOffDistance = 2;

        private readonly HashCalculator _seedHash;
        private readonly double _scaleFactor;
        private readonly double _blurFactor;
        private readonly double _blurNormalizationFactor;

        /// <summary>
        /// Initializes a new instance of the <see cref="Noise2D"/> class.
        /// <para>
        /// Performance is proportional to (largeFeatureScale/smallFeatureScale)^2, so if you need a wide range of feature sizes,
        /// it's better to add multiple "frequency bands" of noise together.
        /// </para>
        /// </summary>
        /// <param name="seed">A seed value to initialize the random number generator.</param>
        /// <param name="smallFeatureSize">A value proportional to the size of the smallest details created by the noise.</param>
        /// <param name="largeFeatureSize">A value proportional to the size of the largest details created by the noise.</param>
        public Noise2D(uint seed, double smallFeatureSize, double largeFeatureSize)
        {
            _seedHash = HashCalculator.FromSeed(seed);
            _scaleFactor = 1.0 / smallFeatureSize;
            _blurFactor = largeFeatureSize / smallFeatureSize;

            /* The blurred sampling is multiplied by this factor to make sure that the coefficients add up to the same value. */
            _blurNormalizationFactor = 1.0 / (_blurFactor * _blurFactor);
        }

        public double this[double x, double y]
        {
            get
            {
                x *= _scaleFactor;
                y *= _scaleFactor;

                double value = 0;

                double x0, x1, y0, y1;

                /* We implement a high-pass filter by subtracting a blurred version of the noise from itself. */

                x0 = Math.Ceiling(x - _blurFactor * CutOffDistance);
                y0 = Math.Ceiling(y - _blurFactor * CutOffDistance);
                x1 = Math.Floor(x + _blurFactor * CutOffDistance);
                y1 = Math.Floor(y + _blurFactor * CutOffDistance);
                for (var xi = x0; xi <= x1; ++xi)
                {
                    var squaredDistanceX = ExtraMath.Square(x - xi);

                    for (var yi = y0; yi <= y1; ++yi)
                    {
                        var squaredDistance = ExtraMath.Square(y - yi) + squaredDistanceX;
                        value -= GetWeight(squaredDistance * _blurNormalizationFactor) * GetValue(_seedHash[xi][yi]);
                    }
                }

                value *= _blurNormalizationFactor;

                /* We implement a low-pass filter by simply only sampling the noise function at certain intervals. */

                x0 = Math.Ceiling(x - CutOffDistance);
                y0 = Math.Ceiling(y - CutOffDistance);
                x1 = Math.Floor(x + CutOffDistance);
                y1 = Math.Floor(y + CutOffDistance);
                for (var xi = x0; xi <= x1; ++xi)
                {
                    var squaredDistanceX = ExtraMath.Square(x - xi);

                    for (var yi = y0; yi <= y1; ++yi)
                    {
                        var squaredDistance = ExtraMath.Square(y - yi) + squaredDistanceX;
                        value += GetWeight(squaredDistance) * GetValue(_seedHash[xi][yi]);
                    }
                }

                return value;
            }
        }

        private static double GetWeight(double squaredDistance)
        {
            /*
             * If d is the distance and d^2 is squaredDistance, this computes (d^2 - 2)^2/4,
             * which approximates e^(-d^2) for 0 <= d <= 2.
             *
             * In other words, this approximates a gaussian blur filter.
             */

            if (squaredDistance >= 2) return 0;
            double d = squaredDistance - 2;
            return 0.25 * d * d;
        }

        private static double GetValue(HashCalculator hash)
        {
            /* Returns a deterministic "random" value in the range [-1, 1]. */
            return hash.GetHashCode() * (-1.0 / int.MinValue);
        }
    }
}
