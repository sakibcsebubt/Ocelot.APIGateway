// Program.cs (ApiGateway)
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
// SwaggerForOcelot namespaces:
//using MMLib.SwaggerForOcelot.DependencyInjection;
//using MMLib.SwaggerForOcelot.Middlewares;

var builder = WebApplication.CreateBuilder(args);
// Add JWT Bearer Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Your Identity Provider URL
        options.Authority = "http://localhost:5002"; // Your Partner Service URL or Authorization Server URL
        options.Audience = "your-api-audience"; // Your Gateway API audience
        options.RequireHttpsMetadata = false; // Set to true in production
        options.SaveToken = true;
    });



// Ocelot + SwaggerForOcelot read the same config file
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Add Ocelot and SwaggerForOcelot
builder.Services.AddOcelot(builder.Configuration);
builder.Services.AddSwaggerForOcelot(builder.Configuration, o =>
{
    // Optional: show docs for Ocelot "Aggregates" if you use them
    o.GenerateDocsForAggregates = true;
    // Optional: cache transformed downstream docs
    // o.DownstreamDocsCacheExpire = TimeSpan.FromMinutes(5);
});

var app = builder.Build();

// A tiny home page
app.MapGet("/", () => "Ocelot API Gateway running");

// <-- SwaggerForOcelot must be added BEFORE UseOcelot (Ocelot is terminal)
app.UseSwaggerForOcelotUI(opt =>
{
    // default generator endpoint used by the UI
    opt.PathToSwaggerGenerator = "/swagger/docs";
}, ui =>
{
    ui.DocumentTitle = "Gateway Swagger";
});

// Ocelot pipeline (keep at the end)
await app.UseOcelot();
await app.RunAsync();
