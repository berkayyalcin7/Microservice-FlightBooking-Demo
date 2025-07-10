using FlightBooker.Identity.API.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddAuthorization();

// ---- YENÝ EKLENEN CORS SERVÝSÝ ----
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.AllowAnyOrigin() // Geliþtirme için tüm kaynaklara izin ver
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

// ASP.NET Core Identity servislerini ekle ve JWT kullanacaðýný belirt
// AddIdentity yerine AddIdentityCore kullanabiliriz, çünkü UI istemiyoruz.
builder.Services.AddIdentityCore<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization();

//// Sihirli satýr: /register, /login gibi endpoint'leri oluþturur
//app.MapIdentityApi<IdentityUser>();

app.MapControllers();
app.Run();