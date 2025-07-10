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
