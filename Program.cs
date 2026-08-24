using Microsoft.EntityFrameworkCore;
using RequestLifeCycle.data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(Option =>
Option.UseMySQL(
        builder.Configuration.GetConnectionString(
            "DefaultConnection")!
    ));
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
