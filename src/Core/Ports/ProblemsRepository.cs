using Core.Domain;

namespace Core.Ports;

public interface ProblemsRepository
{
    public Task CreateAndFlush(Problem problem);
}
