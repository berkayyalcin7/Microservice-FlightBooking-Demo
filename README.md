# FlightBooker - Mikroservis Mimarisi ile Uçuþ Rezervasyon Sistemi

Bu proje, modern .NET teknolojileri ve mikroservis mimarisi prensipleri kullanýlarak sýfýrdan geliþtirilen 
bir uçuþ rezervasyon sistemi örneðidir. Proje, daðýtýk sistemlerin temel yapý taþlarýný ve desenlerini 
uygulamalý olarak öðretmeyi hedefler.

## Bölüm 2: Mimari ve Sistem Tasarýmý

### Ders 1: API Gateway Deseni ve YARP

Sistemin bu ilk aþamasýnda, mikroservis mimarisinin temelini oluþturan **API Gateway Deseni** uygulanmýþtýr.

- **API Gateway Nedir?**
  API Gateway, tüm mikroservislerin önünde duran merkezi bir giriþ kapýsýdýr. 
- Dýþ dünyadan gelen tüm istekler (web, mobil vb.) önce API Gateway tarafýndan karþýlanýr ve ardýndan isteðin içeriðine 
- göre ilgili mikroservise yönlendirilir. Bu yaklaþým, aþaðýdaki avantajlarý saðlar:
  - **Basitleþtirilmiþ Ýstemci:** Ýstemciler, onlarca farklý servisin adresini bilmek yerine sadece tek bir Gateway adresini bilir.
  - **Merkezi Yönetim:** Kimlik doðrulama, yetkilendirme, 
	- loglama ve rate limiting gibi tüm servisleri ilgilendiren ortak konular bu merkezi noktada yönetilebilir.
  - **Güvenlik:** Ýç aðdaki mikroservislerin doðrudan dýþ dünyaya açýlmasý engellenir.

- **Kullanýlan Teknoloji: YARP (Yet Another Reverse Proxy)**
  API Gateway implementasyonu için Microsoft tarafýndan geliþtirilen, 
- .NET ile tam uyumlu, yüksek performanslý ve esnek bir ters proxy kütüphanesi olan **YARP** kullanýlmýþtýr. 
- Yönlendirme kurallarý (`Routes`) ve hedef servis gruplarý (`Clusters`), `appsettings.json` 
- dosyasý üzerinden kolayca yapýlandýrýlmýþtýr.

### Ders 2 & 3: Senkron Servisler Arasý Ýletiþim
- **Senkron Ýletiþim:** Bir servisin, bir iþlemi tamamlamak için baþka bir servisten anlýk olarak cevap beklemesi senaryosu (`Booking.API`'nin `Search.API`'den uçuþ doðrulamasý yapmasý) iþlendi.
- **IHttpClientFactory:** .NET'te servisler arasý HTTP istekleri yapmanýn modern ve en doðru yolu olan `IHttpClientFactory` kullanýlarak servisler arasý güvenli ve verimli bir iletiþim saðlandý.

### Ders 4: Asenkron Ýletiþim ve Olay Tabanlý Mimari
- **Olay Tabanlý Mimari (EDA):** Servisler arasý sýký baðýmlýlýðý azaltmak için olay tabanlý mimariye geçiþ yapýldý. `Booking.API` artýk bir rezervasyon sonrasý sadece bir "olay" yayýnlayarak diðer servisleri bloke etmeden iþine devam eder.
- **RabbitMQ:** Servisler arasýnda "postane" görevi gören, endüstri standardý bir mesajlaþma kuyruðu (message broker) sistemi olarak kullanýldý.
- **MassTransit:** RabbitMQ ile .NET arasýndaki entegrasyonu son derece basitleþtiren, hata yönetimi gibi birçok profesyonel özelliði barýndýran üst düzey bir soyutlama kütüphanesi entegre edildi.


