using Core.Domain;
using Core.Ports;

namespace Infrastructure.Persistence.EF;

public class EfProblemsRepository(DatabaseContext ctx) : ProblemsRepository
{
    public async Task CreateAndFlush(Problem problem)
    {
        await ctx.AddAsync(problem);
        await ctx.SaveChangesAsync();
    }
}
