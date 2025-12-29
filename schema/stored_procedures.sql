-- ============================================
-- Kütüphane Otomasyonu Sistemi
-- Stored Procedures, Views & Functions Script
-- ============================================
-- NOT: Bu script tabloların zaten var olduğunu varsayar.
-- Önce setup_database.sql'in tablo kısmını çalıştırın veya
-- aşağıdaki tablolar bölümünü uncomment edin.
-- ============================================

USE KutuphaneDB;
GO

-- ============================================
-- TABLOLAR (Yoksa oluştur)
-- ============================================

-- BOOKS Tablosu
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'books')
BEGIN
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
    PRINT N'✓ BOOKS tablosu oluşturuldu';
END
ELSE
    PRINT N'→ BOOKS tablosu zaten mevcut';
GO

-- COPIES Tablosu
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'copies')
BEGIN
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
    PRINT N'✓ COPIES tablosu oluşturuldu';
END
ELSE
    PRINT N'→ COPIES tablosu zaten mevcut';
GO

-- MEMBERS Tablosu
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'members')
BEGIN
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
    PRINT N'✓ MEMBERS tablosu oluşturuldu';
END
ELSE
    PRINT N'→ MEMBERS tablosu zaten mevcut';
GO

-- LOANS Tablosu
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'loans')
BEGIN
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
    PRINT N'✓ LOANS tablosu oluşturuldu';
END
ELSE
    PRINT N'→ LOANS tablosu zaten mevcut';
GO

-- LOAN_HISTORY Tablosu
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'loan_history')
BEGIN
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
    PRINT N'✓ LOAN_HISTORY tablosu oluşturuldu';
END
ELSE
    PRINT N'→ LOAN_HISTORY tablosu zaten mevcut';
GO

PRINT N'';
PRINT N'--- Tablolar kontrol edildi ---';
PRINT N'';
GO

-- ============================================
-- STORED PROCEDURES
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
            description = ISNULL(@description, description),
            updated_at = GETDATE()
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
        SET status = N'loaned', updated_at = GETDATE()
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
        SET status = N'available', updated_at = GETDATE()
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
-- VIEWS (Görünümler)
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

-- vw_book_availability: Kitap müsaitlik durumu
CREATE OR ALTER VIEW vw_book_availability AS
SELECT 
    b.book_id,
    b.isbn,
    b.title,
    b.author,
    b.category,
    COUNT(c.copy_id) AS total_copies,
    SUM(CASE WHEN c.status = N'available' THEN 1 ELSE 0 END) AS available_copies,
    SUM(CASE WHEN c.status = N'loaned' THEN 1 ELSE 0 END) AS loaned_copies
FROM dbo.books b
LEFT JOIN dbo.copies c ON b.book_id = c.book_id
GROUP BY b.book_id, b.isbn, b.title, b.author, b.category;
GO

PRINT N'✓ vw_book_availability view oluşturuldu';
GO

-- vw_member_loan_summary: Üye ödünç özeti
CREATE OR ALTER VIEW vw_member_loan_summary AS
SELECT 
    m.member_id,
    m.full_name,
    m.email,
    m.status,
    m.max_loan_limit,
    COUNT(CASE WHEN l.returned_at IS NULL THEN 1 END) AS active_loans,
    COUNT(l.loan_id) AS total_loans,
    SUM(ISNULL(l.fine_amount, 0)) AS total_fines
FROM dbo.members m
LEFT JOIN dbo.loans l ON m.member_id = l.member_id
GROUP BY m.member_id, m.full_name, m.email, m.status, m.max_loan_limit;
GO

PRINT N'✓ vw_member_loan_summary view oluşturuldu';
GO

-- ============================================
-- FUNCTIONS (Fonksiyonlar)
-- ============================================

-- fn_GetMemberActiveLoans: Üyenin aktif ödünç sayısını döndürür
CREATE OR ALTER FUNCTION fn_GetMemberActiveLoans(@member_id INT)
RETURNS INT
AS
BEGIN
    DECLARE @count INT;
    SELECT @count = COUNT(*)
    FROM dbo.loans
    WHERE member_id = @member_id AND returned_at IS NULL;
    RETURN ISNULL(@count, 0);
END;
GO

PRINT N'✓ fn_GetMemberActiveLoans function oluşturuldu';
GO

-- fn_CanMemberBorrow: Üyenin ödünç alıp alamayacağını kontrol eder
CREATE OR ALTER FUNCTION fn_CanMemberBorrow(@member_id INT)
RETURNS BIT
AS
BEGIN
    DECLARE @can_borrow BIT = 0;
    DECLARE @status NVARCHAR(20);
    DECLARE @active_loans INT;
    DECLARE @max_limit INT;
    
    SELECT @status = status, @max_limit = max_loan_limit
    FROM dbo.members WHERE member_id = @member_id;
    
    IF @status = N'active'
    BEGIN
        SELECT @active_loans = COUNT(*)
        FROM dbo.loans
        WHERE member_id = @member_id AND returned_at IS NULL;
        
        IF @active_loans < @max_limit
            SET @can_borrow = 1;
    END
    
    RETURN @can_borrow;
