namespace api_dapper_api.Models.Responses;

public class LoginResponse
{
  public string AccessToken {get;set;} = null!;
  public int ExpiresIn {get;set;}
}