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
                        Console.Write("Message received: {0}", consumerResponse);
                        Console.Write("Enter your message: ");
                        message = Console.ReadLine();
                        var body = Encoding.UTF8.GetBytes(message);
                        channel.BasicPublish("direct", "msg", basicProperties, body);
                    }

                };
                channel.BasicConsume("msgq", false, basicConsumer);
                Console.ReadLine();
            }

            //RunQueue();
        }

        private static void RunQueue()
        {
            ConnectionFactory connectionFactory = new ConnectionFactory();

            connectionFactory.Port = 5672;
            connectionFactory.HostName = "localhost";
            connectionFactory.UserName = "account";
            connectionFactory.Password = "accountpass";
            connectionFactory.VirtualHost = "test";

            IConnection connection = connectionFactory.CreateConnection();
            IModel channel = connection.CreateModel();

            channel.QueueDeclare("queues.msg", true, false, false, null);
            SendMessage(channel);

            channel.Close();
            connection.Close();
        }

        private static void SendMessage(IModel channel)
        {
            string responseQueue = channel.QueueDeclare().QueueName;

            string correlationId = Guid.NewGuid().ToString();
            string consumerResponse = null;

            IBasicProperties basicProperties = channel.CreateBasicProperties();
            basicProperties.ReplyTo = responseQueue;
            basicProperties.CorrelationId = correlationId;
            Console.Write("Enter your message: ");
            string message = Console.ReadLine();
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish("", "queues.msg", basicProperties, messageBytes);

            EventingBasicConsumer eventingBasicConsumer = new EventingBasicConsumer(channel);
            eventingBasicConsumer.Received += (sender, basicDeliveryEventArgs) =>
            {
                IBasicProperties props = basicDeliveryEventArgs.BasicProperties;
                if (props != null && props.CorrelationId == correlationId)
                {
                    var body = basicDeliveryEventArgs.Body.ToArray();
                    string response = Encoding.UTF8.GetString(body);
                    consumerResponse = response;
                }
                channel.BasicAck(basicDeliveryEventArgs.DeliveryTag, false);
                Console.WriteLine("Response: {0}", consumerResponse);
                Console.Write("Enter your message: ");
                message = Console.ReadLine();
                messageBytes = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish("", "queues.msg", basicProperties, messageBytes);
            };
            channel.BasicConsume(responseQueue, false, eventingBasicConsumer);
        }
    }
}