### Ders 5: Merkezi Kimlik Doðrulama ve Güvenlik
- **Merkezi Kimlik Servisi:** Tüm kullanýcý doðrulama ve yetkilendirme iþlemlerini yöneten, tek sorumlu bir `Identity.API` servisi oluþturuldu.
- **JWT (JSON Web Token):** Güvenli iletiþim için standart olan JWT'ler kullanýldý. `Identity.API`, baþarýlý giriþ denemelerinde istemcilere imzalý bir `accessToken` üretir.
- **Paylaþýlan Gizli Anahtar (Shared Secret):** `Identity.API`'nin token'ý imzalarken kullandýðý gizli anahtarýn aynýsý, `Booking.API` gibi korumalý servisler tarafýndan token'ý doðrulamak için kullanýldý. Bu güven iliþkisi, `appsettings.json` dosyalarý üzerinden yönetildi.
- **Manuel JWT Üretimi/Doðrulamasý:** `.NET 8`'in `MapIdentityApi` metodunun mikroservis senaryolarýndaki zorluklarý nedeniyle, token üretimi ve doðrulamasý standart JWT kütüphaneleri kullanýlarak manuel ve daha net bir þekilde yapýlandýrýldý.

### Ders 6: Sistemin Dayanýklýlýðý ve Hata Yönetimi (Polly)
Daðýtýk sistemler doðalarý gereði kýrýlgandýr; aðlar yavaþlayabilir, servisler anlýk olarak cevap vermeyebilir. 
Bu derste, sistemimizi bu tür geçici hatalara karþý daha dirençli (resilient) hale getirmek için **Polly** kütüphanesi kullanýlmýþtýr.

- **Konsept:** Polly, .NET için geliþtirilmiþ bir dayanýklýlýk ve geçici hata yönetimi kütüphanesidir. 
- Baþarýsýz olabilecek operasyonlarý (örneðin HTTP istekleri), önceden tanýmlanmýþ sigorta poliçeleri gibi davranan politikalarla sarmalar.
- **Uygulanan Desenler:**
    - **Retry (Tekrar Deneme):** `Booking.API`'nin `Search.API`'ye yaptýðý istek anlýk bir hatayla baþarýsýz olduðunda, 
    - sistem hemen pes etmek yerine, isteði belirli aralýklarla birkaç kez daha dener.
    - **Circuit Breaker (Devre Kesici):** Eðer `Search.API` sürekli olarak hata döndürüyorsa, 
    - `Booking.API` bir "sigorta attýrýr" gibi davranarak bir süreliðine o servise istek göndermeyi tamamen durdurur. 
    - Bu, hem `Booking.API`'nin kaynaklarýný tüketmesini engeller hem de sorunlu olan 
    - `Search.API`'nin toparlanmasý için ona zaman tanýr. Bu desen, bir servisteki hatanýn tüm sisteme yayýlmasýný (cascading failure) önler.


### Ders 7 & 8: Gözlemlenebilirlik (Serilog & OpenTelemetry)
Daðýtýk bir sistemde "içeride neler oluyor?" sorusuna cevap verebilmek için gözlemlenebilirlik altyapýsý kurulmuþtur. Bu, sistemin bir "kara kutu" olmaktan çýkýp, iç iþleyiþinin þeffaf bir þekilde izlenebildiði bir yapýya dönüþmesini saðlar.

- **Yapýsal Loglama (Serilog):** Tüm servisler, loglarýný düz metin yerine `{"TraceId": "...", "Message": "..."}` 
- gibi aranabilir ve filtrelenebilir JSON formatýnda üretecek þekilde **Serilog** ile yapýlandýrýlmýþtýr.
- **Merkezi Loglama (Seq):** `Search.API` servisi, ürettiði tüm yapýsal loglarý, bu iþ için özel olarak 
- tasarlanmýþ olan **Seq** log sunucusuna göndermektedir. Bu, tüm loglarýn tek bir yerden, kullanýcý dostu bir arayüzle analiz edilmesini saðlar.
- **Daðýtýk Ýzleme (OpenTelemetry):** Sisteme giren her isteðe benzersiz bir "Ýzleme Numarasý" (`TraceId`) 
- atanmasý için **OpenTelemetry** standardý entegre edilmiþtir. Bu `TraceId`, isteðin uðradýðý tüm mikroservislere taþýnýr. 
- Bu sayede, tek bir kullanýcý iþleminin tüm sistemdeki yolculuðu baþtan sona izlenebilir ve hata ayýklama süreci dramatik ölçüde kolaylaþýr.


