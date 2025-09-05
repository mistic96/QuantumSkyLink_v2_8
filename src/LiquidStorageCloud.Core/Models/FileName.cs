using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidStorageCloud.Core.Models
{
    public class Message
    {
        public long Id { get; set; }
        public string EntityId { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string Namespace { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public string EntityData { get; set; } = string.Empty;
        public bool? SolidState { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string Status { get; set; } = "Received";
        public string? Error { get; set; }
    }
}
