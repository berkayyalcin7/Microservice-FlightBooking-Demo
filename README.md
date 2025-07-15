# FlightBooker - Mikroservis Mimarisi ile U�u� Rezervasyon Sistemi

Bu proje, modern .NET teknolojileri ve mikroservis mimarisi prensipleri kullan�larak s�f�rdan geli�tirilen 
bir u�u� rezervasyon sistemi �rne�idir. Proje, da��t�k sistemlerin temel yap� ta�lar�n� ve desenlerini 
uygulamal� olarak ��retmeyi hedefler.

## B�l�m 2: Mimari ve Sistem Tasar�m�

### Ders 1: API Gateway Deseni ve YARP

Sistemin bu ilk a�amas�nda, mikroservis mimarisinin temelini olu�turan **API Gateway Deseni** uygulanm��t�r.

- **API Gateway Nedir?**
  API Gateway, t�m mikroservislerin �n�nde duran merkezi bir giri� kap�s�d�r. 
- D�� d�nyadan gelen t�m istekler (web, mobil vb.) �nce API Gateway taraf�ndan kar��lan�r ve ard�ndan iste�in i�eri�ine 
- g�re ilgili mikroservise y�nlendirilir. Bu yakla��m, a�a��daki avantajlar� sa�lar:
  - **Basitle�tirilmi� �stemci:** �stemciler, onlarca farkl� servisin adresini bilmek yerine sadece tek bir Gateway adresini bilir.
  - **Merkezi Y�netim:** Kimlik do�rulama, yetkilendirme, 
	- loglama ve rate limiting gibi t�m servisleri ilgilendiren ortak konular bu merkezi noktada y�netilebilir.
  - **G�venlik:** �� a�daki mikroservislerin do�rudan d�� d�nyaya a��lmas� engellenir.

- **Kullan�lan Teknoloji: YARP (Yet Another Reverse Proxy)**
  API Gateway implementasyonu i�in Microsoft taraf�ndan geli�tirilen, 
- .NET ile tam uyumlu, y�ksek performansl� ve esnek bir ters proxy k�t�phanesi olan **YARP** kullan�lm��t�r. 
- Y�nlendirme kurallar� (`Routes`) ve hedef servis gruplar� (`Clusters`), `appsettings.json` 
- dosyas� �zerinden kolayca yap�land�r�lm��t�r.

### Ders 2 & 3: Senkron Servisler Aras� �leti�im
- **Senkron �leti�im:** Bir servisin, bir i�lemi tamamlamak i�in ba�ka bir servisten anl�k olarak cevap beklemesi senaryosu (`Booking.API`'nin `Search.API`'den u�u� do�rulamas� yapmas�) i�lendi.
- **IHttpClientFactory:** .NET'te servisler aras� HTTP istekleri yapman�n modern ve en do�ru yolu olan `IHttpClientFactory` kullan�larak servisler aras� g�venli ve verimli bir ileti�im sa�land�.

### Ders 4: Asenkron �leti�im ve Olay Tabanl� Mimari
- **Olay Tabanl� Mimari (EDA):** Servisler aras� s�k� ba��ml�l��� azaltmak i�in olay tabanl� mimariye ge�i� yap�ld�. `Booking.API` art�k bir rezervasyon sonras� sadece bir "olay" yay�nlayarak di�er servisleri bloke etmeden i�ine devam eder.
- **RabbitMQ:** Servisler aras�nda "postane" g�revi g�ren, end�stri standard� bir mesajla�ma kuyru�u (message broker) sistemi olarak kullan�ld�.
- **MassTransit:** RabbitMQ ile .NET aras�ndaki entegrasyonu son derece basitle�tiren, hata y�netimi gibi bir�ok profesyonel �zelli�i bar�nd�ran �st d�zey bir soyutlama k�t�phanesi entegre edildi.


