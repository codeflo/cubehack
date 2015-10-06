// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

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
