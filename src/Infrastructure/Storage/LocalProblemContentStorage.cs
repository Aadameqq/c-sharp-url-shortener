using Core.Domain;
using Core.Ports;
using Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Infrastructure.Storage;

public class LocalProblemContentStorage(IOptions<StorageOptions> storageOptions)
    : ProblemContentStorage
{
    public async Task Store(
        StoredFile content,
        Problem problem,
        CancellationToken cancellationToken
    )
    {
        var filePath = Path.Combine($"{storageOptions.Value.Location}/{problem.Id}.pdf");

        var stream = new FileStream(filePath, FileMode.Create);

        await content.CopyToStream(stream);

        stream.Close();
    }
}