### Ders 5: Merkezi Kimlik Do�rulama ve G�venlik
- **Merkezi Kimlik Servisi:** T�m kullan�c� do�rulama ve yetkilendirme i�lemlerini y�neten, tek sorumlu bir `Identity.API` servisi olu�turuldu.
- **JWT (JSON Web Token):** G�venli ileti�im i�in standart olan JWT'ler kullan�ld�. `Identity.API`, ba�ar�l� giri� denemelerinde istemcilere imzal� bir `accessToken` �retir.
- **Payla��lan Gizli Anahtar (Shared Secret):** `Identity.API`'nin token'� imzalarken kulland��� gizli anahtar�n ayn�s�, `Booking.API` gibi korumal� servisler taraf�ndan token'� do�rulamak i�in kullan�ld�. Bu g�ven ili�kisi, `appsettings.json` dosyalar� �zerinden y�netildi.
- **Manuel JWT �retimi/Do�rulamas�:** `.NET 8`'in `MapIdentityApi` metodunun mikroservis senaryolar�ndaki zorluklar� nedeniyle, token �retimi ve do�rulamas� standart JWT k�t�phaneleri kullan�larak manuel ve daha net bir �ekilde yap�land�r�ld�.

### Ders 6: Sistemin Dayan�kl�l��� ve Hata Y�netimi (Polly)
Da��t�k sistemler do�alar� gere�i k�r�lgand�r; a�lar yava�layabilir, servisler anl�k olarak cevap vermeyebilir. 
Bu derste, sistemimizi bu t�r ge�ici hatalara kar�� daha diren�li (resilient) hale getirmek i�in **Polly** k�t�phanesi kullan�lm��t�r.

- **Konsept:** Polly, .NET i�in geli�tirilmi� bir dayan�kl�l�k ve ge�ici hata y�netimi k�t�phanesidir. 
- Ba�ar�s�z olabilecek operasyonlar� (�rne�in HTTP istekleri), �nceden tan�mlanm�� sigorta poli�eleri gibi davranan politikalarla sarmalar.
- **Uygulanan Desenler:**
    - **Retry (Tekrar Deneme):** `Booking.API`'nin `Search.API`'ye yapt��� istek anl�k bir hatayla ba�ar�s�z oldu�unda, 
    - sistem hemen pes etmek yerine, iste�i belirli aral�klarla birka� kez daha dener.
    - **Circuit Breaker (Devre Kesici):** E�er `Search.API` s�rekli olarak hata d�nd�r�yorsa, 
    - `Booking.API` bir "sigorta att�r�r" gibi davranarak bir s�reli�ine o servise istek g�ndermeyi tamamen durdurur. 
    - Bu, hem `Booking.API`'nin kaynaklar�n� t�ketmesini engeller hem de sorunlu olan 
    - `Search.API`'nin toparlanmas� i�in ona zaman tan�r. Bu desen, bir servisteki hatan�n t�m sisteme yay�lmas�n� (cascading failure) �nler.


### Ders 7 & 8: G�zlemlenebilirlik (Serilog & OpenTelemetry)
Da��t�k bir sistemde "i�eride neler oluyor?" sorusuna cevap verebilmek i�in g�zlemlenebilirlik altyap�s� kurulmu�tur. Bu, sistemin bir "kara kutu" olmaktan ��k�p, i� i�leyi�inin �effaf bir �ekilde izlenebildi�i bir yap�ya d�n��mesini sa�lar.

- **Yap�sal Loglama (Serilog):** T�m servisler, loglar�n� d�z metin yerine `{"TraceId": "...", "Message": "..."}` 
- gibi aranabilir ve filtrelenebilir JSON format�nda �retecek �ekilde **Serilog** ile yap�land�r�lm��t�r.
- **Merkezi Loglama (Seq):** `Search.API` servisi, �retti�i t�m yap�sal loglar�, bu i� i�in �zel olarak 
- tasarlanm�� olan **Seq** log sunucusuna g�ndermektedir. Bu, t�m loglar�n tek bir yerden, kullan�c� dostu bir aray�zle analiz edilmesini sa�lar.
- **Da��t�k �zleme (OpenTelemetry):** Sisteme giren her iste�e benzersiz bir "�zleme Numaras�" (`TraceId`) 
- atanmas� i�in **OpenTelemetry** standard� entegre edilmi�tir. Bu `TraceId`, iste�in u�rad��� t�m mikroservislere ta��n�r. 
- Bu sayede, tek bir kullan�c� i�leminin t�m sistemdeki yolculu�u ba�tan sona izlenebilir ve hata ay�klama s�reci dramatik �l��de kolayla��r.


