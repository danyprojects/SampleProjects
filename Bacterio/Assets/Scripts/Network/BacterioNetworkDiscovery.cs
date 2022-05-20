using System;
using System.Net;
using Mirror;
using Mirror.Discovery;

namespace Bacterio.Network
{
    public class DiscoveryRequest : NetworkMessage
    {
        
    }

    public class DiscoveryResponse : NetworkMessage
    {
        public int PlayerId;
        public string RoomName;
    }

    public class BacterioNetworkDiscovery : NetworkDiscoveryBase<DiscoveryRequest, DiscoveryResponse>
    {
        public event Action<DiscoveryResponse, IPEndPoint> LANRoomFoundEvent = null;

        public DiscoveryResponse Response = null;

        protected override void ProcessClientRequest(DiscoveryRequest request, IPEndPoint endpoint)
        {
            //Only process if we have a response created
            if(Response != null)
                base.ProcessClientRequest(request, endpoint);
        }

        protected override DiscoveryResponse ProcessRequest(DiscoveryRequest request, IPEndPoint endpoint)
        {            
            return Response;
        }

        protected override DiscoveryRequest GetRequest()
        {
            return new DiscoveryRequest();
        }

        protected override void ProcessResponse(DiscoveryResponse response, IPEndPoint endpoint)
        {
            LANRoomFoundEvent?.Invoke(response, endpoint);
        }
    }
}
