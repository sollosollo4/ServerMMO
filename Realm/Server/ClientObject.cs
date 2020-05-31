using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Realm.Server
{
    class ClientObject
    {
        ServerObject server;
        User user;
        int receivePort; // Порт который будет принимать сообщения от данного клиента

        public ClientObject(ServerObject _server, User _user, int _port)
        {
            server = _server;
            user = _user;
            receivePort = _port;
            server.UserIsAdded(this);
        }

        /// <summary>
        /// Прием сообщений от конкретного пользователя
        /// </summary>
        public void Listen()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint receiveIP = new IPEndPoint(IPAddress.Parse("0.0.0.0"), receivePort);
            socket.Bind(receiveIP);
            while (true)
            {
                StringBuilder builder = new StringBuilder();
                int bytes = 0;
                byte[] data = new byte[64];

                EndPoint senderIP = new IPEndPoint(user.FullInfoIP.Address, user.FullInfoIP.Port);

                do
                {
                    bytes = socket.ReceiveFrom(data, ref senderIP);
                    builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                }
                while (socket.Available > 0);

                Console.WriteLine("{0}: {1}", user.Name, builder.ToString());
                server.BroadcastMessage(builder.ToString(), user.FullInfoIP.Address.ToString());
            }
        }
    }
}
