
using System.Linq;
using System;
using RabbitSubscriberWorkerService.Messaging.RabbitMQ.vHost.TestHost;

namespace RabbitSubscriberWorkerService.SubscriberManagement
{

    /// <summary>
    /// SubscriptionManager class
    /// </summary>
    public class SubscriptionManager : ISubscriptionManager
    {
        private readonly IServiceProvider _serviceProvider;
        public SubscriptionManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }


        /// <summary>
        /// Implement InitializeSubscriptions. Uses reflection to get all the subscribers implementing RabbitSubscriber and then invokes Subscribe virtual method to register for onreceive events
        /// </summary>
        public void InitializeSubscriptions()
        {
            var subscribers =
               System.Reflection.Assembly.GetExecutingAssembly().GetTypes()
                   .Where(w => !w.IsAbstract && w.IsSubclassOf(typeof(RabbitSubscriber)));

            var subscribersList = subscribers.Select(s => (IRabbitSubscriber)_serviceProvider.GetService(typeof(IRabbitSubscriber)));
            foreach (var subscriber in subscribersList)
            {
                subscriber.Subscribe();
            }
        }

    }
}
