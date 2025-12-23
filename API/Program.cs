var builder = WebApplication.CreateBuilder(args);

var MiPoliticaCORS = "_miPoliticaCORS";

// Agrega esto en tu Program.cs
builder.Services.AddControllers();
/* builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Esta línea es la clave: ignora los ciclos de referencia
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        
        // Opcional: para que el JSON se vea ordenado (con sangría)
        options.JsonSerializerOptions.WriteIndented = true; 
    });
 */
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MiPoliticaCORS,
                      policy =>
                      {
                          policy.WithOrigins("*") // Permite cualquier origen (para desarrollo)
                                .AllowAnyHeader() // Permite cualquier encabezado en la solicitud
                                .AllowAnyMethod(); // Permite cualquier método HTTP (GET, POST, PUT, etc.)
                      });
});

var app = builder.Build();

app.UseHttpsRedirection();

app.UseCors(MiPoliticaCORS);

app.MapControllers();

app.Run();