### Ders 9: Üretime Hazýrlýk - Health Checks ve Rate Limiting
Bu derste, sistemin sadece çalýþýr durumda deðil, ayný zamanda "saðlýklý" ve "sürdürülebilir" olduðundan emin olmak için iki kritik desen uygulanmýþtýr.

- **Health Checks (Saðlýk Kontrolleri):** Mikroservislerin, dýþ dünyaya kendi saðlýk durumlarýný bildiren bir `/health` endpoint'i sunmasý saðlanmýþtýr. 
- Bu, bir servisin veritabaný veya mesaj kuyruðu gibi kritik baðýmlýlýklarýnýn çalýþýp çalýþmadýðýný kontrol eder. Kubernetes gibi orkestrasyon araçlarý, 
- bu endpoint'i kullanarak saðlýksýz bir servisi otomatik olarak yeniden baþlatabilir. Bu özellik için `AspNetCore.HealthChecks` kütüphanesi kullanýlmýþtýr.

- **Rate Limiting (Ýstek Sýnýrlama):** Sistemin giriþ kapýsý olan `ApiGateway`'e, belirli bir zaman aralýðýnda kabul edeceði maksimum istek sayýsýný kýsýtlayan bir politika eklenmiþtir. 
- Bu, kötü niyetli saldýrýlara (DDoS) veya hatalý bir istemcinin yaratacaðý aþýrý yüke karþý sistemi korur. 
- Bu özellik, .NET 8 ile gelen yerleþik Rate Limiting middleware'i kullanýlarak kolayca uygulanmýþtýr.


## Bölüm 3: Daðýtým ve Konteynerleþtirme (Docker)

Bu bölümde, geliþtirilen çok parçalý mikroservis uygulamasýnýn, "benim makinemde çalýþýyordu" sorununu ortadan kaldýracak þekilde, taþýnabilir ve tutarlý bir yapýda paketlenmesi hedeflenmiþtir.

### Konteynerleþtirme (`Dockerfile` ile)
- **Konsept:** Her bir mikroservis, kendi baðýmlýlýklarý ve çalýþma ortamýyla birlikte izole bir "konteyner" içerisine paketlenmiþtir. Bu paketleme tarifi, her projenin kendi `Dockerfile`'ý içinde tanýmlanmýþtýr.
- **Multi-Stage Builds:** Final imaj boyutunu küçültmek ve güvenliði artýrmak için, projeyi derleyen `.NET SDK` imajý ile uygulamayý çalýþtýran `.NET ASP.NET Runtime` imajýnýn ayrýldýðý çok aþamalý build tekniði kullanýlmýþtýr.

### Orkestrasyon (`Docker Compose` ile)
- **Konsept:** Çok sayýda konteynerden oluþan tüm uygulamanýn (API'ler, veritabaný, mesaj kuyruðu, log sunucusu) tek bir merkezden, tek bir komutla yönetilmesi için **Docker Compose** kullanýlmýþtýr.
- **Service Discovery:** `docker-compose.yml` içinde tanýmlanan servisler, Docker tarafýndan oluþturulan sanal bir að içinde yer alýr. Bu sayede servisler birbirlerine `localhost:PORT` gibi adresler yerine, `http://booking-api` veya `rabbitmq` gibi kendi servis adlarýyla eriþirler. `appsettings.json` dosyalarý bu yapýya uygun olarak güncellenmiþtir.
- **Uygulama Baþlangýç Dayanýklýlýðý:** `Identity.API` gibi, baþlangýçta veritabanýna baðýmlý olan servislerin `Program.cs` dosyasýna **Polly** ile bir "Retry" politikasý eklenmiþtir. Bu sayede, veritabaný konteyneri kendisinden daha yavaþ ayaða kalksa bile, uygulama çökmez, sabýrla baðlantýnýn hazýr olmasýný bekler.

Bu son adým ile birlikte, projemiz sadece mimari olarak deðil, ayný zamanda geliþtirme ve daðýtým süreçleri açýsýndan da modern ve profesyonel bir yapýya kavuþmuþtur.
