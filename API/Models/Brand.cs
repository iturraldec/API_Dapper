using System.ComponentModel.DataAnnotations;

namespace api_dapper_api.Models;

public class Brand
{
  public Guid BrandId {get;set;}
  public string Name {get;set;} = null!;
  public DateTime? UpdatedAt {get;set;}
  public DateTime? DeletedTimeUtc {get;set;}
  public bool IsDeleted {get;set;}
  public byte[]? VersionFila {get;set;}
}
