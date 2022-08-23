using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitSubscriberWorkerService.Messaging.RabbitMQ.vHost.TestHost
{
    /// <summary>
    /// Interface for RabbitMqChannel
    /// </summary>
    public interface IRabbitMQChannel : IDisposable
    {
        /// <summary>
        /// Method signature for GetChannel
        /// </summary>
        /// <returns></returns>
        IModel GetChannel();
    }
}


