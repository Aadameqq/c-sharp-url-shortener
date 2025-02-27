using Api.Controllers.Dtos;
using Core.Ports;
using Core.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProblemsController(CreateProblemUseCase createProblemUseCase) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CreateProblemResponse>> Create(
        IFormFile file,
        [FromForm] CreateProblemBody body,
        CancellationToken cancellationToken
    )
    {
        var result = await createProblemUseCase.Execute(
            body.Title,
            new StoredFileImpl(file),
            cancellationToken
        );

        return new CreateProblemResponse(
            result.Value,
            $"http://localhost:5007/static/problems/{result.Value}.pdf"
        );
    }

    public class StoredFileImpl(IFormFile formFile) : StoredFile
    {
        public async Task CopyToStream(Stream stream)
        {
            await formFile.CopyToAsync(stream);
        }

        public string GetFileName()
        {
            return formFile.FileName;
        }

        public string GetContentType()
        {
            return formFile.ContentType;
        }

        public long GetLength()
        {
            return formFile.Length;
        }
    }
}
