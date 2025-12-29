-- ============================================
-- Kütüphane Otomasyonu Sistemi
-- Tam Veritabanı Kurulum Scripti
-- Versiyon: 1.0
-- Ekip: İbrahim Ünal, Burak Çelik
-- ============================================

-- 1. VERİTABANI OLUŞTUR
USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'KutuphaneDB')
BEGIN
    ALTER DATABASE KutuphaneDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE KutuphaneDB;
END
GO

CREATE DATABASE KutuphaneDB;
GO

USE KutuphaneDB;
GO

PRINT N'--- Veritabanı oluşturuldu ---';
GO

-- ============================================
-- 2. TABLOLAR
-- ============================================

-- BOOKS Tablosu
CREATE TABLE dbo.books (
    book_id         INT IDENTITY(1,1) PRIMARY KEY,
    isbn            NVARCHAR(13) NOT NULL UNIQUE,
    title           NVARCHAR(255) NOT NULL,
    author          NVARCHAR(255) NOT NULL,
    publish_year    INT CHECK (publish_year >= 1800 AND publish_year <= YEAR(GETDATE())),
    category        NVARCHAR(100),
    publisher       NVARCHAR(255),
    page_count      INT CHECK (page_count > 0),
    language        NVARCHAR(50) DEFAULT N'Türkçe',
    description     NVARCHAR(MAX),
    created_at      DATETIME2 DEFAULT GETDATE(),
    updated_at      DATETIME2 DEFAULT GETDATE()
);
GO

PRINT N'✓ BOOKS tablosu oluşturuldu';
GO

-- COPIES Tablosu
CREATE TABLE dbo.copies (
    copy_id         INT IDENTITY(1,1) PRIMARY KEY,
    book_id         INT NOT NULL,
    shelf_location  NVARCHAR(50) NOT NULL,
    status          NVARCHAR(20) NOT NULL DEFAULT N'available',
    condition_note  NVARCHAR(MAX),
    acquisition_date DATE DEFAULT CAST(GETDATE() AS DATE),
    price           DECIMAL(10,2) CHECK (price >= 0),
    created_at      DATETIME2 DEFAULT GETDATE(),
    updated_at      DATETIME2 DEFAULT GETDATE(),
    
    CONSTRAINT FK_copies_book FOREIGN KEY (book_id) 
        REFERENCES dbo.books(book_id)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    
    CONSTRAINT CHK_copies_status 
        CHECK (status IN (N'available', N'loaned', N'damaged', N'lost', N'reserved'))
);
GO

PRINT N'✓ COPIES tablosu oluşturuldu';
GO

-- MEMBERS Tablosu
CREATE TABLE dbo.members (
    member_id       INT IDENTITY(1,1) PRIMARY KEY,
    full_name       NVARCHAR(255) NOT NULL,
    email           NVARCHAR(255) NOT NULL UNIQUE,
    phone           NVARCHAR(20),
    address         NVARCHAR(MAX),
    date_of_birth   DATE,
    joined_at       DATE DEFAULT CAST(GETDATE() AS DATE),
    status          NVARCHAR(20) NOT NULL DEFAULT N'active',
    role            NVARCHAR(20) NOT NULL DEFAULT N'member',
    password_hash   NVARCHAR(255),
    max_loan_limit  INT DEFAULT 3 CHECK (max_loan_limit > 0),
    created_at      DATETIME2 DEFAULT GETDATE(),
    updated_at      DATETIME2 DEFAULT GETDATE(),
    
    CONSTRAINT CHK_members_status 
        CHECK (status IN (N'active', N'inactive', N'suspended')),
    
    CONSTRAINT CHK_members_role 
        CHECK (role IN (N'admin', N'librarian', N'member')),
    
    CONSTRAINT CHK_members_email 
        CHECK (email LIKE '%_@__%.__%')
);
GO

PRINT N'✓ MEMBERS tablosu oluşturuldu';
GO

