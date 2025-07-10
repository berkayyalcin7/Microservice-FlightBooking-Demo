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

### Ders 2 & 3: Senkron Servisler Aras� �leti�im (HTTP)

Konsept: Bir mikroservisin, bir i�lemi tamamlamak i�in ba�ka bir mikroservisten anl�k olarak veri beklemesi senaryosudur.

Uygulanan Senaryo:

Booking.API, bir rezervasyon iste�i ald���nda, i�lemi onaylamadan �nce Search.API'ye do�rudan bir HTTP iste�i g�ndererek ilgili u�u�un var olup olmad���n� senkron olarak do�rular.

Kullan�lan Teknoloji: IHttpClientFactory

.NET'te servisler aras� HTTP istekleri yapman�n modern ve en do�ru yoludur. HttpClient nesnelerinin �mr�n� ve ba�lant�lar�n� verimli bir �ekilde y�netir.

Dezavantaj:

Bu yakla��m, servisler aras�nda s�k� bir anl�k ba��ml�l�k (tight coupling) olu�turur. 
Search.API ula��lamaz oldu�unda, Booking.API'nin yeni rezervasyon yapma yetene�i de do�rudan etkilenir.

### Ders 4: Asenkron �leti�im ve Olay Tabanl� Mimari

Konsept: Senkron ileti�imin getirdi�i k�r�lganl��� azaltmak ve servisleri birbirinden ba��ms�zla�t�rmak i�in 
Olay Tabanl� Mimari (Event-Driven Architecture) deseni uygulanm��t�r. Servisler birbirine do�rudan komut vermek yerine, bir olay oldu�unda bunu etrafa duyurur ve ilgilenen di�er servisler bu olay� dinler.

Uygulanan Senaryo:

Booking.API, bir rezervasyon olu�turduktan sonra, 
bir BookingCreatedEvent (Rezervasyon Olu�turuldu Olay�) mesaj� yay�nlar.

Notification.Service ad�nda yeni bir arka plan servisi, 
bu olaylar� dinleyerek kullan�c�ya onay e-postas� g�nderme gibi ikincil i�lemleri, di�er servisleri bloke etmeden, kendi zaman�nda ger�ekle�tirir.

Kullan�lan Teknolojiler:

RabbitMQ: Servisler aras�nda "postane" g�revi g�ren, end�stri standard� bir mesajla�ma kuyru�u (message broker) sistemidir.

MassTransit: RabbitMQ gibi sistemlerle �al��may� 
.NET �zerinde son derece kolayla�t�ran, hata y�netimi ve yap�land�rma gibi bir�ok detay� otomatikle�tiren �st d�zey bir soyutlama k�t�phanesidir.