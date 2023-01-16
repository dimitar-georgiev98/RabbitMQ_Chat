using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RabbitMQ_Chat_Consumer
{
    internal class Program
    {
        static void Main(string[] args)
        {
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
                string message = Encoding.UTF8.GetString(basicDeliveryEventArgs.Body.Span);
                channel.BasicAck(basicDeliveryEventArgs.DeliveryTag, false);
                Console.Write("Message: {0} {1}", message, "Enter your message: ");
                string response = Console.ReadLine();
                IBasicProperties basicProperties = channel.CreateBasicProperties();
                basicProperties.CorrelationId = basicDeliveryEventArgs.BasicProperties.CorrelationId;
                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                channel.BasicPublish("", basicDeliveryEventArgs.BasicProperties.ReplyTo, basicProperties, responseBytes);
            };
            channel.BasicConsume("queues.msg", false, eventingBasicConsumer);
        }
    }
}