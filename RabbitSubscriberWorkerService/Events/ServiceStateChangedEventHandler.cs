using System;

namespace RabbitSubscriberWorkerService.Events
{
    public delegate void ServiceStateChangedEventHandler(ServiceStateChangedEventArgs e);

    /// <summary>
    /// ServiceState enum
    /// </summary>
    public enum ServiceState
    {
        Started,
        Stopped,
        Paused
    }

    /// <summary>
    /// ServiceStateChangedEventArgs
    /// </summary>
    public class ServiceStateChangedEventArgs : EventArgs
    {
        public ServiceState ServiceState { get; }

        public ServiceStateChangedEventArgs(ServiceState serviceState)
        {
            this.ServiceState = serviceState;
        }
    }

}
