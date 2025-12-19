var builder = WebApplication.CreateBuilder(args);

var MiPoliticaCORS = "_miPoliticaCORS";

builder.Services.AddControllers();

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
