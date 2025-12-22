# 📚 Kütüphane Otomasyonu Web Uygulaması

Modern, kullanıcı dostu bir Kütüphane Yönetim Sistemi.

## 🛠️ Teknolojiler

- **Backend**: ASP.NET Core 8.0 MVC
- **ORM**: Entity Framework Core (Database First)
- **Frontend**: Bootstrap 5, DataTables
- **Database**: Microsoft SQL Server
- **Test**: xUnit

## 📋 Ön Gereksinimler

1. **.NET SDK 8.0** veya üzeri
2. **SQL Server** (LocalDB veya Express)
3. **Visual Studio 2022** veya **VS Code**

## 🚀 Kurulum Adımları

### 1. Veritabanını Oluşturun

SQL Server Management Studio (SSMS) veya Azure Data Studio'da:

```sql
CREATE DATABASE KutuphaneDB;
GO
```

Ardından verilen SQL scriptlerini sırasıyla çalıştırın:
- `01_create_database.sql` (Veritabanı)
- `02_create_tables.sql` (Tablolar)
- `04_stored_procedures.sql` (Saklı Yordamlar)
- `05_sample_data.sql` (Örnek Veriler - Opsiyonel)

### 2. Connection String Ayarlayın

`appsettings.json` dosyasını düzenleyin:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=KutuphaneDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

> **Not**: SQL Server Authentication kullanıyorsanız:
> `"Server=localhost;Database=KutuphaneDB;User Id=sa;Password=ŞİFRENİZ;TrustServerCertificate=True;"`

### 3. Demo Kullanıcı Oluşturun

SQL Server'da aşağıdaki sorguyu çalıştırın:

```sql
-- Admin kullanıcısı (şifre: 123456)
INSERT INTO members (full_name, email, role, status, password_hash, max_loan_limit)
VALUES (N'Admin Kullanıcı', 'admin@kutuphane.com', 'admin', 'active', 
        'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 3);

-- Kütüphaneci (şifre: 123456)
INSERT INTO members (full_name, email, role, status, password_hash, max_loan_limit)
VALUES (N'Kütüphaneci', 'kutuphane@kutuphane.com', 'librarian', 'active', 
        'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 3);

-- Normal üye (şifre: 123456)
INSERT INTO members (full_name, email, role, status, password_hash, max_loan_limit)
VALUES (N'Test Üye', 'uye@kutuphane.com', 'member', 'active', 
        'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 3);
```

### 4. Uygulamayı Çalıştırın

```powershell
cd KutuphaneOtomasyon
dotnet restore
dotnet run
```

Tarayıcınızda açın: `https://localhost:5001` veya `http://localhost:5000`

## 🧪 Testleri Çalıştırma

```powershell
cd KutuphaneOtomasyon.Tests
dotnet test --verbosity normal
```

## 👤 Kullanıcı Rolleri

| Rol | E-posta | Şifre | Yetkiler |
|-----|---------|-------|----------|
| Admin | admin@kutuphane.com | 123456 | Tüm yetkiler |
| Kütüphaneci | kutuphane@kutuphane.com | 123456 | Kitap/Üye/Ödünç yönetimi |
| Üye | uye@kutuphane.com | 123456 | Sadece kendi ödünçlerini görüntüleme |

## 📁 Proje Yapısı

```
KutuphaneOtomasyon/
├── Controllers/         # MVC Controller'lar
├── Data/                # DbContext
├── Models/              # Entity sınıfları
├── Services/            # İş mantığı katmanı
├── ViewModels/          # View modelleri
├── Views/               # Razor view'ları
└── wwwroot/             # Statik dosyalar
```

## ✨ Özellikler

- ✅ Rol bazlı giriş sistemi (Admin/Kütüphaneci/Üye)
- ✅ Dashboard istatistikleri
- ✅ Kitap CRUD işlemleri (Stored Procedure ile)
- ✅ Üye yönetimi
- ✅ Ödünç verme / İade alma
- ✅ Gecikme takibi
- ✅ Raporlar (Popüler kitaplar, Üye istatistikleri)
- ✅ DataTables ile arama ve sayfalama
- ✅ Modern responsive tasarım

## 👥 Ekip

- İbrahim Ünal
- Burak Çelik

---

📅 **Tarih**: Aralık 2025  
📚 **Ders**: VTYS 2 - K4 Projesi
