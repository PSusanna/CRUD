using MassTransit;

namespace CRUDShared.Contracts
{
    [EntityName("birthday-contract")]
    public class BirthdayContract : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; init; }
        public string Name { get; init; }
        public string Email { get; init; }
    }
}
