using Microsoft.Extensions.Configuration;
using Quest.Common.Messages;

namespace Quest.Lib.Processor
{
    public interface IProcessor
    {
        ProcessingUnitId Id { get; set; }

        ProcessorStatusCode Status { get; set; }

        void Prepare(ProcessingUnitId processingUnitId, IConfiguration config);

        void Start();
    }
}