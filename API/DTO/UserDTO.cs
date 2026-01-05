using System.ComponentModel.DataAnnotations;

namespace api_dapper_api.Requests
{
  public class UserDTO
  {
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
  }
}