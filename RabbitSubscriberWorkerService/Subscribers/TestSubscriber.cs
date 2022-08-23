using RabbitMQ.Client.Events;
using System.Text;
using Microsoft.Extensions.Logging;
using RabbitSubscriberWorkerService.Messaging.RabbitMQ.vHost.TestHost;

namespace RetrievalSystem.DataIntake.Service.Service.Subscribers
{

    /// <summary>
    /// This subscriber will listen to Member queue
    /// </summary>
    public class TestSubscriber : RabbitSubscriber
    {
        private readonly ILogger<TestSubscriber> _logger;
        private readonly short _deadLetterCount;

        public TestSubscriber(IRabbitMQConnection rabbitMqConnection, string queue, short deadLetterCount, ILogger<TestSubscriber> logger)
            : base(rabbitMqConnection, queue, logger)
        {
            _logger = logger;
            _deadLetterCount = deadLetterCount;
        }

        /// <summary>
        /// SubscriberOnReceive method to receive messages from member queue
        /// </summary>
        /// <param name="e"></param>
        public override void SubscriberOnReceive(BasicDeliverEventArgs e)
        {
            var messageString = Encoding.UTF8.GetString(e.Body.ToArray());
            _logger.LogInformation(messageString);

            if (e.BasicProperties.IsHeadersPresent() && e.BasicProperties.Headers != null && e.BasicProperties.Headers != null && e.BasicProperties.Headers.TryGetValue("count", out object countValue) && (Convert.ToInt16(countValue) != _deadLetterCount))
            {
               // testModel.RetryCount = Convert.ToInt16(countValue);
            }
            else
            {
                //testModel.RetryCount = 0;
            }
        }
    }
}