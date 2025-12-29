# Kütüphane Otomasyonu Sistemi
## Veritabanı Yönetim Sistemleri Proje Raporu

**Ekip Üyeleri:** İbrahim Ünal, Burak Çelik  
**Tarih:** 29 Aralık 2024  
**Veritabanı:** Microsoft SQL Server  
**Uygulama:** ASP.NET Core MVC

---

## 1. Proje Tanımı

Kütüphane Otomasyonu Sistemi, bir kütüphanenin tüm operasyonlarını dijital ortamda yönetmek için geliştirilmiş kapsamlı bir web uygulamasıdır. Sistem; kitap yönetimi, üye yönetimi, ödünç verme/iade işlemleri ve raporlama modüllerini içermektedir.

### 1.1 Amaç
- Kütüphane kaynaklarının etkin yönetimi
- Üye işlemlerinin takibi
- Ödünç verme süreçlerinin otomasyonu
- Gecikme cezası hesaplama
- Raporlama ve istatistik

---

## 2. Veritabanı Yapısı

### 2.1 Tablolar (5 Adet)

| Tablo | Açıklama | Temel Alanlar |
|-------|----------|---------------|
| **books** | Kitap bilgileri | book_id, isbn, title, author, category, publisher |
| **copies** | Kitap kopyaları | copy_id, book_id, shelf_location, status, price |
| **members** | Üye bilgileri | member_id, full_name, email, role, status |
| **loans** | Ödünç kayıtları | loan_id, copy_id, member_id, loaned_at, due_at |
| **loan_history** | İşlem geçmişi | history_id, loan_id, action, performed_by |

### 2.2 Entity-Relationship (ER) Diyagramı

```
┌─────────────┐       ┌─────────────┐       ┌─────────────┐
│   BOOKS     │1     N│   COPIES    │1     N│   LOANS     │
│─────────────│───────│─────────────│───────│─────────────│
│ book_id(PK) │       │ copy_id(PK) │       │ loan_id(PK) │
│ isbn        │       │ book_id(FK) │       │ copy_id(FK) │
│ title       │       │ status      │       │ member_id(FK│
│ author      │       │ location    │       │ due_at      │
└─────────────┘       └─────────────┘       └──────┬──────┘
                                                   │N
                                                   │
                                            ┌──────┴──────┐
                                            │   MEMBERS   │
                                            │─────────────│
                                            │ member_id(PK│
                                            │ full_name   │
                                            │ email       │
                                            │ role        │
                                            └─────────────┘
```

### 2.3 Kısıtlamalar (Constraints)

- **Primary Key:** Her tabloda benzersiz kimlik
- **Foreign Key:** İlişkisel bütünlük
- **Check Constraints:**
  - `status IN ('available', 'loaned', 'damaged', 'lost')`
  - `role IN ('admin', 'librarian', 'member')`
  - `publish_year >= 1800`
- **Unique:** ISBN, email alanları

---

## 3. Stored Procedures (7 Adet)

| Prosedür | İşlev | Parametreler |
|----------|-------|--------------|
| **sp_AddBook** | Yeni kitap ekler | @isbn, @title, @author, @publish_year |
| **sp_UpdateBook** | Kitap günceller | @book_id, @title, @author... |
| **sp_DeleteBook** | Kitap siler | @book_id |
| **sp_AddCopy** | Kopya ekler | @book_id, @shelf_location |
| **sp_AddMember** | Üye kaydı | @full_name, @email, @role |
| **sp_LoanBook** | Ödünç verir | @copy_id, @member_id, @loan_days |
| **sp_ReturnBook** | İade alır | @loan_id, @fine_amount |

### Örnek: sp_LoanBook
```sql
-- Ödünç verme prosedürü
-- Üye limiti, kopya durumu ve aktif ödünç kontrolü yapar
CREATE PROCEDURE sp_LoanBook
    @copy_id INT,
    @member_id INT,
    @loan_days INT = 14
AS
BEGIN
    -- Üye aktiflik kontrolü
    -- Kopya müsaitlik kontrolü
    -- Limit aşımı kontrolü
    -- Ödünç kaydı oluşturma
    -- Kopya durumu güncelleme
    -- Geçmiş kaydı ekleme
END
```

---

## 4. Views (Görünümler) - 4 Adet

