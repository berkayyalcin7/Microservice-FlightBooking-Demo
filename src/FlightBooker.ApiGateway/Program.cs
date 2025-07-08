var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


// Gelen istekleri YARP'�n y�nlendirme haritas�na g�nder
app.MapReverseProxy();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();


app.Run();
