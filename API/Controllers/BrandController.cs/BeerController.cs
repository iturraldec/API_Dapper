using api_dapper_api.Models;
using api_dapper_api.Models.Responses;
using api_dapper_api.Requests;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;

namespace api_dapper_api.Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class BeerController : ControllerBase
{
  private readonly SqlConnection _connection;
  
  //
  public BeerController(SqlConnection sqlConnection)
  {
    _connection = sqlConnection;
  }

  // listado de marcas
  [HttpGet]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MessageResponse))]
  public async Task<IActionResult> Index()
  {
    // de muchos a uno
    var sql = @"SELECT *
              FROM Beer
              INNER JOIN Brand ON Beer.BrandId = Brand.BrandId
              ORDER BY Beer.Name;";
      
    var beers = await _connection.QueryAsync<Beer, Brand, Beer>(sql, (beer, brand) => {
                          beer.Brand = brand;
                          
                          return beer;
                      }, 
                      splitOn: "BrandId" );

    var result = new MessageResponse
                    {
                      Result = true,
                      Message = "Listado de Cervezas",
                      Data = beers.ToList()
                    };

    return Ok(result);
  }
}
