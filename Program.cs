using Extensions;
using THESSA.Contract;
using THESSA.Repository;
using THESSA.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var services = builder.Services;

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.AddApiVersioning();
services.AddHttpClient();

// Register Semantic Kernel
builder.Services.AddKernelServices();

// Register Repositories
builder.Services.AddScoped<IThessaBotRepository, ThessaRepository>();
builder.Services.AddScoped<IGitHub, GitHubRepository>();

// Register Services
builder.Services.AddScoped<IGithubService, GitHubService>();
builder.Services.AddScoped<IThessaService, ThessaService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
