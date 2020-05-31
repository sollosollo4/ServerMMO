using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Realm.Server
{
    class ServerObject
    {
        private int acceptPort; // Порт для "приема" новых клиентов
        public int receivePort; // Порт для приема сообщений от "подключенных клиентов"
        private Socket listeningSocket; // Сокет для "приема" новых клиентов
        private List<User> users = new List<User>(); // Список всех "подключенных" клиентов
        
        /// <summary>
        /// Запуск сервера
        /// </summary>
        public void StartServer()
        {
            Console.Write("Write port for accept users: ");
            acceptPort = Int32.Parse(Console.ReadLine());
            Console.Write("Write port for receive message: ");
            receivePort = Int32.Parse(Console.ReadLine());
            Console.WriteLine();

            try
            {
                listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp); //Создаем сокет

                Task listenTask = new Task(Listen); // Создаем отдельный поток для метода

                listenTask.Start(); // Запускаем поток
                listenTask.Wait(); // Ожидаем завершения потока
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in StartServer(): " + ex.Message);
            }
            finally
            {
                StopServer(); // Останавливаем сервер
            }
        }

        /// <summary>
        /// Ожидание "подключений" к серверу
        /// </summary>
        private void Listen()
        {
            try
            {
                IPEndPoint acceptIP = new IPEndPoint(IPAddress.Parse("0.0.0.0"), acceptPort); 
                // Создаем конечную точку на которую будут приходить сообщения

                listeningSocket.Bind(acceptIP);

                while (true)
                {
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    byte[] data = new byte[64];
                    EndPoint senderIP = new IPEndPoint(IPAddress.Any, 0);

                    do
                    {
                        bytes = listeningSocket.ReceiveFrom(data, ref senderIP);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (listeningSocket.Available > 0 && Regex.IsMatch(builder.ToString(), "\\bCODE:[NAME]\\b") == true); 
                    // Удостоверяемся, что все данные получены и сообщение содержит метку означающее новое подключение

                    IPEndPoint senderFullIP = senderIP as IPEndPoint;

                    // Добавляем пользователя в список "подключенных"
                    bool addNewUser = true;
                    bool firstUser = false;

                    if (users.Count == 0)
                    {
                        AddUser(senderFullIP, builder);
                        firstUser = true;
                        addNewUser = false;
                        Console.WriteLine("First connected {0}:{1} his name - {2}", senderFullIP.Address.ToString(), senderFullIP.Port.ToString(), builder.ToString());
                    }

                    if (firstUser == false)
                        for (int i = 0; i <= users.Count; i++)
                            if (users[i].FullInfoIP.Address.ToString() == senderFullIP.Address.ToString())
                                addNewUser = false;

                    if (addNewUser == true)
                    {
                        AddUser(senderFullIP, builder);
                        Console.WriteLine("Connected {0}:{1} his name - {2}", senderFullIP.Address, senderFullIP.Port, builder.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Listen(): " + ex.Message);
            }
            finally
            {
                StopServer();
            }
        }

        /// <summary>
        /// Рассылка сообщений всем пользователям кроме одного
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="address">Адрес отправителя ( на этот адрес не будет отправлено сообщение )</param>
        public void BroadcastMessage(string message, string address)
        {
            for (int i = 0; i < users.Count; i++)
                if (users[i].FullInfoIP.Address.ToString() != address)
                {
                    byte[] data = Encoding.Unicode.GetBytes(message);
                    listeningSocket.SendTo(data, users[i].FullInfoIP);
                }
        }

        /// <summary>
        /// Добавление клиента в список "подключенных"
        /// </summary>
        /// <param name="senderFullIP">Информация о новом клиенте</param>
        /// <param name="builder">Сообщение клиента</param>
        private void AddUser(IPEndPoint senderFullIP, StringBuilder builder)
        {
            User user = new User();
            user.Id = Guid.NewGuid().ToString();
            user.FullInfoIP = senderFullIP;
            user.Name = builder.ToString();
            users.Add(user);
            ClientObject client = new ClientObject(this, user, receivePort);
        }

        /// <summary>
        /// Запуск метода для прослушивания данного клиента в отдельном потоке
        /// </summary>
        /// <param name="client">Клиент для которого запускается отдельный поток</param>
        public void UserIsAdded(ClientObject client)
        {
            Task clientTask = new Task(client.Listen);
            clientTask.Start();
        }

        /// <summary>
        /// Остановка сервера
        /// </summary>
        private void StopServer()
        {
            if (listeningSocket != null)
            {
                listeningSocket.Shutdown(SocketShutdown.Both);
                listeningSocket.Close();
                listeningSocket = null;
            }
        }
    }
}
