# Konum: src/FlightBooker.Search.API/Dockerfile

# --- A�ama 1: Build Ortam� ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Proje dosyalar�n� ve solution dosyas�n� kopyala
COPY ["GEMINI-2.sln", "."]
COPY ["src/FlightBooker.ApiGateway/FlightBooker.ApiGateway.csproj", "src/FlightBooker.ApiGateway/"]
COPY ["src/FlightBooker.Search.API/FlightBooker.Search.API.csproj", "src/FlightBooker.Search.API/"]
COPY ["src/FlightBooker.Booking.API/FlightBooker.Booking.API.csproj", "src/FlightBooker.Booking.API/"]
COPY ["src/FlightBooker.Identity.API/FlightBooker.Identity.API.csproj", "src/FlightBooker.Identity.API/"]
COPY ["src/FlightBooker.Notification.Service/FlightBooker.Notification.Service.csproj", "src/FlightBooker.Notification.Service/"]
COPY ["src/FlightBooker.Messages/FlightBooker.Messages.csproj", "src/FlightBooker.Messages/"]

# NuGet paketlerini y�kle
RUN dotnet restore "GEMINI-2.sln"

# Projenin geri kalan t�m dosyalar�n� kopyala
COPY . .

# Projeyi yay�nla
WORKDIR "/source/src/FlightBooker.Search.API"
RUN dotnet publish "FlightBooker.Search.API.csproj" -c Release -o /app/publish

# --- A�ama 2: Final Ortam� ---
# Bu bir API projesi oldu�u i�in, ASP.NET Core runtime'�n� i�eren imaj� kullan�yoruz.
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FlightBooker.Search.API.dll"]
