using System;

namespace RO.Network
{
    sealed class PacketDistributer
    {
        private static Action<RCV_Packet>[] _distributerCallbacks = new Action<RCV_Packet>[(int)PacketIds.RCV_End - (int)PacketIds.SND_End + 2];

        public static void RegisterCallback(PacketIds packetId, Action<RCV_Packet> action)
        {
            _distributerCallbacks[(short)packetId - (int)PacketIds.SND_End] = action;
        }

        public static void Distribute(RCV_Packet packet)
        {
            _distributerCallbacks[packet.PacketId - (int)PacketIds.SND_End](packet);
        }

        static PacketDistributer()
        {

        }
    }
}
