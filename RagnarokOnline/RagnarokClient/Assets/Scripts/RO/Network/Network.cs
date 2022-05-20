using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace RO.Network
{
    sealed class NetworkController
    {
        //public static readonly string SERVER_IP = "rounity.dynip.sapo.pt";
        public static readonly string SERVER_IP = "127.0.0.1";
        public const int SERVER_PORT = 9999; // 7777;

        //Dequeued elements are ready to be distributed by the mainthread using line below
        //Networking.DistributerFunctions[PacketIn.packetId](PacketIn.packetStruct);
        public static ConcurrentQueue<RCV_Packet> QueuePacketsIn = new ConcurrentQueue<RCV_Packet>();

        //dequeued elements need to be executed and will return a struct of PacketOut which is ready to be sent,
        //to be run by external thread
        public static ConcurrentQueue<SND_Packet> QueuePacketsOut = new ConcurrentQueue<SND_Packet>();

        public static void SendPacket<T>(T packet) where T : SND_Packet
        {
            QueuePacketsOut.Enqueue(packet);
            TcpSocket.PacketOutSemaphore.Release();
        }

        public static bool IsConnected()
        {
            return TcpSocket.IsConnected;
        }

        public static bool StartTcp()
        {
#if !STANDALONE_CLIENT

            if (TcpSocket.IsConnected)
                TcpSocket.Disconnect();
            if (!TcpSocket.Connect(SERVER_IP, SERVER_PORT))
                return false;

            //Starts write and read packet tasks in a different context than unity main thread
            Task.Run(() =>
            {
                TcpSocket.StartIOTasks();
            }).ConfigureAwait(false).GetAwaiter();

#endif

            return true;
        }
    }
}