-- LOANS Tablosu
CREATE TABLE dbo.loans (
    loan_id         INT IDENTITY(1,1) PRIMARY KEY,
    copy_id         INT NOT NULL,
    member_id       INT NOT NULL,
    loaned_at       DATETIME2 DEFAULT GETDATE(),
    due_at          DATETIME2 NOT NULL,
    returned_at     DATETIME2 NULL,
    fine_amount     DECIMAL(10,2) DEFAULT 0.00 CHECK (fine_amount >= 0),
    notes           NVARCHAR(MAX),
    created_by      INT,
    created_at      DATETIME2 DEFAULT GETDATE(),
    
    CONSTRAINT FK_loans_copy FOREIGN KEY (copy_id)
        REFERENCES dbo.copies(copy_id)
        ON DELETE NO ACTION
        ON UPDATE CASCADE,
    
    CONSTRAINT FK_loans_member FOREIGN KEY (member_id)
        REFERENCES dbo.members(member_id)
        ON DELETE NO ACTION
        ON UPDATE CASCADE,
    
    CONSTRAINT FK_loans_created_by FOREIGN KEY (created_by)
        REFERENCES dbo.members(member_id)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION,
    
    CONSTRAINT CHK_loans_dates 
        CHECK (returned_at IS NULL OR returned_at >= loaned_at),
    
    CONSTRAINT CHK_loans_due_date 
        CHECK (due_at > loaned_at)
);
GO

PRINT N'✓ LOANS tablosu oluşturuldu';
GO

-- LOAN_HISTORY Tablosu
CREATE TABLE dbo.loan_history (
    history_id      INT IDENTITY(1,1) PRIMARY KEY,
    loan_id         INT NOT NULL,
    action          NVARCHAR(50) NOT NULL,
    action_date     DATETIME2 DEFAULT GETDATE(),
    performed_by    INT,
    old_status      NVARCHAR(20),
    new_status      NVARCHAR(20),
    notes           NVARCHAR(MAX),
    
    CONSTRAINT FK_history_loan FOREIGN KEY (loan_id)
        REFERENCES dbo.loans(loan_id)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    
    CONSTRAINT FK_history_performer FOREIGN KEY (performed_by)
        REFERENCES dbo.members(member_id)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION
);
GO

PRINT N'✓ LOAN_HISTORY tablosu oluşturuldu';
GO

-- ============================================
-- 3. STORED PROCEDURES
-- ============================================

-- sp_AddBook
CREATE OR ALTER PROCEDURE sp_AddBook
    @isbn NVARCHAR(13),
    @title NVARCHAR(255),
    @author NVARCHAR(255),
    @publish_year INT,
    @category NVARCHAR(100) = NULL,
    @publisher NVARCHAR(255) = NULL,
    @page_count INT = NULL,
    @language NVARCHAR(50) = N'Türkçe',
    @description NVARCHAR(MAX) = NULL,
    @book_id INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        IF EXISTS (SELECT 1 FROM dbo.books WHERE isbn = @isbn)
        BEGIN
            RAISERROR(N'Bu ISBN numarası zaten kayıtlı!', 16, 1);
            RETURN;
        END
        
        INSERT INTO dbo.books (isbn, title, author, publish_year, category, publisher, page_count, language, description)
        VALUES (@isbn, @title, @author, @publish_year, @category, @publisher, @page_count, @language, @description);
        
        SET @book_id = SCOPE_IDENTITY();
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

PRINT N'✓ sp_AddBook oluşturuldu';
GO

-- sp_UpdateBook
CREATE OR ALTER PROCEDURE sp_UpdateBook
    @book_id INT,
    @title NVARCHAR(255) = NULL,
    @author NVARCHAR(255) = NULL,
    @publish_year INT = NULL,
    @category NVARCHAR(100) = NULL,
    @publisher NVARCHAR(255) = NULL,
    @page_count INT = NULL,
    @description NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM dbo.books WHERE book_id = @book_id)
        BEGIN
            RAISERROR(N'Kitap bulunamadı!', 16, 1);
            RETURN;
        END
        
        UPDATE dbo.books
        SET 
            title = ISNULL(@title, title),
            author = ISNULL(@author, author),
            publish_year = ISNULL(@publish_year, publish_year),
            category = ISNULL(@category, category),
            publisher = ISNULL(@publisher, publisher),
            page_count = ISNULL(@page_count, page_count),
            description = ISNULL(@description, description)
        WHERE book_id = @book_id;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

