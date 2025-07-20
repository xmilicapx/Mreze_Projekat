using System;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Core
{
    public static class NetworkHelper
    {
        public static void SendMessage(Socket socket, object data)
        {
            try
            {
                // Serialize the data
                byte[] serializedData;
                using (var ms = new MemoryStream())
                {
                    var bf = new BinaryFormatter();
                    bf.Serialize(ms, data);
                    serializedData = ms.ToArray();
                }

                // Send the length of the message first
                byte[] lengthPrefix = BitConverter.GetBytes(serializedData.Length);
                socket.Send(lengthPrefix);

                // Send the actual message
                socket.Send(serializedData);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to send message: {ex.Message}", ex);
            }
        }

        public static T ReceiveMessage<T>(Socket socket)
        {
            try
            {
                // Receive the length prefix
                byte[] lengthBytes = new byte[4];
                int totalReceived = 0;
                while (totalReceived < 4)
                {
                    int received = socket.Receive(lengthBytes, totalReceived, 4 - totalReceived, SocketFlags.None);
                    if (received == 0)
                        throw new Exception("Connection closed by remote host");
                    totalReceived += received;
                }

                int messageLength = BitConverter.ToInt32(lengthBytes, 0);

                // Receive the actual message
                byte[] messageBytes = new byte[messageLength];
                totalReceived = 0;
                while (totalReceived < messageLength)
                {
                    int received = socket.Receive(messageBytes, totalReceived, messageLength - totalReceived, SocketFlags.None);
                    if (received == 0)
                        throw new Exception("Connection closed by remote host");
                    totalReceived += received;
                }

                // Deserialize the message
                using (var ms = new MemoryStream(messageBytes))
                {
                    var bf = new BinaryFormatter();
                    return (T)bf.Deserialize(ms);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to receive message: {ex.Message}", ex);
            }
        }

        // Keep this method for UDP messages which are self-contained
        public static T DeserializeObject<T>(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                var bf = new BinaryFormatter();
                return (T)bf.Deserialize(ms);
            }
        }
    }
}