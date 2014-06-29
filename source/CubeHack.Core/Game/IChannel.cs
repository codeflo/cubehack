// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using CubeHack.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeHack.Game
{
    public interface IChannel
    {
        Func<GameEvent, Task> OnGameEventAsync { get; set; }

        Task SendPlayerEventAsync(PlayerEvent playerEvent);

        ModData ModData { get; }
    }
}