PRINT N'✓ sp_UpdateBook oluşturuldu';
GO

-- sp_DeleteBook
CREATE OR ALTER PROCEDURE sp_DeleteBook
    @book_id INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        IF EXISTS (
            SELECT 1 FROM dbo.loans l
            INNER JOIN dbo.copies c ON l.copy_id = c.copy_id
            WHERE c.book_id = @book_id AND l.returned_at IS NULL
        )
        BEGIN
            RAISERROR(N'Bu kitabın aktif ödünçleri var. Silinemez!', 16, 1);
            RETURN;
        END
        
        DELETE FROM dbo.books WHERE book_id = @book_id;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

PRINT N'✓ sp_DeleteBook oluşturuldu';
GO

-- sp_AddCopy
CREATE OR ALTER PROCEDURE sp_AddCopy
    @book_id INT,
    @shelf_location NVARCHAR(50),
    @status NVARCHAR(20) = N'available',
    @price DECIMAL(10,2) = NULL,
    @condition_note NVARCHAR(MAX) = NULL,
    @copy_id INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        IF NOT EXISTS (SELECT 1 FROM dbo.books WHERE book_id = @book_id)
        BEGIN
            RAISERROR(N'Kitap bulunamadı!', 16, 1);
            RETURN;
        END
        
        INSERT INTO dbo.copies (book_id, shelf_location, status, price, condition_note)
        VALUES (@book_id, @shelf_location, @status, @price, @condition_note);
        
        SET @copy_id = SCOPE_IDENTITY();
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

PRINT N'✓ sp_AddCopy oluşturuldu';
GO

-- sp_AddMember
CREATE OR ALTER PROCEDURE sp_AddMember
    @full_name NVARCHAR(255),
    @email NVARCHAR(255),
    @phone NVARCHAR(20) = NULL,
    @address NVARCHAR(MAX) = NULL,
    @date_of_birth DATE = NULL,
    @role NVARCHAR(20) = N'member',
    @password_hash NVARCHAR(255) = NULL,
    @member_id INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        IF EXISTS (SELECT 1 FROM dbo.members WHERE email = @email)
        BEGIN
            RAISERROR(N'Bu e-posta adresi zaten kayıtlı!', 16, 1);
            RETURN;
        END
        
        INSERT INTO dbo.members (full_name, email, phone, address, date_of_birth, role, password_hash)
        VALUES (@full_name, @email, @phone, @address, @date_of_birth, @role, @password_hash);
        
        SET @member_id = SCOPE_IDENTITY();
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

PRINT N'✓ sp_AddMember oluşturuldu';
GO

