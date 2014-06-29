using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
