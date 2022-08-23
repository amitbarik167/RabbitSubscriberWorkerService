using RabbitSubscriberWorkerService.Domain.Objects.Models.Messaging.RabbitMQ.Payloads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitSubscriberWorkerService.Contract.Messaging.RabbitMQ.vHost.TestHost
{
    /// <summary>
    /// Publish Message
    /// </summary>
    public interface IPublishMessage
    {

        /// <summary>
        /// Method signature for SendToQueue : Provider requeue process
        /// </summary>
        /// <param name="provider"><see cref="Provider"/></param>
        void SendToQueue(TestModel testModel);

    }
}