-- sp_LoanBook
CREATE OR ALTER PROCEDURE sp_LoanBook
    @copy_id INT,
    @member_id INT,
    @created_by INT,
    @loan_days INT = 14,
    @loan_id INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @current_loan_count INT;
    DECLARE @max_loan_limit INT;
    DECLARE @copy_status NVARCHAR(20);
    DECLARE @member_status NVARCHAR(20);
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        SELECT @member_status = status, @max_loan_limit = max_loan_limit
        FROM dbo.members WHERE member_id = @member_id;
        
        IF @member_status IS NULL
        BEGIN
            RAISERROR(N'Üye bulunamadı!', 16, 1);
            RETURN;
        END
        
        IF @member_status != N'active'
        BEGIN
            RAISERROR(N'Üye aktif değil!', 16, 1);
            RETURN;
        END
        
        SELECT @copy_status = status FROM dbo.copies WHERE copy_id = @copy_id;
        
        IF @copy_status IS NULL
        BEGIN
            RAISERROR(N'Kopya bulunamadı!', 16, 1);
            RETURN;
        END
        
        IF @copy_status != N'available'
        BEGIN
            RAISERROR(N'Kopya müsait değil!', 16, 1);
            RETURN;
        END
        
        SELECT @current_loan_count = COUNT(*)
        FROM dbo.loans
        WHERE member_id = @member_id AND returned_at IS NULL;
        
        IF @current_loan_count >= @max_loan_limit
        BEGIN
            RAISERROR(N'Üye limit aşımı!', 16, 1);
            RETURN;
        END
        
        INSERT INTO dbo.loans (copy_id, member_id, loaned_at, due_at, created_by)
        VALUES (
            @copy_id, 
            @member_id, 
            GETDATE(), 
            DATEADD(DAY, @loan_days, GETDATE()),
            @created_by
        );
        
        SET @loan_id = SCOPE_IDENTITY();
        
        UPDATE dbo.copies
        SET status = N'loaned'
        WHERE copy_id = @copy_id;
        
        INSERT INTO dbo.loan_history (loan_id, action, performed_by, old_status, new_status)
        VALUES (@loan_id, N'LOAN_CREATED', @created_by, N'available', N'loaned');
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

PRINT N'✓ sp_LoanBook oluşturuldu';
GO

-- sp_ReturnBook
CREATE OR ALTER PROCEDURE sp_ReturnBook
    @loan_id INT,
    @returned_by INT,
    @fine_amount DECIMAL(10,2) = 0
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @copy_id INT;
    DECLARE @due_at DATETIME2;
    DECLARE @is_late BIT = 0;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        SELECT @copy_id = copy_id, @due_at = due_at
        FROM dbo.loans
        WHERE loan_id = @loan_id AND returned_at IS NULL;
        
        IF @copy_id IS NULL
        BEGIN
            RAISERROR(N'Aktif ödünç kaydı bulunamadı!', 16, 1);
            RETURN;
        END
        
        IF GETDATE() > @due_at
            SET @is_late = 1;
        
        UPDATE dbo.loans
        SET 
            returned_at = GETDATE(),
            fine_amount = @fine_amount
        WHERE loan_id = @loan_id;
        
        UPDATE dbo.copies
        SET status = N'available'
        WHERE copy_id = @copy_id;
        
        INSERT INTO dbo.loan_history (loan_id, action, performed_by, old_status, new_status, notes)
        VALUES (
            @loan_id, 
            N'BOOK_RETURNED', 
            @returned_by, 
            N'loaned', 
            N'available',
            CASE WHEN @is_late = 1 THEN N'Gecikmeli iade' ELSE N'Normal iade' END
        );
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

PRINT N'✓ sp_ReturnBook oluşturuldu';
GO

-- ============================================
-- 4. VIEWS (Görünümler)
-- ============================================

-- vw_active_loans: Aktif ödünçleri ve gecikme durumunu gösterir
CREATE OR ALTER VIEW vw_active_loans AS
SELECT 
    l.loan_id,
    b.title AS book_title,
    b.author,
    m.full_name AS member_name,
    m.email AS member_email,
    l.loaned_at,
    l.due_at,
    CASE WHEN GETDATE() > l.due_at THEN 1 ELSE 0 END AS is_overdue,
    CASE WHEN GETDATE() > l.due_at THEN DATEDIFF(DAY, l.due_at, GETDATE()) ELSE 0 END AS days_overdue
FROM dbo.loans l
INNER JOIN dbo.copies c ON l.copy_id = c.copy_id
INNER JOIN dbo.books b ON c.book_id = b.book_id
INNER JOIN dbo.members m ON l.member_id = m.member_id
WHERE l.returned_at IS NULL;
GO

PRINT N'✓ vw_active_loans view oluşturuldu';
GO

-- vw_top_books_last30: Son 30 günün en çok ödünç verilen kitapları
CREATE OR ALTER VIEW vw_top_books_last30 AS
SELECT TOP 10
    b.book_id,
    b.title,
    b.author,
    b.category,
    COUNT(*) AS loan_count
