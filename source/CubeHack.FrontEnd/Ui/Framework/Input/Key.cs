// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using System;

namespace CubeHack.FrontEnd.Ui.Framework.Input
{
    public sealed class Key
    {
        /*
         * Only list the keys we really need *in code*.
         * The key rebinding dialog will also work with any key not listed here.
         */

        public static readonly Key Escape = new Key("Escape");
        public static readonly Key BackSpace = new Key("BackSpace");
        public static readonly Key Enter = new Key("Enter");

        public Key(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            Name = name;
        }

        /// <summary>
        /// Gets a readable name for the key, like "Escape". This can be used in keybinding dialogs.
        /// </summary>
        public string Name { get; }

        public static bool operator ==(Key a, Key b)
        {
            if (ReferenceEquals(a, null)) return ReferenceEquals(b, null);
            if (ReferenceEquals(b, null)) return false;
            return a.Name == b.Name;
        }

        public static bool operator !=(Key a, Key b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            var key = obj as Key;
            if (ReferenceEquals(key, null)) return false;
            return Name == key.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
