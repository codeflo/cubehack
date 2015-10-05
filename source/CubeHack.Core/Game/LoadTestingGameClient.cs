// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

namespace CubeHack.Game
{
    public class LoadTestingGameClient : AbstractGameClient
    {
        public LoadTestingGameClient(IChannel channel)
            : base(channel)
        {
        }

        internal override void OnSendPlayerEvents()
        {
            UpdateState(k => k == GameKey.Jump);
        }
    }
}
