using ProtoBuf;

namespace CubeHack.Game
{
    [ProtoContract]
    public class EntityData
    {
        [ProtoMember(1)]
        public PositionData PositionData;

        [ProtoMember(2)]
        public int? ModelIndex;
    }
}
