using Core.Domain;
using Core.Ports;

namespace Core.UseCases;

public class CreateProblemUseCase(
    ProblemContentStorage contentStorage,
    DateTimeProvider dateTimeProvider,
    ProblemsRepository problemsRepository
)
{
    public async Task<Result<Guid>> Execute(
        string title,
        StoredFile content,
        CancellationToken cancellationToken
    )
    {
        var problem = new Problem(title, dateTimeProvider.Now());

        // await problemsRepository.CreateAndFlush(problem);

        await contentStorage.Store(content, problem, cancellationToken);

        return problem.Id;
    }
}
