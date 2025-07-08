var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


// Gelen istekleri YARP'ýn yönlendirme haritasýna gönder
app.MapReverseProxy();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();


app.Run();
