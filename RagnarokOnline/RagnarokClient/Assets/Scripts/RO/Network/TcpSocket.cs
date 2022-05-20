using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


namespace RO.Network
{
    class TcpSocket
    {
        static TcpClient _tcpClient = null;
        static SslStream _sslStream = null;
        static volatile bool _cancelTasks = false;
        static SemaphoreSlim _writeTask = null, _readTask = null;
        public static SemaphoreSlim PacketOutSemaphore = new SemaphoreSlim(0);

        public static bool Connect(string server, int port)
        {
            try
            {
                Debug.Log("Connecting to server...");
                _tcpClient = new TcpClient();
                if (!_tcpClient.ConnectAsync(server, port).Wait(5000))
                    throw new Exception("Timed out on connecting to server");

                _sslStream = new SslStream(_tcpClient.GetStream(), false,
                    new RemoteCertificateValidationCallback(ValidateServerCertificate), null);

                if (!_sslStream.AuthenticateAsClientAsync(server).Wait(5000))
                    throw new Exception("Timed out on authenticating client");

                if (_sslStream.IsAuthenticated)
                    Debug.Log("Connected and authenticated");

                return true;
            }
            catch (Exception e)
            {
                Debug.Log("Failed to connect to server: " + e.Message);
                return false;
            }
        }

        public static void Disconnect()
        {
            _tcpClient?.Close();
            _cancelTasks = true;

            //Unblock write task. Packet semaphore is where writer will be blocked
            if (PacketOutSemaphore != null && PacketOutSemaphore.CurrentCount == 0)
                PacketOutSemaphore.Release();
            //Unblock read task. sslStream is where reader will be blocked
            _sslStream.Dispose();

            //Wait for tasks to close
            _readTask?.Wait();
            _writeTask?.Wait();
            Debug.Log("Connection and tasks closed");
        }

        public static void StartIOTasks()
        {
            _cancelTasks = false;
            _writeTask = new SemaphoreSlim(0);
            _readTask = new SemaphoreSlim(0);
            //Start the reader
            Task.Run(() => ReadPackets());
            //Start the writer
            Task.Run(() => WritePackets());
        }

        public static bool IsConnected
        {
            get
            {
                if (_tcpClient == null)
                    return false;
                return _tcpClient.Connected;
            }
        }

        static async void ReadPackets()
        {
            byte[] buffer = new byte[256];
            try
            {
                while (!_cancelTasks)
                {
                    int bytesRead = 0;

                    //Read the packet id
                    while (bytesRead < sizeof(short))
                        bytesRead += await _sslStream.ReadAsync(buffer, bytesRead, sizeof(short) - bytesRead);
                    int packetId = buffer[1];
                    packetId = (packetId << 8) | buffer[0];

                    Debug.Assert(packetId < (short)PacketIds.None, "Received invalid packet id: " + packetId.ToString());

                    int packetSize = PacketFactory.RcvPacketSizes[packetId - (short)PacketIds.RCV_Start];
                    //Debug.Log("Received packet ID - Size: " + packetId.ToString() + " - " + packetSize.ToString());

                    //If it's a dynamic packet then read how many bytes total from socket
                    if (packetSize == short.MaxValue)
                    {
                        while (bytesRead < sizeof(short) * 2) //size of id + dynamic length
                            bytesRead += await _sslStream.ReadAsync(buffer, bytesRead, sizeof(short) * 2 - bytesRead);
                        packetSize = buffer[3];
                        packetSize = (packetSize << 8) | buffer[2];
                    }

                    //Reads the remaining packet data until length is read
                    while (bytesRead < packetSize)
                        bytesRead += await _sslStream.ReadAsync(buffer, bytesRead, packetSize - bytesRead);

                    Debug.Assert(bytesRead == packetSize, "Something went wrong in reading packet id: " + packetId.ToString());

                    NetworkController.QueuePacketsIn.Enqueue(PacketFactory.PacketFromBytes[packetId - (short)PacketIds.RCV_Start](buffer));
                }
                throw new Exception("Cancelation request");
            }
            catch (Exception e)
            {
                Debug.Log("Read connection stopped. Exception " + e.StackTrace);
                _readTask.Release();
                NetworkController.QueuePacketsIn.Enqueue(new Internal_ConnectionClosed()); // notify game manager
            }
        }

        static async void WritePackets()
        {
            try
            {
                while (!_cancelTasks)
                {
                    await PacketOutSemaphore.WaitAsync();
                    // Because only the model is 1 to 1, we have guarantees that if we get back from semaphore await
                    // that queue has an element so try dequeue should always work
                    if (NetworkController.QueuePacketsOut.TryDequeue(out SND_Packet packetOut))
                    {
                        byte[] payload = packetOut.ToBytes();
                        await _sslStream.WriteAsync(payload, 0, payload.Length);
                    }
                }
                throw new Exception("cancelation request");
            }
            catch (Exception e)
            {
                Debug.Log("Write connection stopped. Exception:" + e.Message);
                _writeTask.Release();
            }
        }

        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
