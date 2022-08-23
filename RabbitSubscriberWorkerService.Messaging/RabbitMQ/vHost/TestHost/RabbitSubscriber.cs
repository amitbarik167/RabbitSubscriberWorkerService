using System;
using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using RabbitSubscriberWorkerService.Messaging.Snippets;

namespace RabbitSubscriberWorkerService.Messaging.RabbitMQ.vHost.TestHost
{

    /// <summary>
    /// Abstract class RabbitSubscriber
    /// </summary>
    public abstract class RabbitSubscriber : IRabbitSubscriber, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IRabbitMQConnection _rabbitMqConnection;
        private IModel _subscriberChannel;
        private readonly string _queueName;
        protected RabbitSubscriber(IRabbitMQConnection rabbitMqConnection, string queueName, ILogger logger)
        {
            _rabbitMqConnection = rabbitMqConnection;
            _queueName = queueName;
            _logger = logger;
        }

        /// <summary>
        /// Subscribe virtual method 
        /// </summary>
        public virtual void Subscribe()
        {
            if (_subscriberChannel != null)
                return;

            var connection = _rabbitMqConnection.Connection;
            _subscriberChannel = connection.CreateModel();
            _subscriberChannel.BasicQos(0, 1, false);
            _subscriberChannel.ModelShutdown += OnModelShutdown;

            var consumer = new EventingBasicConsumer(_subscriberChannel);
            consumer.Registered += HandleSubscriberOnRegistered;
            consumer.Received += HandleSubscriberOnReceive;
            consumer.Unregistered += HandleSubscriberOnUnregistered;
            consumer.ConsumerCancelled += HandleSubscriberOnCancelled;
            consumer.Shutdown += HandleSubscriberOnShutdown;

            _subscriberChannel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);
        }

        /// <summary>
        /// Dispose Channel
        /// </summary>
        public void Dispose()
        {
            if (_subscriberChannel == null)
                return;

            _subscriberChannel.Close();
            _subscriberChannel.Dispose();
            _subscriberChannel = null;
        }

        /// <summary>
        /// Abstract method
        /// </summary>
        /// <param name="message"><see cref="BasicDeliverEventArgs"/></param>
        public abstract void SubscriberOnReceive(BasicDeliverEventArgs message);


        /// <summary>
        /// HandleSubscriberOnReceive 
        /// </summary>
        /// <param name="sender"><see cref="BasicDeliverEventArgs"/></param>
        /// <param name="e"></param>
        private void HandleSubscriberOnReceive(object sender, BasicDeliverEventArgs e)
        {
            string message = null;
            try
            {
                message = Encoding.UTF8.GetString(e.Body.ToArray());
                _logger.LogDebug($"Starting RecordNotificationSubscriber for: {message}");
                SubscriberOnReceive(e);
                _subscriberChannel.BasicAck(e.DeliveryTag, false);
            }
            catch (AlreadyClosedException ex)
            {
                _logger.LogError($"Subscriber: {GetType().Name} channel was closed. Message: {message} \r\n{ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Subscriber: {GetType().Name} encountered an unhandled exception. Message {message} \r\n{ex.Message}", ex);
                _subscriberChannel.BasicReject(e.DeliveryTag, false);
            }
            _logger.LogInformation($"RecordNotificationSubscriber complete for: {message}");
        }

        /// <summary>
        /// OnModelShutdown method event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnModelShutdown(object sender, ShutdownEventArgs e)
        {
            _logger.LogWarning($"Subscriber {GetType().Name}: RabbitMQ model/channel shutdown. Args: {e}");
        }

        /// <summary>
        /// HandleSubscriberOnRegistered event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleSubscriberOnRegistered(object sender, ConsumerEventArgs e)
        {
            _logger.LogDebug($"Subscriber: {GetType().Name} registered. Info: {GetLogInfo(sender)}");
        }

        /// <summary>
        /// HandleSubscriberOnUnregistered event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleSubscriberOnUnregistered(object sender, ConsumerEventArgs e)
        {
            _logger.LogDebug($"Subscriber: {GetType().Name} unregistered. Info: {GetLogInfo(sender)}");
        }

        /// <summary>
        /// HandleSubscriberOnCancelled event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleSubscriberOnCancelled(object sender, ConsumerEventArgs e)
        {
            _logger.LogDebug($"Subscriber: {GetType().Name} cancelled. Info: {GetLogInfo(sender)}");
        }

        /// <summary>
        /// HandleSubscriberOnShutdown event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleSubscriberOnShutdown(object sender, ShutdownEventArgs e)
        {
            _logger.LogDebug($"Subscriber: {GetType().Name} shutdown. Info: {GetLogInfo(sender)}");
        }

        /// <summary>
        /// GetLogInfo method
        /// </summary>
        /// <param name="sender"></param>
        /// <returns>string LogInfo</returns>
        private string GetLogInfo(object sender)
        {
            var consumer = sender as EventingBasicConsumer;
            var logInfo = new
            {
                IsRunning = consumer?.IsRunning,
                ConsumerTags = consumer?.ConsumerTags,
                ShutdownReason = consumer?.ShutdownReason
            };
            return logInfo.AsJsonString();
        }
    }
}
