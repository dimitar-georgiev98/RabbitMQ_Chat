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
            using (var connection = connect.CreateConnection())
            using (var channel = connection.CreateModel())
            { 

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (sender, BasicDeliveryEventArgs) =>
                {
                    var body = BasicDeliveryEventArgs.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    channel.BasicAck(BasicDeliveryEventArgs.DeliveryTag, false);
                    var routingKey = BasicDeliveryEventArgs.RoutingKey;
                    Console.WriteLine("Message: {0}", message);
                    Console.Write("Enter reply: ");
                };
                channel.BasicConsume("msgq", false, consumer);
                Console.ReadLine();
            }
        }
    }
}