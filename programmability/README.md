# Veritabanı Programlanabilir Nesneleri

Bu klasör SQL Server'da kullanılan programlanabilir nesneleri içerir:

- **Stored Procedures (SP)** - Veritabanı işlemleri
- **Views** - Raporlama görünümleri  
- **Functions** - Yardımcı fonksiyonlar

## Mevcut SP'ler

| SP Adı | Açıklama |
|--------|----------|
| sp_AddBook | Yeni kitap ekleme |
| sp_UpdateBook | Kitap güncelleme |
| sp_DeleteBook | Kitap silme |
| sp_AddCopy | Kitap kopyası ekleme |
| sp_AddMember | Yeni üye ekleme |
| sp_LoanBook | Kitap ödünç verme |
| sp_ReturnBook | Kitap iade işlemi |

Detaylı SQL kodları için `schema/setup_database.sql` dosyasına bakın.
