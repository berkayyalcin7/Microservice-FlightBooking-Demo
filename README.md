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

### Ders 2 & 3: Senkron Servisler Arasý Ýletiþim (HTTP)

Konsept: Bir mikroservisin, bir iþlemi tamamlamak için baþka bir mikroservisten anlýk olarak veri beklemesi senaryosudur.

Uygulanan Senaryo:

Booking.API, bir rezervasyon isteði aldýðýnda, iþlemi onaylamadan önce Search.API'ye doðrudan bir HTTP isteði göndererek ilgili uçuþun var olup olmadýðýný senkron olarak doðrular.

Kullanýlan Teknoloji: IHttpClientFactory

.NET'te servisler arasý HTTP istekleri yapmanýn modern ve en doðru yoludur. HttpClient nesnelerinin ömrünü ve baðlantýlarýný verimli bir þekilde yönetir.

Dezavantaj:

Bu yaklaþým, servisler arasýnda sýký bir anlýk baðýmlýlýk (tight coupling) oluþturur. Search.API ulaþýlamaz olduðunda, Booking.API'nin yeni rezervasyon yapma yeteneði de doðrudan etkilenir.