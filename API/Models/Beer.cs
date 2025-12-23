namespace api_dapper_api.Models;

public class Beer
{
  public int BeerId {get;set;}
  public string Name {get;set;} = null!;
  public string Style {get;set;} = null!;
  public Guid BrandId {get;set;}
  public Brand Brand {get;set;} = null!;
}