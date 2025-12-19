var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseRouting();

app.MapStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
