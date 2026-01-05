namespace api_dapper_api.Models;

public class User
{
  public int Id {get;set;}
  public string Email {get;set;} = null!;
  public string Password {get;set;} = null!;
}