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
                byte[] serializedData;
                using (var ms = new MemoryStream())
                {
                    var bf = new BinaryFormatter();
                    bf.Serialize(ms, data);
                    serializedData = ms.ToArray();
                }

                socket.Send(serializedData);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to send message: {ex.Message}", ex);
            }
        }

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