END;
GO

PRINT N'✓ fn_CanMemberBorrow function oluşturuldu';
GO

-- fn_CalculateFine: Gecikme cezası hesaplar (günlük 5 TL)
CREATE OR ALTER FUNCTION fn_CalculateFine(@loan_id INT)
RETURNS DECIMAL(10,2)
AS
BEGIN
    DECLARE @fine DECIMAL(10,2) = 0;
    DECLARE @due_at DATETIME2;
    DECLARE @days_overdue INT;
    
    SELECT @due_at = due_at
    FROM dbo.loans
    WHERE loan_id = @loan_id AND returned_at IS NULL;
    
    IF @due_at IS NOT NULL AND GETDATE() > @due_at
    BEGIN
        SET @days_overdue = DATEDIFF(DAY, @due_at, GETDATE());
        SET @fine = @days_overdue * 5.00; -- Günlük 5 TL ceza
    END
    
    RETURN @fine;
END;
GO

PRINT N'✓ fn_CalculateFine function oluşturuldu';
GO

-- fn_GetBookAvailableCopies: Kitabın müsait kopya sayısını döndürür
CREATE OR ALTER FUNCTION fn_GetBookAvailableCopies(@book_id INT)
RETURNS INT
AS
BEGIN
    DECLARE @count INT;
    SELECT @count = COUNT(*)
    FROM dbo.copies
    WHERE book_id = @book_id AND status = N'available';
    RETURN ISNULL(@count, 0);
END;
GO

PRINT N'✓ fn_GetBookAvailableCopies function oluşturuldu';
GO

-- fn_GetOverdueLoans: Tüm gecikmiş ödünçleri döndüren table-valued function
CREATE OR ALTER FUNCTION fn_GetOverdueLoans()
RETURNS TABLE
AS
RETURN
(
    SELECT 
        l.loan_id,
        b.title AS book_title,
        m.full_name AS member_name,
        m.email,
        m.phone,
        l.loaned_at,
        l.due_at,
        DATEDIFF(DAY, l.due_at, GETDATE()) AS days_overdue,
        DATEDIFF(DAY, l.due_at, GETDATE()) * 5.00 AS estimated_fine
    FROM dbo.loans l
    INNER JOIN dbo.copies c ON l.copy_id = c.copy_id
    INNER JOIN dbo.books b ON c.book_id = b.book_id
    INNER JOIN dbo.members m ON l.member_id = m.member_id
    WHERE l.returned_at IS NULL AND GETDATE() > l.due_at
);
GO

PRINT N'✓ fn_GetOverdueLoans table function oluşturuldu';
GO

-- fn_SearchBooks: Kitap arama fonksiyonu
CREATE OR ALTER FUNCTION fn_SearchBooks(@search_term NVARCHAR(255))
RETURNS TABLE
AS
RETURN
(
    SELECT 
        b.book_id,
        b.isbn,
        b.title,
        b.author,
        b.category,
        b.publish_year,
        b.publisher,
        (SELECT COUNT(*) FROM dbo.copies c WHERE c.book_id = b.book_id AND c.status = N'available') AS available_copies
    FROM dbo.books b
    WHERE 
        b.title LIKE N'%' + @search_term + N'%' OR
        b.author LIKE N'%' + @search_term + N'%' OR
        b.isbn LIKE N'%' + @search_term + N'%' OR
        b.category LIKE N'%' + @search_term + N'%'
);
GO

PRINT N'✓ fn_SearchBooks table function oluşturuldu';
GO

-- ============================================
-- ÖZET
-- ============================================
PRINT N'';
PRINT N'═══════════════════════════════════════════';
PRINT N'✓ TÜM NESNELER OLUŞTURULDU!';
PRINT N'═══════════════════════════════════════════';
PRINT N'';
PRINT N'Tablolar: books, copies, members, loans, loan_history';
PRINT N'';
PRINT N'Prosedürler:';
PRINT N'  - sp_AddBook';
PRINT N'  - sp_UpdateBook';
PRINT N'  - sp_DeleteBook';
PRINT N'  - sp_AddCopy';
PRINT N'  - sp_AddMember';
PRINT N'  - sp_LoanBook';
PRINT N'  - sp_ReturnBook';
PRINT N'';
PRINT N'Views:';
PRINT N'  - vw_active_loans';
PRINT N'  - vw_top_books_last30';
PRINT N'  - vw_book_availability';
PRINT N'  - vw_member_loan_summary';
PRINT N'';
PRINT N'Functions:';
PRINT N'  - fn_GetMemberActiveLoans';
PRINT N'  - fn_CanMemberBorrow';
PRINT N'  - fn_CalculateFine';
PRINT N'  - fn_GetBookAvailableCopies';
PRINT N'  - fn_GetOverdueLoans (table)';
PRINT N'  - fn_SearchBooks (table)';
PRINT N'';
GO
