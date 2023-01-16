using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RabbitMQ_Chat
{
    internal class Program
    {
        static void Main(string[] args)
        {
            RunQueue();
        }

        private static void RunQueue()
        {
            ConnectionFactory connectionFactory = new ConnectionFactory();

            connectionFactory.Port = 5672;
            connectionFactory.HostName= "localhost";
            connectionFactory.UserName= "account";
            connectionFactory.Password= "accountpass";
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
                    string response = Encoding.UTF8.GetString(basicDeliveryEventArgs.Body.Span);
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