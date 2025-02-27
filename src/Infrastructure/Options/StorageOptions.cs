using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Options;

public class StorageOptions
{
    [Required]
    public required string Location { get; set; }
}
