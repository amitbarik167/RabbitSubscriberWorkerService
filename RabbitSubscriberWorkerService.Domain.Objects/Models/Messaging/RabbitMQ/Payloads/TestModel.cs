using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitSubscriberWorkerService.Domain.Objects.Models.Messaging.RabbitMQ.Payloads
{
    public class TestModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public short? RetryCount { get; set; }


    }
}