FROM dbo.loans l
INNER JOIN dbo.copies c ON l.copy_id = c.copy_id
INNER JOIN dbo.books b ON c.book_id = b.book_id
WHERE l.loaned_at >= DATEADD(DAY, -30, GETDATE())
GROUP BY b.book_id, b.title, b.author, b.category
ORDER BY loan_count DESC;
GO

PRINT N'✓ vw_top_books_last30 view oluşturuldu';
GO

-- ============================================
-- 5. ÖRNEK VERİLER (DÜZ METİN ŞİFRE - DEMO İÇİN)
-- ============================================

-- Demo Şifreler (düz metin olarak kaydediliyor):
-- admin123, librarian123, member123

-- Admin kullanıcı (admin123)
INSERT INTO dbo.members (full_name, email, phone, role, status, password_hash)
VALUES (N'Admin User', N'admin@kutuphane.com', N'0555 555 00 00', N'admin', N'active', N'admin123');

-- Kütüphaneci (librarian123)
INSERT INTO dbo.members (full_name, email, phone, role, status, password_hash)
VALUES (N'Ayşe Kütüphaneci', N'ayse@kutuphane.com', N'0555 555 00 01', N'librarian', N'active', N'librarian123');

-- Üyeler (member123)
INSERT INTO dbo.members (full_name, email, phone, role, status, password_hash)
VALUES 
    (N'Ahmet Yılmaz', N'ahmet@email.com', N'0555 111 22 33', N'member', N'active', N'member123'),
    (N'Mehmet Demir', N'mehmet@email.com', N'0555 222 33 44', N'member', N'active', N'member123'),
    (N'Fatma Kaya', N'fatma@email.com', N'0555 333 44 55', N'member', N'active', N'member123'),
    (N'Ali Öztürk', N'ali@email.com', N'0555 444 55 66', N'member', N'active', N'member123'),
    (N'Zeynep Şahin', N'zeynep@email.com', N'0555 555 66 77', N'member', N'active', N'member123');

PRINT N'✓ Örnek üyeler eklendi';
GO

-- Kitaplar
INSERT INTO dbo.books (isbn, title, author, publish_year, category, publisher, page_count, language)
VALUES 
    (N'9789750719387', N'Suç ve Ceza', N'Fyodor Dostoyevski', 1866, N'Roman', N'İş Bankası Kültür Yayınları', 687, N'Türkçe'),
    (N'9789750726439', N'1984', N'George Orwell', 1949, N'Bilim Kurgu', N'Can Yayınları', 352, N'Türkçe'),
    (N'9789750738609', N'Sefiller', N'Victor Hugo', 1862, N'Roman', N'İş Bankası Kültür Yayınları', 1456, N'Türkçe'),
    (N'9789753638029', N'Küçük Prens', N'Antoine de Saint-Exupéry', 1943, N'Çocuk', N'Can Yayınları', 96, N'Türkçe'),
    (N'9789750802942', N'Simyacı', N'Paulo Coelho', 1988, N'Roman', N'Can Yayınları', 184, N'Türkçe'),
    (N'9786053609728', N'Satranç', N'Stefan Zweig', 1942, N'Roman', N'İş Bankası Kültür Yayınları', 80, N'Türkçe'),
    (N'9789750735516', N'Hayvan Çiftliği', N'George Orwell', 1945, N'Bilim Kurgu', N'Can Yayınları', 152, N'Türkçe'),
    (N'9789750806766', N'Dönüşüm', N'Franz Kafka', 1915, N'Roman', N'İş Bankası Kültür Yayınları', 80, N'Türkçe'),
    (N'9789750729188', N'Fareler ve İnsanlar', N'John Steinbeck', 1937, N'Roman', N'Sel Yayıncılık', 120, N'Türkçe'),
    (N'9789750739385', N'Yabancı', N'Albert Camus', 1942, N'Roman', N'Can Yayınları', 128, N'Türkçe');

PRINT N'✓ Örnek kitaplar eklendi';
GO

