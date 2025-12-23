using api_dapper_api.Models;
using api_dapper_api.Models.Responses;
using api_dapper_api.Requests;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace api_dapper_api;

[Route("api/[controller]")]
[ApiController]
public class BrandController : ControllerBase
{
  private readonly string _connectionString = @"Server=localhost,1433;
                                                Database=Northwind;
                                                User ID=sa;
                                                Password=J1z01234_;
                                                Encrypt=False;";
  private readonly SqlConnection _connection;
  
  //
  public BrandController()
  {
    _connection = new SqlConnection(_connectionString);
  }

  // listado de marcas
  [HttpGet]
  public async Task<IActionResult> Index()
  {
    // mapeando a Brand, tambien se puede hacer sin mapeo de datos
    var brands = await _connection.QueryAsync<Brand>("SELECT * FROM Brand WHERE IsDeleted = 0 ORDER BY Name;");    
    var result = new MessageResponse
                    {
                      Result = true,
                      Message = "Listado de Marcas",
                      Data = brands.ToList()
                    };

    return Ok(result);
  }

  // listado de marcas con cervezas
  [HttpGet("GetBrandsWithBeers")]
  public async Task<IActionResult> GetBrandsWithBeers()
  {
    // 1. Diccionario para mantener rastro de las masrcas ya creadas
    var brandDictionary = new Dictionary<Guid, Brand>();

    var sql = @"
        SELECT brand.BrandId, brand.Name, brand.UpdatedAt, brand.VersionFila, beer.BeerID, beer.Name, beer.Style, beer.BrandID
        FROM Brand brand
        LEFT JOIN Beer beer ON brand.BrandId = beer.BrandID";

    var brands = await _connection.QueryAsync<Brand, Beer, Brand>(
        sql, 
        (brand, beer) => 
        {
            // 2. Intentamos obtener la marca del diccionario
            if (!brandDictionary.TryGetValue(brand.BrandId, out var currentBrand))
            {
                currentBrand = brand;
                currentBrand.Beers = new List<Beer>();
                brandDictionary.Add(currentBrand.BrandId, currentBrand);
            }

            // 3. Si hay un producto en esta fila, lo añadimos a la lista de la categoría
            if (beer != null)
            {
                currentBrand.Beers.Add(beer);
            }

            return currentBrand;
        }, 
        splitOn: "BeerId" // Divide las columnas donde empieza la tabla Beer
    );

    // 4. Obtenemos los valores únicos del diccionario
    var result = brandDictionary.Values.ToList();

    return Ok(new MessageResponse
    {
        Result = true,
        Message = "Listado de Marcas con Cervezas",
        Data = result
    });
  }

  // retornar una marca
  [HttpGet("GetById/{Id}")]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<IActionResult> GetById([FromRoute] Guid Id)
  {
    var sql = @"SELECT * FROM Brand WHERE BrandId = @Id;";

    var brand = await _connection.QuerySingleOrDefaultAsync<Brand>(sql, new {Id});

    var result = new MessageResponse();

    if(brand == null)
    {
      result.Result = false;
      result.Message = "Marca no encontrada!";

      return NotFound(result);
    }
    
    result.Result = true;
    result.Data = brand;

    return Ok(result);
  }

  // insertar una marca
  [HttpPost]
  [ProducesResponseType(StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> Create([FromBody] Brand brand)
  {
    var sql = "INSERT INTO Brand (BrandId, Name, IsDeleted) VALUES (@BrandId, @Name, 0);";
    
    brand.BrandId = Guid.NewGuid();

    await _connection.ExecuteAsync(sql, new { brand.BrandId, brand.Name });

    var result = new MessageResponse
                    {
                      Result = true,
                      Message = "Marca creada",
                      Data = brand
                    };
    
    return CreatedAtAction(nameof(GetById), new { Id = brand.BrandId }, result);
  }

  // actualizar una marca
  [HttpPut]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status409Conflict)]
  public async Task<IActionResult> Update([FromBody] UpdateBrandRequest request)
  {
    //Thread.Sleep(10000);
    // 1. Validación básica
    if (request.VersionFila == null)
    {
        return BadRequest("Se requiere VersionFila para actualizar.");
    }

    // 2. SQL de Actualización con chequeo de concurrencia
    // Nota: NO actualizamos VersionFila manualmente, SQL Server lo hace solo.
    // Solo lo usamos en el WHERE para comparar.
    var sql = @"
        UPDATE Brand 
        SET 
            Name = @Name, 
            UpdatedAt = GETUTCDATE()  -- Seteamos la fecha de actualización aquí
        WHERE 
            BrandId = @BrandId 
            AND VersionFila = @VersionFila;"; // <--- EL SECRETO ESTÁ AQUÍ
    
    //  3. Ejecutar la actualización
    int rowsAffected = await _connection.ExecuteAsync(sql, request);
    
    // 4. Verificamos si se actualizó algo
    if (rowsAffected == 0)
    {
        // Si entra aquí, pueden pasar dos cosas:
        // A) El ID no existe.
        // B) El ID existe, pero el VersionFila ya cambió (Concurrencia).
        
        // Opcional: Verificar si el registro existe para dar un error más preciso
        var exists = await _connection.ExecuteScalarAsync<bool>(
            "SELECT COUNT(1) FROM Brand WHERE BrandId = @BrandId", new { request.BrandId });
            
        if (!exists)
            return NotFound("La marca no existe.");
            
        // Si existe pero no actualizó, es conflicto de concurrencia
        return Conflict("El registro fue modificado por otro usuario. Por favor, recargue los datos.");
    }

    // 5. Retornamos éxito (204 No Content es estándar para updates)
    return NoContent();
  }

  // eliminar una marca "suavemente"
  [HttpGet("Delete/{Id}")]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  public async Task<IActionResult> Delete([FromRoute] Guid Id)
  {
    var sql = "UPDATE Brand SET IsDeleted = 1, DeletedTimeUtc = GETDATE() WHERE BrandId = @Id;";
    
    int rows = await _connection.ExecuteAsync(sql, new { Id });
    
    return (rows == 0 ? NotFound() : NoContent());
  }

  // eliminar una marca
  [HttpDelete("{Id}")]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  public async Task<IActionResult> HardDelete([FromRoute] Guid Id)
  {
    var sql = "DELETE FROM Brand WHERE BrandId = @Id;";
    
    int rows = await _connection.ExecuteAsync(sql, new { Id });
    
    return (rows == 0 ? NotFound() : NoContent());
  }
}