### Ders 9: �retime Haz�rl�k - Health Checks ve Rate Limiting
Bu derste, sistemin sadece �al���r durumda de�il, ayn� zamanda "sa�l�kl�" ve "s�rd�r�lebilir" oldu�undan emin olmak i�in iki kritik desen uygulanm��t�r.

- **Health Checks (Sa�l�k Kontrolleri):** Mikroservislerin, d�� d�nyaya kendi sa�l�k durumlar�n� bildiren bir `/health` endpoint'i sunmas� sa�lanm��t�r. 
- Bu, bir servisin veritaban� veya mesaj kuyru�u gibi kritik ba��ml�l�klar�n�n �al���p �al��mad���n� kontrol eder. Kubernetes gibi orkestrasyon ara�lar�, 
- bu endpoint'i kullanarak sa�l�ks�z bir servisi otomatik olarak yeniden ba�latabilir. Bu �zellik i�in `AspNetCore.HealthChecks` k�t�phanesi kullan�lm��t�r.

- **Rate Limiting (�stek S�n�rlama):** Sistemin giri� kap�s� olan `ApiGateway`'e, belirli bir zaman aral���nda kabul edece�i maksimum istek say�s�n� k�s�tlayan bir politika eklenmi�tir. 
- Bu, k�t� niyetli sald�r�lara (DDoS) veya hatal� bir istemcinin yarataca�� a��r� y�ke kar�� sistemi korur. 
- Bu �zellik, .NET 8 ile gelen yerle�ik Rate Limiting middleware'i kullan�larak kolayca uygulanm��t�r.


## B�l�m 3: Da��t�m ve Konteynerle�tirme (Docker)

Bu b�l�mde, geli�tirilen �ok par�al� mikroservis uygulamas�n�n, "benim makinemde �al���yordu" sorununu ortadan kald�racak �ekilde, ta��nabilir ve tutarl� bir yap�da paketlenmesi hedeflenmi�tir.

### Konteynerle�tirme (`Dockerfile` ile)
- **Konsept:** Her bir mikroservis, kendi ba��ml�l�klar� ve �al��ma ortam�yla birlikte izole bir "konteyner" i�erisine paketlenmi�tir. Bu paketleme tarifi, her projenin kendi `Dockerfile`'� i�inde tan�mlanm��t�r.
- **Multi-Stage Builds:** Final imaj boyutunu k���ltmek ve g�venli�i art�rmak i�in, projeyi derleyen `.NET SDK` imaj� ile uygulamay� �al��t�ran `.NET ASP.NET Runtime` imaj�n�n ayr�ld��� �ok a�amal� build tekni�i kullan�lm��t�r.

### Orkestrasyon (`Docker Compose` ile)
- **Konsept:** �ok say�da konteynerden olu�an t�m uygulaman�n (API'ler, veritaban�, mesaj kuyru�u, log sunucusu) tek bir merkezden, tek bir komutla y�netilmesi i�in **Docker Compose** kullan�lm��t�r.
- **Service Discovery:** `docker-compose.yml` i�inde tan�mlanan servisler, Docker taraf�ndan olu�turulan sanal bir a� i�inde yer al�r. Bu sayede servisler birbirlerine `localhost:PORT` gibi adresler yerine, `http://booking-api` veya `rabbitmq` gibi kendi servis adlar�yla eri�irler. `appsettings.json` dosyalar� bu yap�ya uygun olarak g�ncellenmi�tir.
- **Uygulama Ba�lang�� Dayan�kl�l���:** `Identity.API` gibi, ba�lang��ta veritaban�na ba��ml� olan servislerin `Program.cs` dosyas�na **Polly** ile bir "Retry" politikas� eklenmi�tir. Bu sayede, veritaban� konteyneri kendisinden daha yava� aya�a kalksa bile, uygulama ��kmez, sab�rla ba�lant�n�n haz�r olmas�n� bekler.

Bu son ad�m ile birlikte, projemiz sadece mimari olarak de�il, ayn� zamanda geli�tirme ve da��t�m s�re�leri a��s�ndan da modern ve profesyonel bir yap�ya kavu�mu�tur.
