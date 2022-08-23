namespace RabbitSubscriberWorkerService.Domain.Objects
{

    /// <summary>
    /// AppSettings Class
    /// </summary>
    public class AppSettingsConfig
    {

        /// <summary>
        /// Application Name
        /// </summary>
        public string? ApplicationName { get; set; }

        /// <summary>
        /// Application Friendly Name
        /// </summary>
        public string? ApplicationFriendlyName { get; set; }

        /// <summary>
        /// Application Description
        /// </summary>
        public string? ApplicationDescription { get; set; }

        /// <summary>
        /// Rabbit Hosts
        /// </summary>
        public string? RabbitMQHosts { get; set; }

        /// <summary>
        /// Rabbit Port
        /// </summary>
        public short RabbitMQPort { get; set; }

        /// <summary>
        /// Rabbit VirtualHost
        /// </summary>
        public string? RabbitMQVirtualHost { get; set; }

        /// <summary>
        /// Provider Queue Name
        /// </summary>
        public string? RabbitMQRetrievalSystemDataIntakeProviderQueue { get; set; }

        /// <summary>
        /// Member Queue Name
        /// </summary>
        public string? RabbitMQRetrievalSystemDataIntakeMemberQueue { get; set; }

        /// <summary>
        /// Request queue Name
        /// </summary>
        public string? RabbitMQRetrievalSystemDataIntakeRequestQueue { get; set; }

        /// <summary>
        /// Rabbit UserName
        /// </summary>
        public string? RabbitMQUserName { get; set; }

        /// <summary>
        /// Rabbit Password
        /// </summary>
        public string? RabbitMQEncryptedPassword { get; set; }

        /// <summary>
        /// Rabbit NetworkRecoveryInterval
        /// </summary>
        public short RabbitMQNetworkRecoveryInterval { get; set; }

        /// <summary>
        /// Rabbit ConnectionAttemptCount
        /// </summary>
        public short RabbitMQConnectionAttemptCount { get; set; }

        /// <summary>
        /// Rabbit WaitBetweenConnectionRetries
        /// </summary>
        public short RabbitMQWaitBetweenConnectionRetries { get; set; }

        /// <summary>
        /// Rabbit DelayedExchange
        /// </summary>
        public string? RetrievalSystemDataIntakeRabbitMQDelayedExchange { get; set; }

        /// <summary>
        /// Rabbit TTLExpiration
        /// </summary>
        public int RetrievalSystemDataIntakeRabbitMQTTLExpiration { get; set; }

        /// <summary>
        /// Rabbit ChannelRetryCount
        /// </summary>
        public short RetrievalSystemDataIntakeRabbitMQChannelRetryCount { get; set; }

        /// <summary>
        /// Rabbit WaitBetweenChannelRetries
        /// </summary>
        public short RetrievalSystemDataIntakeRabbitMQWaitBetweenChannelRetries { get; set; }

        /// <summary>
        /// Rabbit ReprocessingMaxRetries
        /// </summary>
        public short RetrievalSystemDataIntakeRabbitMQReprocessingMaxRetries { get; set; }


        /// <summary>
        /// Rabbit DeadLetterRetryCount
        /// </summary>
        public short RetrievalSystemDataIntakeRabbitMQDeadLetterRetryCount { get; set; }

        /// <summary>
        /// Rabbit XDelayInMilliSeconds
        /// </summary>
        public short RetrievalSystemDataIntakeRabbitMQXDelayInMilliSeconds { get; set; }


    }
}
