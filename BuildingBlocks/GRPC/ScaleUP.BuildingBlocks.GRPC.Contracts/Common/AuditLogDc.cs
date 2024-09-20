using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Common
{
    public class AuditLogDc
    {
        public long Id { get; set; }
        public string UserId { get; set; }
        public long EntityId { get; set; }
        public string DatabaseName { get; set; }
        public string EntityName { get; set; }
        public string Action { get; set; }
        public DateTime Timestamp { get; set; }
        public string Changes { get; set; }
        public List<Field> Fields { get; set; }
        public int TotalRecords { get; set; }

    }
    public class Field
    {
        public string FieldName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
}
