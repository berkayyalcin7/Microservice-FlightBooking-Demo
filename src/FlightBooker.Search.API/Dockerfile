# Konum: src/FlightBooker.Search.API/Dockerfile

# --- Aþama 1: Build Ortamý ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Proje dosyalarýný ve solution dosyasýný kopyala
COPY ["GEMINI-2.sln", "."]
COPY ["src/FlightBooker.ApiGateway/FlightBooker.ApiGateway.csproj", "src/FlightBooker.ApiGateway/"]
COPY ["src/FlightBooker.Search.API/FlightBooker.Search.API.csproj", "src/FlightBooker.Search.API/"]
COPY ["src/FlightBooker.Booking.API/FlightBooker.Booking.API.csproj", "src/FlightBooker.Booking.API/"]
COPY ["src/FlightBooker.Identity.API/FlightBooker.Identity.API.csproj", "src/FlightBooker.Identity.API/"]
COPY ["src/FlightBooker.Notification.Service/FlightBooker.Notification.Service.csproj", "src/FlightBooker.Notification.Service/"]
COPY ["src/FlightBooker.Messages/FlightBooker.Messages.csproj", "src/FlightBooker.Messages/"]

# NuGet paketlerini yükle
RUN dotnet restore "GEMINI-2.sln"

# Projenin geri kalan tüm dosyalarýný kopyala
COPY . .

# Projeyi yayýnla
WORKDIR "/source/src/FlightBooker.Search.API"
RUN dotnet publish "FlightBooker.Search.API.csproj" -c Release -o /app/publish

# --- Aþama 2: Final Ortamý ---
# Bu bir API projesi olduðu için, ASP.NET Core runtime'ýný içeren imajý kullanýyoruz.
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FlightBooker.Search.API.dll"]
