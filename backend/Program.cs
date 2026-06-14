using backend.Interfaces;
using backend.Repositories;
using backend.Services;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

var jwtKey = "MushuClaveLeo.ari,Cami,alex;dani";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:8080")
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            ),

            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();


builder.Services.AddScoped<
    IPurchaseConfirmationRepository,
    PurchaseConfirmationRepository>();

builder.Services.AddScoped<
    IQrService, 
    QrService>();

builder.Services.AddScoped<
    IPaymentRepository,
    PaymentRepository>();
    
builder.Services.AddScoped<
    IEmailPurchaseService,
    EmailPurchaseService>();
    
builder.Services.AddScoped<
    IPaymentService,
    PaymentService>();

builder.Services.AddScoped<
    PurchaseConfirmationService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddScoped<IExternalApiRepository, ExternalApiRepository>();
builder.Services.AddScoped<ExternalApiService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddScoped<backend.Interfaces.IPassengerRepository,  backend.Repositories.PassengerRepository>();
builder.Services.AddScoped<backend.Interfaces.IPurchaseRepository,   backend.Repositories.PurchaseRepository>();


builder.Services.AddScoped<IAirportRepository, AirportRepository>();
builder.Services.AddScoped<IAirportService, AirportService>();

builder.Services.AddScoped<IAircraftRepository, AircraftRepository>();
builder.Services.AddScoped<IAircraftService, AircraftService>();

builder.Services.AddScoped<IAircraftTypeRepository, AircraftTypeRepository>();
builder.Services.AddScoped<IAircraftTypeService, AircraftTypeService>();

builder.Services.AddScoped<IUserListRepository, UserListRepository>();
builder.Services.AddScoped<IUserListService, UserListService>();

builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
builder.Services.AddScoped<IProfileService, ProfileService>();

builder.Services.AddScoped<IFlightRepository, RouteCreationRepository>();
builder.Services.AddScoped<FlightAggregatorService>();

builder.Services.AddSingleton<backend.Interfaces.ICodeGenerator,     backend.Services.CodeGenerator>();
builder.Services.AddSingleton<backend.Interfaces.IPricingCalculator, backend.Services.PurchasePricingCalculator>();
builder.Services.AddScoped<backend.Interfaces.IRouteCreationService, backend.Services.RouteCreationService>();
builder.Services.AddScoped<backend.Interfaces.IPurchaseService,      backend.Services.PurchaseService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
