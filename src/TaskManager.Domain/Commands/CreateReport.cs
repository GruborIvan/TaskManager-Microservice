using System;
using System.Collections.Generic;

namespace TaskManager.Domain.Commands
{
    public class CreateReport : ICommand
    {
        public CreateReport(
            Guid correlationId,
            IEnumerable<string> dboEntities,
            DateTime? fromDatetime,
            DateTime? toDatetime,
            Guid initiatedBy
            )
        {
            CommandId = Guid.NewGuid();
            CorrelationId = correlationId;
            DboEntities = dboEntities;
            FromDatetime = fromDatetime;
            ToDatetime = toDatetime;
            InitiatedBy = initiatedBy;
        }

        public Guid CommandId { get; }
        public Guid CorrelationId { get; }
        public IEnumerable<string> DboEntities { get; }
        public DateTime? FromDatetime { get; }
        public DateTime? ToDatetime { get; }
        public Guid InitiatedBy { get; }
    }
}
