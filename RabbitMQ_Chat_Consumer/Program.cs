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
                Console.WriteLine("No messages yet!");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (sender, BasicDeliverEventArgs) =>
                {
                    var body = BasicDeliverEventArgs.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    channel.BasicAck(BasicDeliverEventArgs.DeliveryTag, false);
                    var routingKey = BasicDeliverEventArgs.RoutingKey;
                    Console.WriteLine("Message received: {0}", message);
                    Console.Write("Enter your reply: ");
                };
                channel.BasicConsume("msgq", false, consumer);
                Console.ReadLine();
            }

            /*
            ConnectionFactory connectionFactory = new ConnectionFactory();

            connectionFactory.Port = 5672;
            connectionFactory.HostName = "localhost";
            connectionFactory.UserName = "account";
            connectionFactory.Password = "accountpass";
            connectionFactory.VirtualHost = "test";

            IConnection connection = connectionFactory.CreateConnection();
            IModel channel = connection.CreateModel();
            channel.BasicQos(0, 1, false);

            EventingBasicConsumer eventingBasicConsumer = new EventingBasicConsumer(channel);
            eventingBasicConsumer.Received += (sender, basicDeliveryEventArgs) =>
            {
                var body = basicDeliveryEventArgs.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);
                channel.BasicAck(basicDeliveryEventArgs.DeliveryTag, false);
                Console.Write("Message: {0} {1}", message, "Enter your message: ");
                string response = Console.ReadLine();
                IBasicProperties basicProperties = channel.CreateBasicProperties();
                basicProperties.CorrelationId = basicDeliveryEventArgs.BasicProperties.CorrelationId;
                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                channel.BasicPublish("", basicDeliveryEventArgs.BasicProperties.ReplyTo, basicProperties, responseBytes);
            };
            channel.BasicConsume("queues.msg", false, eventingBasicConsumer);
            */
        }
    }
}