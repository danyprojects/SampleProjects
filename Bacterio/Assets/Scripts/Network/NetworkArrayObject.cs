using Mirror;

namespace Bacterio.Network
{
    public abstract class NetworkArrayObject : NetworkBehaviour
    {
        public struct UniqueId
        {
            public static UniqueId invalid { get { return new UniqueId(Constants.INVALID_BACTERIA_INDEX, short.MinValue); } }

            //Marked as readonly so it's clear this not meant to be changed recklessly
            public readonly short index;
            public readonly short tag;

            public UniqueId(short index, short tag)
            {
                this.index = index;
                this.tag = tag;
            }
        }

        [SyncVar]
        public UniqueId _uniqueId;
    }
}
