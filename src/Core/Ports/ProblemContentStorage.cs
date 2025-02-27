using Core.Domain;

namespace Core.Ports;

public interface ProblemContentStorage
{
    public Task Store(StoredFile content, Problem problem, CancellationToken cancellationToken);
}
