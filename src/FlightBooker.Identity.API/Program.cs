using FlightBooker.Identity.API.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddAuthorization();

// ---- YEN� EKLENEN CORS SERV�S� ----
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.AllowAnyOrigin() // Geli�tirme i�in t�m kaynaklara izin ver
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

// ASP.NET Core Identity servislerini ekle ve JWT kullanaca��n� belirt
// AddIdentity yerine AddIdentityCore kullanabiliriz, ��nk� UI istemiyoruz.
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

//// Sihirli sat�r: /register, /login gibi endpoint'leri olu�turur
//app.MapIdentityApi<IdentityUser>();

app.MapControllers();
app.Run();