using System;
using RabbitMQ.Client;

namespace RabbitSubscriberWorkerService.Messaging.RabbitMQ.vHost.TestHost
{
    /// <summary>
    /// Interface IRabbitMQConnection
    /// </summary>
    public interface IRabbitMQConnection : IDisposable
    {
        /// <summary>
        /// Mthod signature
        /// </summary>
        IConnection Connection { get; }
    }
}