| View | Açıklama |
|------|----------|
| **vw_active_loans** | Aktif ödünçler ve gecikme durumu |
| **vw_top_books_last30** | Son 30 günün en popüler kitapları |
| **vw_book_availability** | Kitap müsaitlik durumu |
| **vw_member_loan_summary** | Üye ödünç özeti |

---

## 5. Functions (Fonksiyonlar) - 6 Adet

### Scalar Functions
| Fonksiyon | Dönen Değer |
|-----------|-------------|
| **fn_GetMemberActiveLoans** | Üyenin aktif ödünç sayısı |
| **fn_CanMemberBorrow** | Ödünç alabilir mi? (BIT) |
| **fn_CalculateFine** | Gecikme cezası (5₺/gün) |
| **fn_GetBookAvailableCopies** | Müsait kopya sayısı |

### Table-Valued Functions
| Fonksiyon | Dönen Tablo |
|-----------|-------------|
| **fn_GetOverdueLoans** | Tüm gecikmiş ödünçler |
| **fn_SearchBooks** | Arama sonuçları |

---

## 6. Web Uygulaması

### 6.1 Teknolojiler
- **Backend:** ASP.NET Core 8.0 MVC
- **Frontend:** HTML5, CSS3, JavaScript
- **Veritabanı:** SQL Server Express
- **ORM:** Entity Framework Core

### 6.2 Modüller (Controllers)

| Controller | Sayfa Sayısı | İşlevler |
|------------|--------------|----------|
| **HomeController** | 2 | Ana sayfa, Dashboard |
| **AccountController** | 4 | Giriş, Kayıt, Profil |
| **BooksController** | 4 | CRUD işlemleri |
| **MembersController** | 3 | Üye yönetimi |
| **LoansController** | 2 | Ödünç işlemleri |
| **MemberDashboardController** | 5 | Üye paneli |
| **ReportsController** | 1 | Raporlar |

### 6.3 Kullanıcı Rolleri

| Rol | Yetkiler |
|-----|----------|
| **Admin** | Tam yetki, sistem yönetimi |
| **Librarian** | Kitap/üye yönetimi, ödünç işlemleri |
| **Member** | Kendi ödünçlerini görüntüleme |

---

## 7. Güvenlik Özellikleri

- ✅ SQL Injection koruması (Parameterized queries)
- ✅ Rol bazlı yetkilendirme
- ✅ Şifre hashleme
- ✅ Transaction yönetimi
- ✅ Veri bütünlüğü kontrolleri

---

## 8. Örnek Kullanım Senaryoları

### Senaryo 1: Kitap Ödünç Verme
1. Kütüphaneci sisteme giriş yapar
2. Üye bilgilerini kontrol eder
3. Kitap kopyasını seçer
4. `sp_LoanBook` prosedürü çalışır
5. Kopya durumu "loaned" olur
6. `loan_history` tablosuna kayıt eklenir

### Senaryo 2: Gecikmiş Kitap İadesi
1. Üye kitabı iade eder
2. `fn_CalculateFine` ceza hesaplar
3. `sp_ReturnBook` prosedürü çalışır
4. Ceza tutarı kaydedilir
5. Kopya durumu "available" olur

---

## 9. Kurulum

```bash
# 1. Veritabanı oluşturma
sqlcmd -S .\SQLEXPRESS -i schema/setup_database.sql

# 2. Uygulama çalıştırma
cd app/KutuphaneOtomasyonu.Web
dotnet run
```

**Demo Hesapları:**
- Admin: `admin@kutuphane.com` / `admin123`
- Kütüphaneci: `ayse@kutuphane.com` / `librarian123`
- Üye: `ahmet@email.com` / `member123`

---

## 10. Sonuç

Kütüphane Otomasyonu Sistemi, modern web teknolojileri ve ilişkisel veritabanı yönetim sistemlerini kullanarak kütüphane operasyonlarını etkin bir şekilde yönetmektedir. Stored procedure'ler ile iş mantığı veritabanı katmanında tutularak performans ve güvenlik sağlanmıştır.

---

**Proje Deposu:** GitHub - VTYS_PROJE  
**Toplam Kod Satırı:** ~3000+  
**Toplam Tablo:** 5 | **Stored Procedure:** 7 | **View:** 4 | **Function:** 6
