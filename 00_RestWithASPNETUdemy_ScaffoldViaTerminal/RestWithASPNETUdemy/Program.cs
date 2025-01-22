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
using RestWithASPNETUdemy.Services;
using RestWithASPNETUdemy.Services.Implementations;
using RestWithASPNETUdemy.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);
var appName = "Rest API's RESTful From 0 to Azure with ASP.NET Core 5 and Docker";
var appVersion = "v1";

builder.Services.AddRouting(options => options.LowercaseUrls = true); // For what url's like swagger url is always in lower case

// Token Configuration
var tokenConfigurations = new TokenConfiguration();

// Load configuration from appsettings.json
new ConfigureFromConfigurationOptions<TokenConfiguration>(
    builder.Configuration.GetSection("TokenConfigurations")
)
.Configure(tokenConfigurations);

// Adicionar o tokenConfigurations como um serviço singleton
builder.Services.AddSingleton(tokenConfigurations);

// Configuração da autenticação
builder.Services.AddAuthentication(options =>
{
    /* A autenticação padrão usada será baseada em JWT Bearer. Isso indica que a aplicação espera 
    receber tokens JWT nas requisições HTTP.*/
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
// AddJwtBearer(options => { ... }) permite que você configure as opções específicas para autenticação JWT.
.AddJwtBearer(options =>
{
    // TokenValidationParameters é um objeto que define como o token JWT deve ser validado. As propriedades incluem:
    options.TokenValidationParameters = new TokenValidationParameters{
        ValidateIssuer = true, // Se deve validar o emissor do token
        ValidateAudience = true, // Se deve validar a audiência do token.
        ValidateLifetime = true, // Se deve validar a expiração do token.
        ValidateIssuerSigningKey = true, // Se deve validar a chave que foi usada para assinar o token.
        ValidIssuer = tokenConfigurations.Issuer, // O emissor esperado, retirado da configuração que foi carregada anteriormente.
        ValidAudience = tokenConfigurations.Audience, // A audiência esperada, também retirada da configuração.
        /* IssuerSigningKey é a chave que será usada para validar a assinatura do token, que é criada a 
        partir da chave secreta configurada (convertida em bytes). */
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenConfigurations.Secret)),
    };
});

// AddAuthorization(auth => { ... }) permite que você configure as políticas de autorização.
builder.Services.AddAuthorization(auth => {
    /*Aqui, uma nova política chamada "Bearer" é adicionada. Essa política requer que o usuário 
    esteja autenticado e especifica que o esquema de autenticação a ser usado é o JWT Bearer*/
    auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme) 
    .RequireAuthenticatedUser().Build()); // indica que apenas usuários autenticados podem acessar recursos protegidos por esta política.
});

// CORS -> It's allow that aplication from anothers domain can access this app.
builder.Services.AddCors(options =>
options.AddDefaultPolicy(builder => builder
.AllowAnyOrigin()
.AllowAnyMethod()
.AllowAnyHeader())
);

builder.Services.AddControllers(); // Add aplication controllers in this app

// Add context MySQLContext
var connection = builder.Configuration["MySQLConnection:MySQLConnectionString"];
builder.Services.AddDbContext<MySQLContext>(
    options => options.UseMySql(connection, new MySqlServerVersion(new Version(9, 1))));

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
builder.Services.AddScoped<ILoginBusiness, LoginBusinessImplementation>();

builder.Services.AddTransient<ITokenService, TokenService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>)); // Injection generic interface repositoy

// ...
//builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
/*if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}*/

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