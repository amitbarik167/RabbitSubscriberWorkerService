using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitSubscriberWorkerService.Contract.Messaging.RabbitMQ.vHost.TestHost;
using RabbitSubscriberWorkerService.Domain.Objects.Models.Messaging.RabbitMQ.Payloads;
using RabbitSubscriberWorkerService.Messaging.RabbitMQ.vHost.TestHost;
using RabbitSubscriberWorkerService.Messaging.Snippets;
using System.Text;

namespace RabbitSubscriberWorkerService.Impl
{
    public class PublishMessage : IPublishMessage
    {
        private readonly IRabbitMQChannel _rabbitMQChannel;
        private readonly string _rabbitMQDelayedExchangeName;

        private readonly string _ttlExpiration;
        private readonly ILogger<PublishMessage> _logger;
        private readonly short _maxReprocessingRetries;
        private readonly short _deadLetterRetryCount;
        private readonly short _xDelayInMilliSeconds;


        public PublishMessage(IRabbitMQChannel rabbitMQChannel, string rabbitMQDelayedExchangeName , string ttlExpiration, short maxReprocessingRetries, short deadLetterRetryCount, short xDelayInMilliSeconds, ILogger<PublishMessage> logger)
        {
            _rabbitMQChannel = rabbitMQChannel;
            _rabbitMQDelayedExchangeName = rabbitMQDelayedExchangeName;
            _ttlExpiration = ttlExpiration;
            _logger = logger;
            _maxReprocessingRetries = maxReprocessingRetries;
            _deadLetterRetryCount = deadLetterRetryCount;
            _xDelayInMilliSeconds = xDelayInMilliSeconds;
        }

        public void SendToQueue(TestModel testModel)
        {
            var json = JsonConvert.SerializeObject(testModel);
            var body = Encoding.UTF8.GetBytes(json);
            var dictionary = new Dictionary<string, object>
            {
                { "x-delay", _xDelayInMilliSeconds }
            };
            if (testModel.RetryCount >= _maxReprocessingRetries)
            {
                dictionary.Add("count", _deadLetterRetryCount);
            }
            else
            {
                dictionary.Add("count", testModel.RetryCount + 1);
            }
            var channel = _rabbitMQChannel.GetChannel();
            var props = channel.CreateBasicProperties();
            props.Headers = dictionary;
            props.ContentType = "application/json";
            props.DeliveryMode = (int)RabbitMQDeliveryModeEnum.Persistent;
            props.Expiration = _ttlExpiration;
            channel.BasicPublish(_rabbitMQDelayedExchangeName, "", true, props, body);
            _logger.LogInformation($"Message sent to queue through exchange:{_rabbitMQDelayedExchangeName}. Published message: {testModel.AsJsonString()}");
        }
    }
}