using RabbitMQ.Client;
using System.Text;

namespace RabbitMQ_Chat
{
    class Program
    {
        static void Main(string[] args)
        {
            var connect = new ConnectionFactory()
            { HostName = "localhost", UserName = "account", Password = "accountpass", VirtualHost = "test" };

            using var connection = connect.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare("msgqueue", true, false, false, null);
            Console.WriteLine("Send message\n");
            while (connection.IsOpen)
            {
                Console.Write("Enter message: ");
                var message = Console.ReadLine();
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish("", "msgqueue", null, body);
            }
        }
    }
}