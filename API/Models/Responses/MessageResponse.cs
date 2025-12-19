namespace api_dapper_api.Models.Responses;

public class MessageResponse
{
  public bool Result {get;set;}
  public string? Message {get;set;}
  public object? Data {get;set;}
}