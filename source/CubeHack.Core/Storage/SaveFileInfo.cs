// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using ProtoBuf;

namespace CubeHack.Storage
{
    /// <summary>
    /// Metadata about a save file.
    /// </summary>
    [ProtoContract]
    public sealed class SaveFileInfo
    {
        public const int CurrentVersion = 2;

        [ProtoMember(1)]
        public int Version;
    }
}