-- Kitap kopyaları
-- Suç ve Ceza için 3 kopya
INSERT INTO dbo.copies (book_id, shelf_location, status, price)
SELECT book_id, N'A-01-01', N'available', 85.00 FROM dbo.books WHERE isbn = N'9789750719387';
INSERT INTO dbo.copies (book_id, shelf_location, status, price)
SELECT book_id, N'A-01-02', N'available', 85.00 FROM dbo.books WHERE isbn = N'9789750719387';
INSERT INTO dbo.copies (book_id, shelf_location, status, price)
SELECT book_id, N'A-01-03', N'available', 85.00 FROM dbo.books WHERE isbn = N'9789750719387';

-- 1984 için 2 kopya
INSERT INTO dbo.copies (book_id, shelf_location, status, price)
SELECT book_id, N'B-02-01', N'available', 45.00 FROM dbo.books WHERE isbn = N'9789750726439';
INSERT INTO dbo.copies (book_id, shelf_location, status, price)
SELECT book_id, N'B-02-02', N'available', 45.00 FROM dbo.books WHERE isbn = N'9789750726439';

-- Diğer kitaplar için birer kopya
INSERT INTO dbo.copies (book_id, shelf_location, status, price)
SELECT book_id, N'C-03-01', N'available', 120.00 FROM dbo.books WHERE isbn = N'9789750738609';

INSERT INTO dbo.copies (book_id, shelf_location, status, price)
SELECT book_id, N'D-04-01', N'available', 35.00 FROM dbo.books WHERE isbn = N'9789753638029';

INSERT INTO dbo.copies (book_id, shelf_location, status, price)
SELECT book_id, N'E-05-01', N'available', 40.00 FROM dbo.books WHERE isbn = N'9789750802942';

INSERT INTO dbo.copies (book_id, shelf_location, status, price)
SELECT book_id, N'F-06-01', N'available', 25.00 FROM dbo.books WHERE isbn = N'9786053609728';

INSERT INTO dbo.copies (book_id, shelf_location, status, price)
SELECT book_id, N'G-07-01', N'available', 38.00 FROM dbo.books WHERE isbn = N'9789750735516';

INSERT INTO dbo.copies (book_id, shelf_location, status, price)
SELECT book_id, N'H-08-01', N'available', 28.00 FROM dbo.books WHERE isbn = N'9789750806766';

INSERT INTO dbo.copies (book_id, shelf_location, status, price)
SELECT book_id, N'I-09-01', N'available', 32.00 FROM dbo.books WHERE isbn = N'9789750729188';

INSERT INTO dbo.copies (book_id, shelf_location, status, price)
SELECT book_id, N'J-10-01', N'available', 30.00 FROM dbo.books WHERE isbn = N'9789750739385';

PRINT N'✓ Örnek kopyalar eklendi';
GO

-- ============================================
-- 6. ÖZET
-- ============================================
PRINT N'';
PRINT N'═══════════════════════════════════════════';
PRINT N'✓ VERİTABANI KURULUMU TAMAMLANDI!';
PRINT N'═══════════════════════════════════════════';
PRINT N'';
PRINT N'Tablolar: books, copies, members, loans, loan_history';
PRINT N'Views: vw_active_loans, vw_top_books_last30';
PRINT N'Prosedürler: sp_AddBook, sp_UpdateBook, sp_DeleteBook,';
PRINT N'             sp_AddCopy, sp_AddMember, sp_LoanBook, sp_ReturnBook';
PRINT N'';
PRINT N'Örnek Veriler:';
PRINT N'  - 7 Üye (1 admin, 1 kütüphaneci, 5 üye)';
PRINT N'  - 10 Kitap';
PRINT N'  - 13 Kopya';
PRINT N'';
PRINT N'Demo Hesaplar:';
PRINT N'  - admin@kutuphane.com / admin123';
PRINT N'  - ayse@kutuphane.com / librarian123';
PRINT N'  - ahmet@email.com / member123';
PRINT N'';
GO

