using FlightBooker.Booking.API.Services;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// 1. Controller'larýn çalýþmasý için gereken temel servisleri kaydeder.
builder.Services.AddControllers();

builder.Services.AddHttpClient("SearchService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:SearchAPI"]);
});

builder.Services.AddScoped<BookingService>();

// MassTransit yapýlandýrmasý.
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

// Yetkilendirme (ileride kullanacaðýz)
app.UseAuthorization();

// 3. EN ÖNEMLÝ SATIR: Gelen istekleri Controller'lara yönlendiren haritalamayý yapar.
app.MapControllers();



app.Run();
