# BusX - Otobüs Biletleme Sistemi

Özellikler ve Mimari Kararlar

Proje **Domain-Driven Design (DDD)** odaklı, katmanlı bir yapıya sahiptir:

* **API Layer:** Minimal Controller yapısı, Swagger entegrasyonu.
* **Infrastructure:** EF Core, Repository Pattern (Service üzerinden), Database Migrations.
* **Domain:** Saf POCO sınıfları, Enums, DTOs.

### Öne Çıkan Teknik Detaylar

1.  **Concurrency (Eşzamanlılık) Yönetimi:**
    * Aynı koltuğa aynı anda yapılan isteklerde **Optimistic Concurrency** uygulanmıştır.
    * `Seat` entity'sindeki `Version` alanı kullanılarak veri tutarlılığı sağlanır.
    * Çakışma durumunda `409 Conflict` döner.

2.  **Transaction Yönetimi:**
    * Bilet satın alma işlemi `BeginTransactionAsync` ile yönetilir. Ödeme veya kayıt hatasında tüm işlemler `Rollback` edilir.

3.  **Performans ve Caching:**
    * Sefer arama (`SearchJourneys`) gibi sık kullanılan sorgular `IMemoryCache` ile 60 saniye boyunca önbelleğe alınır.
    * Okuma işlemlerinde `AsNoTracking()` kullanılarak EF Core performansı optimize edilmiştir.

4.  **Gözlemlenebilirlik (Observability):**
    * Her isteğe benzersiz bir **Correlation-Id** atanır ve loglama altyapısına işlenir.
    * `/health` endpoint'i ile sistem sağlığı izlenebilir.

## Kurulum ve Çalıştırma

Proje **SQLite** kullandığı için herhangi bir veritabanı kurulumuna ihtiyaç duymaz.

1.  Repoyu klonlayın.
2.  Terminali `BusXSystem` klasöründe açın.
3.  Uygulamayı başlatın:

```bash
dotnet run --project BusX.Api