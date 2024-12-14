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
var appName = "Rest API's RESTful From 0 to Azure with ASP.NET Core 5 and Docker";
var appVersion = "v1";

builder.Services.AddRouting(options => options.LowercaseUrls = true); // For what url's like swagger url is always in lower case

// CORS -> It's allow that aplication from anothers domain can access this app.
builder.Services.AddCors(options =>
options.AddDefaultPolicy(builder => builder
.AllowAnyOrigin()
.AllowAnyMethod()
.AllowAnyHeader())
);

builder.Services.AddControllers(); // Add aplication controllers in this app

// Add context MySQLContext
var connection = builder.Configuration.GetConnectionString("MySQLConnectionString");
builder.Services.AddDbContext<MySQLContext>(
    options => options.UseMySql(connection, new MySqlServerVersion(new Version(8, 40))));

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
filterOptions.ContentResponseEnricherList.Add(new BookEnricher());

builder.Services.AddSingleton(filterOptions);

// Versioning Api()
builder.Services.AddApiVersioning();

// Using swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc(appVersion, new OpenApiInfo
    {
        Title = appName,
        Version = appVersion,
        Description = $"REST API RESTful developed in course '{appName}'",
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
// app.UseRouting(); // For what it is used ?

app.MapControllers(); // Responsable to execute the controllers
app.MapControllerRoute("DefaultApi", "{controller=values}/v{version=apiVersion}/{id?}"); // Responsable to execute HATEOS

// Enable CORS
app.UseCors();

// Executing swagger
app.UseSwagger(); // Responsable to Generate Json with the swagger documentation
app.UseSwaggerUI(c =>
{
    // Add access Url and page name to see the json genareted.
    /* "/swagger/v1/swagger.json" -> This link is going to show up on the page top 
    when the the application is running. */
    c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{appName} - {appVersion}");
}); // UseSwaggerUI is responsable to generate a HTML page

// Configuring how will be my swagger page.
var option = new RewriteOptions();
option.AddRedirect("^$", "swagger"); // Redirection rule to swagger page
app.UseRewriter(option);

// Executing application
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