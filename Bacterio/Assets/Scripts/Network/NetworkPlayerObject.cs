using System;
using Mirror;

namespace Bacterio.Network
{
    public abstract class NetworkPlayerObject : NetworkBehaviour
    {
        public static event Action<NetworkPlayerObject> PlayerObjectSpawned;
        public static event Action<NetworkPlayerObject> PlayerObjectDespawned;
        public static event Action<NetworkPlayerObject> PlayerObjectGotAuthority;


        public struct UniqueId
        {
            public int _clientToken;
            public int _index;
        }

        [SyncVar]
        public UniqueId _uniqueId;

        public PlayableType _playableType;

        public override void OnStartClient()
        {
            base.OnStartClient();

            PlayerObjectSpawned?.Invoke(this);
        }

        public override void OnStopClient()
        {
            PlayerObjectDespawned?.Invoke(this);

            base.OnStopClient();
        }

        public override void OnStartAuthority()
        {
            PlayerObjectGotAuthority?.Invoke(this);
            base.OnStartAuthority();
        }
    }
}
