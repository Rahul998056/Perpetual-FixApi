using Microsoft.OpenApi.Models;
using Perpetuals.Fix.Core.Configuration;
using Perpetuals.Fix.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();
builder.Services.AddSingleton<IFixServices>();
builder.Services.AddHttpClient<IUpstreamService>();


//appsettings.json 
builder.Services.Configure<UpstreamApiOptions>(builder.Configuration.GetSection("UpstreamApi"));
builder.Services.AddScoped<IUpstreamService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>

{

    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FIX Market Data API", Version = "v1" });



    // Add Authorization and Session headers

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme

    {

        In = ParameterLocation.Header,

        Description = "Enter your Bearer token here (e.g. Bearer abc123)",

        Name = "Authorization",

        Type = SecuritySchemeType.ApiKey

    });



    c.AddSecurityDefinition("Session", new OpenApiSecurityScheme

    {

        In = ParameterLocation.Header,

        Description = "Session cookie (e.g. .eJxyz...)",

        Name = "Session",

        Type = SecuritySchemeType.ApiKey

    });



    c.AddSecurityRequirement(new OpenApiSecurityRequirement

  {

    {

      new OpenApiSecurityScheme

      {

        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }

      },

      Array.Empty<string>()

    },

    {

      new OpenApiSecurityScheme

      {

        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Session" }

      },

      Array.Empty<string>()

    }

  });

});

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();