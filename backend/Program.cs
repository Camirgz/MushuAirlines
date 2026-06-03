using backend.Interfaces;
using backend.Repositories;
using backend.Services;
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

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
// DEPENDENCY INJECTION

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

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Purchase repositories
builder.Services.AddScoped<backend.Interfaces.IPassengerRepository,  backend.Repositories.PassengerRepository>();
builder.Services.AddScoped<backend.Interfaces.IItineraryRepository,  backend.Repositories.ItineraryRepository>();
builder.Services.AddScoped<backend.Interfaces.IPurchaseRepository,   backend.Repositories.PurchaseRepository>();


builder.Services.AddScoped<IAirportRepository, AirportRepository>();
builder.Services.AddScoped<IAirportService, AirportService>();

builder.Services.AddScoped<IFlightRepository, RouteCreationRepository>();
builder.Services.AddScoped<FlightAggregatorService>();

// Purchase services
builder.Services.AddSingleton<backend.Interfaces.ICodeGenerator,     backend.Services.CodeGenerator>();
builder.Services.AddSingleton<backend.Interfaces.IPricingCalculator, backend.Services.PurchasePricingCalculator>();
builder.Services.AddScoped<backend.Interfaces.IRouteCreationService, backend.Services.RouteCreationService>();
builder.Services.AddScoped<backend.Interfaces.IPurchaseService,      backend.Services.PurchaseService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors(MyAllowSpecificOrigins);

app.MapControllers();

app.Run();
