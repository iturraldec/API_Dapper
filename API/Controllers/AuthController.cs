using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using api_dapper_api.Models;
using api_dapper_api.Models.Responses;
using api_dapper_api.Requests;
using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace api_dapper_api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
  private readonly IConfiguration _configuration;
  private readonly SqlConnection _connection;
  
  //
  public AuthController(IConfiguration configuration, SqlConnection sqlConnection)
  {
    _configuration = configuration;
    _connection = sqlConnection;
  }
  
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MessageResponse))]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [HttpPost("register")]
  public async Task<IActionResult> Register([FromBody] UserDTO request)
  {
    if(string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
    {
      return BadRequest("Debe ingresar el correo y la clave del usuario...");
    }

    var result = new MessageResponse();

    // busqueda
    var sql = @"SELECT * FROM Users WHERE Email = @Email;";

    var user = await _connection.QuerySingleOrDefaultAsync<User>(sql, new {request.Email});

    if(user != null)
    {
      result.Result = false;
      result.Message = "Usuario ya existente!";

      return BadRequest(result);
    }

    user =  new User();
    var hashedPassword = new PasswordHasher<User>().HashPassword(user, request.Password);
    sql = "INSERT INTO Users (Email, Password) VALUES (@Email, @Password);";

    user.Email = request.Email;
    user.Password = hashedPassword;
    await _connection.ExecuteAsync(sql, new {user.Email, user.Password});

    result = new MessageResponse
                    {
                      Result = true,
                      Message = "Usuario creado",
                      Data = user
                    };
    
    return Ok(result);
  }

  [HttpPost]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> Authenticate([FromBody] UserDTO request)
  {
  
    // validar usuario  y clave
    var response = new MessageResponse();

    // busqueda
    var sql = @"SELECT * FROM Users WHERE Email = @Email;";

    var user = await _connection.QuerySingleOrDefaultAsync<User>(sql, new {request.Email});

    if(user == null)
    {
      response.Result = false;
      response.Message = "Usuario no existente!";

      return BadRequest(response);
    }
    
    var verificationResult = new PasswordHasher<User>().VerifyHashedPassword(user, user.Password, request.Password);

    if(verificationResult == PasswordVerificationResult.Failed)
    {
      response.Result = false;
      response.Message = "Clave incorrecta!";

      return BadRequest(response);
    }
    
    // clave correcta
    // generar token
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
        new Claim(JwtRegisteredClaimNames.UniqueName, request.Email),
        new Claim(JwtRegisteredClaimNames.Name, "Carlos Iturralde"),
        new Claim(ClaimTypes.Role, "admin")
      }),
      Expires = tokenExpiryTimeStap,
      Issuer = issuer,
      Audience = audience,
      SigningCredentials = new SigningCredentials(
                              new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
                              SecurityAlgorithms.HmacSha256Signature)
    };

    var tokenHandler = new JwtSecurityTokenHandler();
    var securityToken = tokenHandler.CreateToken(tokenDescriptor);
    var accessToken = tokenHandler.WriteToken(securityToken);
    
    response.Result = true;
    response.Message = "Autenticación exitosa!";
    response.Data = new 
    {
      AccessToken = accessToken,
      ExpiresIn = (int)tokenExpiryTimeStap.Subtract(DateTime.UtcNow).TotalSeconds
    };

    return Ok(response);  
  }
}
