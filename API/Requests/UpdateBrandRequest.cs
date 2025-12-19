using System.ComponentModel.DataAnnotations;

namespace api_dapper_api.Requests
{
  public class UpdateBrandRequest
  {
    [Required]
    public Guid BrandId { get; set; }
    [Required]
    public string Name { get; set; } = null!;
    [Required]
    public byte[]? VersionFila {get;set;}
  }
}