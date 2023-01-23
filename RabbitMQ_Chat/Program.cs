using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RabbitMQ_Chat
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
                channel.ExchangeDeclare(exchange: "direct", type: "direct", true, false);
                var routingKey = "msg";
                string consumerResponse = null;

                IBasicProperties basicProperties = channel.CreateBasicProperties();
                basicProperties.ReplyTo = routingKey;

                Console.Write("Enter your message: ");
                string message = Console.ReadLine();
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish("direct", "msg", basicProperties, body);

                EventingBasicConsumer basicConsumer = new EventingBasicConsumer(channel);
                basicConsumer.Received += (sender, BasicDeliveryEventArgs) =>
                {
                    IBasicConsumer consumer = (IBasicConsumer)sender;
                    if (consumer != null)
                    {
                        Console.WriteLine("Message received: {0}", consumerResponse);
                        Console.Write("Enter your message: ");
                        message = Console.ReadLine();
                        var body = Encoding.UTF8.GetBytes(message);
                        channel.BasicPublish("direct", "msg", basicProperties, body);
                    }
                };
                channel.BasicConsume("msgq", false, basicConsumer);
                Console.ReadLine();
            }
        }
    }
}