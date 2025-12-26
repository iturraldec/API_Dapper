using api_dapper_api.Models;
using api_dapper_api.Models.Responses;
using api_dapper_api.Requests;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace api_dapper_api;

[Route("api/[controller]")]
[ApiController]
public class BeerController : ControllerBase
{
  private readonly string _connectionString = @"Server=localhost,1433;
                                                Database=Northwind;
                                                User ID=sa;
                                                Password=J1z01234_;
                                                Encrypt=False;";
  private readonly SqlConnection _connection;
  
  //
  public BeerController()
  {
    _connection = new SqlConnection(_connectionString);
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
