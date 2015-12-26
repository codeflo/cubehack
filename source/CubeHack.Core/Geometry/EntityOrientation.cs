// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Util;
using ProtoBuf;
using System;

namespace CubeHack.Geometry
{
    [ProtoContract]
    public struct EntityOrientation
    {
        /// <summary>
        /// The horizontal viewing angle, in radians.
        /// </summary>
        [ProtoMember(1)]
        public double Horizontal;

        /// <summary>
        /// The vertical viewing angle, in radians.
        /// </summary>
        [ProtoMember(2)]
        public double Vertical;

        public EntityOrientation(double horizontal, double vertical)
        {
            Horizontal = horizontal;
            Vertical = vertical;
        }

        public static explicit operator EntityOffset(EntityOrientation o)
        {
            double z = -Math.Cos(o.Horizontal) * Math.Cos(-o.Vertical);
            double x = -Math.Sin(o.Horizontal) * Math.Cos(-o.Vertical);
            double y = Math.Sin(-o.Vertical);

            return new EntityOffset(x, y, z);
        }

        public static bool operator ==(EntityOrientation a, EntityOrientation b)
        {
            return a.Horizontal == b.Horizontal && a.Vertical == b.Vertical;
        }

        public static bool operator !=(EntityOrientation a, EntityOrientation b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is EntityOrientation && (EntityOrientation)obj == this;
        }

        public override int GetHashCode()
        {
            return HashCalculator.Value[Horizontal][Vertical];
        }
    }
}
