// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using System;
using System.Threading.Tasks;

namespace CubeHack.Game
{
    public interface IChannel : IDisposable
    {
        Func<GameEvent, Task> OnGameEventAsync { get; set; }

        ModData ModData { get; }

        Task SendPlayerEventAsync(PlayerEvent playerEvent);
    }
}
