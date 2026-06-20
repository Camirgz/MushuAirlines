using backend.Interfaces;
using backend.Repositories;
using backend.Services;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins, policy =>
    {
        policy.WithOrigins(
            "http://localhost:8080",
            "https://mushu-airlines.vercel.app",
            "https://mushu-airlines-git-dev-camirgzs-projects.vercel.app"
        )
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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            ),

            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<IPurchaseConfirmationRepository, PurchaseConfirmationRepository>();
builder.Services.AddScoped<IQrService, QrService>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IEmailPurchaseService, EmailPurchaseService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<PurchaseConfirmationService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase);


builder.Services.AddScoped<IReservationReportService,ReservationReportService>();

builder.Services.AddScoped<IPurchaseConfirmationRepository, PurchaseConfirmationRepository>();
builder.Services.AddScoped<IQrService, QrService>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IEmailPurchaseService, EmailPurchaseService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<PurchaseConfirmationService>();
builder.Services.AddScoped<ILoginRepository, LoginRepository>();
builder.Services.AddScoped<ILoginService, LoginService>();

builder.Services.AddScoped<IReservationLoginRepository, ReservationLoginRepository>();
builder.Services.AddScoped<IReservationLoginService, ReservationLoginService>();

builder.Services.AddScoped<IExternalApiRepository, ExternalApiRepository>();
builder.Services.AddScoped<ExternalApiService>();

builder.Services.AddScoped<IPassengerRepository, PassengerRepository>();
builder.Services.AddScoped<IPurchaseRepository, PurchaseRepository>();

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

builder.Services.AddSingleton<ICodeGenerator, CodeGenerator>();
builder.Services.AddSingleton<IPricingCalculator, PurchasePricingCalculator>();

builder.Services.AddScoped<IRouteCreationService, RouteCreationService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;

        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();