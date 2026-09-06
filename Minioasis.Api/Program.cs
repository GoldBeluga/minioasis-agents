using Minioasis.Api.Extensions.Shared;
using Minioasis.Application.Extensions.Shared;
using Minioasis.Koha.Extensions.Shared;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddMinioasisApplication();
builder.Services.AddMinioasisKoha(builder.Configuration);
builder.Services.AddApiErrorHandling(builder.Environment.ContentRootPath);
builder.Services.AddEndpointModules();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.MapEndpointModules();
app.Run();

public partial class Program;
