using Microsoft.EntityFrameworkCore;
using RestWithASPNETUdemy.Model.Context;
using RestWithASPNETUdemy.Business;
using RestWithASPNETUdemy.Business.Implementation;
using RestWithASPNETUdemy.Repository;
using Serilog;
using MySqlConnector;
using EvolveDb;
using RestWithASPNETUdemy.Repository.Generic;
using Microsoft.Net.Http.Headers;
using RestWithASPNETUdemy.Hypermedia.Filters;
using RestWithASPNETUdemy.Hypermedia.Enricher;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Rewrite;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();

// Add context MySQLContext
var connection = builder.Configuration["MySQLConnection:MySQLConnectionString"];
builder.Services.AddDbContext<MySQLContext>(
    options => options.UseMySql(connection, new MySqlServerVersion(new Version(8, 0))));

// Configuration to use migrations
if (builder.Environment.IsDevelopment())
{
    MigrateDatabase(connection);
}

// [Content negociation] -> Add config to use others mediatype as XML for example.
builder.Services.AddMvc(options =>
{
    /* RespectBrowserAcceptHeader -> It is used to the application 
    accept the propety that come in the request header. */
    options.RespectBrowserAcceptHeader = true;

    /* The lines below add the mediaTypes accepted by the application. */
    options.FormatterMappings.SetMediaTypeMappingForFormat("xml", MediaTypeHeaderValue.Parse("application/xml"));
    options.FormatterMappings.SetMediaTypeMappingForFormat("json", MediaTypeHeaderValue.Parse("application/json"));
})
.AddXmlSerializerFormatters();

// Configuration for use HATEOS
var filterOptions = new HyperMediaFilterOptions();
filterOptions.ContentResponseEnricherList.Add(new PersonEnricher());

builder.Services.AddSingleton(filterOptions);

// Versioning Api()
builder.Services.AddApiVersioning();

// Using swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Rest API's From 0 to Azure with ASP.NET Core 5 and Docker",
        Version = "v1",
        Description = "API RESTful developed in course 'Rest API's From 0 to Azure with ASP.NET Core 5 and Docker'",
        Contact = new OpenApiContact
        {
            Name = "Ewerton Silva",
            Url = new Uri("https://github.com/ewertonss1995")
        }
    });
});

// Dependency injection
builder.Services.AddScoped<IPersonBusiness, PersonBusinessImplementation>();
builder.Services.AddScoped<IBookBusiness, BookBusinessImplementation>();
// Injection generic interface repositoy
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

// ...
//builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers(); // Responsable to execute the controllers
app.MapControllerRoute("DefaultApi", "{controller=values}/v{version=apiVersion}/{id?}"); // Responsable to execute HATEOS
app.UseSwagger(); // Responsable to generate Json with the documentation
app.UseSwaggerUI(c =>
{
    // Add access Url and page name.
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Rest API's From 0 to Azure with ASP.NET Core 5 and Docker - v1");
}); // UseSwaggerUI is responsable to generate a HTML page

// Configuring how will be my swagger page.
var option = new RewriteOptions();
option.AddRedirect("^$", "swagger"); // Redirection rule to swagger page
app.UseRewriter(option);

app.Run();

// Using migration
void MigrateDatabase(string? connection)
{
    try
    {
        using var evolveConnection = new MySqlConnection(connection);
        var evolve = new Evolve(evolveConnection, Log.Information)
        {
            Locations = new List<string> { "db/migrations", "db/dataset" },
            IsEraseDisabled = true
        };

        evolve.Migrate();
    }
    catch (Exception ex)
    {
        Log.Error("Database migration failed", ex);
        throw;
    }
}