using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RabbitMQ_Chat_Consumer
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
            Console.WriteLine("Received messages\n");
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (sender, BasicDeliveryEventArgs) =>
            {
                var body = BasicDeliveryEventArgs.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                Console.WriteLine("Received message: {0}", message);
            };
            channel.BasicConsume("msgqueue", true, consumer);
            Console.ReadLine();
        }
    }
}