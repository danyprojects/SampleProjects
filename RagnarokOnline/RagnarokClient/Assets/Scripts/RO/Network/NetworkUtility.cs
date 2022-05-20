using System.Text;

namespace RO.Network
{
    public interface SND_Packet
    {
        byte[] ToBytes();
        short PacketId { get; }
    }

    public interface RCV_Packet
    {
        void FromBytes(ref byte[] buffer);
        short PacketId { get; }
    }

    public sealed class NetworkUtility
    {
        public static void AppendNumber(byte num, ref byte[] buffer, ref int startIndex)
        {
            buffer[startIndex] = num;
            startIndex++;
        }
        public static void AppendNumber(short num, ref byte[] buffer, ref int startIndex)
        {
            buffer[startIndex] = (byte)(num & 0xFF);
            buffer[startIndex + 1] = (byte)((num & 0xFF00) >> 8);
            startIndex += 2;
        }
        public static void AppendNumber(int num, ref byte[] buffer, ref int startIndex)
        {
            for (int i = 0; i < 4; i++)
            {
                buffer[startIndex + i] = (byte)(num & 0xFF);
                num = num >> 8;
            }
            startIndex += 4;
        }
        public static void AppendString(string text, ref byte[] buffer, ref int startIndex)
        {
            Encoding.ASCII.GetBytes(text, 0, text.Length, buffer, startIndex);
            startIndex += text.Length;
        }
        public static void AppendFixedString(string text, ref byte[] buffer, ref int startIndex, int fixedSize)
        {
            Encoding.ASCII.GetBytes(text, 0, text.Length, buffer, startIndex);
            startIndex += fixedSize;
        }

        public static void ExtractNumber(out byte num, ref byte[] buffer, ref int bufferIndex)
        {
            num = buffer[bufferIndex];
            bufferIndex++;
        }
        public static void ExtractNumber(out short num, ref byte[] buffer, ref int bufferIndex)
        {
            num = buffer[bufferIndex];
            num |= (short)(buffer[bufferIndex + 1] << 8);
            bufferIndex += 2;

        }
        public static void ExtractNumber(out ushort num, ref byte[] buffer, ref int bufferIndex)
        {
            num = buffer[bufferIndex];
            num |= (ushort)(buffer[bufferIndex + 1] << 8);
            bufferIndex += 2;
        }

        public static void ExtractNumber(out int num, ref byte[] buffer, ref int bufferIndex)
        {
            num = buffer[bufferIndex];
            for (int i = 1; i < 4; i++)
                num |= buffer[bufferIndex + i] << (8 * i);
            bufferIndex += 4;
        }

        public static void ExtractNumber(out uint num, ref byte[] buffer, ref int bufferIndex)
        {
            num = buffer[bufferIndex];
            for (int i = 1; i < 4; i++)
                num |= (uint)buffer[bufferIndex + i] << (8 * i);
            bufferIndex += 4;
        }

        public static void ExtractString(out string text, ref byte[] buffer, ref int bufferIndex, int packetSize)
        {
            int i;
            for (i = bufferIndex; i < packetSize; i++)
                if (buffer[i] == '\0')
                    break;
            text = Encoding.ASCII.GetString(buffer, bufferIndex, i);
            bufferIndex += text.Length;
        }
        public static void ExtractFixedString(out string text, ref byte[] buffer, ref int bufferIndex, short fixedSize)
        {
            text = Encoding.ASCII.GetString(buffer, bufferIndex, fixedSize);
            bufferIndex += fixedSize;
        }
    }
}
