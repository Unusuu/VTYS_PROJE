# 📚 Kütüphane Otomasyonu

Modern .NET 10 tabanlı kütüphane yönetim sistemi. Clean Architecture prensipleri ile geliştirilmiştir.

## � Proje Yapısı

```
KutuphaneOtomasyonu/
├── app/                                    # Uygulama katmanları
│   ├── KutuphaneOtomasyonu.API/            # ASP.NET Core Web API / MVC
│   ├── KutuphaneOtomasyonu.Application/    # İş mantığı ve servisler
│   ├── KutuphaneOtomasyonu.Domain/         # Entity ve arayüzler
│   ├── KutuphaneOtomasyonu.Infrastructure/ # Veritabanı ve dış servisler
│   └── frontend/                           # Frontend (Next.js - opsiyonel)
├── docs/                                   # Dokümantasyon
├── programmability/                        # Veritabanı nesneleri (SP, View, Function)
├── schema/                                 # Veritabanı şeması
├── tests/                                  # Test dosyaları
└── KutuphaneOtomasyonu.slnx               # Solution dosyası
```

## 🚀 Başlangıç

### Gereksinimler

- .NET 10.0 SDK
- SQL Server (LocalDB veya SQL Express)

### Kurulum

1. Projeyi klonlayın:
```bash
git clone <repo-url>
cd KutuphaneOtomasyonu
```

2. Veritabanını oluşturun:
```bash
# SQL Server Management Studio'da schema/setup_database.sql dosyasını çalıştırın
```

3. Uygulamayı çalıştırın:
```bash
dotnet run --project app/KutuphaneOtomasyonu.API
```

4. Tarayıcıda açın: `https://localhost:5001`

## 🏗️ Mimari

Proje **Clean Architecture** prensiplerini takip eder:

| Katman | Açıklama |
|--------|----------|
| **Domain** | Entity'ler ve temel iş kuralları |
| **Application** | Servis arayüzleri, DTO'lar ve iş mantığı |
| **Infrastructure** | Veritabanı bağlantısı, DbContext |
| **API** | Controller'lar, View'lar, Program.cs |

## 📦 Özellikler

- ✅ Kitap yönetimi (CRUD)
- ✅ Üye yönetimi
- ✅ Ödünç verme/iade işlemleri
- ✅ Raporlama
- ✅ Kimlik doğrulama (Cookie Authentication)
- ✅ Yetkilendirme (Admin, Librarian, Member rolleri)

## 🧪 Testler

```bash
dotnet test
```

## 📄 Lisans

MIT
