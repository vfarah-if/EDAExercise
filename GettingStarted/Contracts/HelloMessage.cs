using System;

namespace Contracts
{
    public record HelloMessage
    {
        public Guid CommandId { get; init; }
        public string Name { get; init; }

        // public DateTime Created { get; init; } = DateTime.Now;
        public DateTime Created { get; init; }
    }
}
