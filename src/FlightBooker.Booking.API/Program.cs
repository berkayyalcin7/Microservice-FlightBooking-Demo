using FlightBooker.Booking.API.Services;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// 1. Controller'lar�n �al��mas� i�in gereken temel servisleri kaydeder.
builder.Services.AddControllers();

builder.Services.AddHttpClient("SearchService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:SearchAPI"]);
});

builder.Services.AddScoped<BookingService>();

// MassTransit yap�land�rmas�.
builder.Services.AddMassTransit(config => {
    config.UsingRabbitMq((ctx, cfg) => {
        cfg.Host("localhost", "/", h => {
            h.Username("guest");
            h.Password("guest");
        });
    });
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Yetkilendirme (ileride kullanaca��z)
app.UseAuthorization();

// 3. EN �NEML� SATIR: Gelen istekleri Controller'lara y�nlendiren haritalamay� yapar.
app.MapControllers();



app.Run();
