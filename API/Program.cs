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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRouting(routing => routing.LowercaseUrls = true);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(MiPoliticaCORS);

app.MapControllers();

app.Run();
