// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

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
