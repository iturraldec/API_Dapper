using System.ComponentModel.DataAnnotations;

namespace api_dapper_api.Requests
{
  public class LoginRequest
  {
    [Required]
    public string Username { get; set; } = null!;
    [Required]
    public string Password { get; set; } = null!;
  }
}