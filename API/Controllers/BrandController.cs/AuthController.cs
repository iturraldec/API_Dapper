using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using api_dapper_api.Models.Responses;
using api_dapper_api.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace api_dapper_api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
  private readonly IConfiguration _configuration;
  
  //
  public AuthController(IConfiguration configuration)
  {
    _configuration = configuration;
  }

  [HttpPost]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponse))]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public IActionResult Authenticate([FromBody] LoginRequest loginRequest)
  {
    // validar usuario  y clave
    if(true)
    {
      var issuer = _configuration["Jwt:Issuer"];
      var audience = _configuration["Jwt:Audience"];
      var key = _configuration["Jwt:Key"];
      var tokenValidityMins = _configuration.GetValue<int>("Jwt:TokenValidityMins");
      var tokenExpiryTimeStap = DateTime.UtcNow.AddMinutes(tokenValidityMins);
      var tokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity(new[]
        {
          new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
          new Claim(JwtRegisteredClaimNames.UniqueName, loginRequest.Username),
          new Claim(JwtRegisteredClaimNames.Name, "Carlos Iturralde"),
          new Claim(ClaimTypes.Role, "admin")
        }),
        Expires = tokenExpiryTimeStap,
        Issuer = issuer,
        Audience = audience,
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),SecurityAlgorithms.HmacSha256Signature)
      };

      var tokenHandler = new JwtSecurityTokenHandler();
      var securityToken = tokenHandler.CreateToken(tokenDescriptor);
      var accessToken = tokenHandler.WriteToken(securityToken);

      LoginResponse response = new LoginResponse
      {
        AccessToken = accessToken,
        ExpiresIn = (int)tokenExpiryTimeStap.Subtract(DateTime.UtcNow).TotalSeconds
      };

      return Ok(response);
    };

    return BadRequest();
  }
}
