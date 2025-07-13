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