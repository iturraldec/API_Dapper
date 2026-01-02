using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var MiPoliticaCORS = "_miPoliticaCORS";

builder.Services.AddScoped<SqlConnection>(_ => new SqlConnection(connectionString));
//builder.Services.AddSingleton<IConfiguration>(_ => builder.Configuration);
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

/////////// 3 SERVICIOS //////////////////
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(options => 
{
  options.RequireHttpsMetadata = false; // en produccion se recomienda true
  options.SaveToken = true;
  options.TokenValidationParameters = new TokenValidationParameters{
    ValidIssuer = builder.Configuration["Jwt:Issuer"],
    ValidAudience = builder.Configuration["Jwt:Audience"],
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    
  };
});
builder.Services.AddAuthorization();
/////////// 3 //////////////////

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(MiPoliticaCORS);

/////////// 4 //////////////////
app.UseAuthentication();
app.UseAuthorization();
/////////// 4 //////////////////

app.MapControllers();

app.Run();
