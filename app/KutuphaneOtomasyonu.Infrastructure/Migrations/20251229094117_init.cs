using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KutuphaneOtomasyonu.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "books",
                columns: table => new
                {
                    book_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    isbn = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    author = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    publish_year = table.Column<int>(type: "int", nullable: true),
                    category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    publisher = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    page_count = table.Column<int>(type: "int", nullable: true),
                    language = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Türkçe"),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_books", x => x.book_id);
                });

            migrationBuilder.CreateTable(
                name: "members",
                columns: table => new
                {
                    member_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    full_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    date_of_birth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    joined_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "active"),
                    role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "member"),
                    password_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    max_loan_limit = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_members", x => x.member_id);
                });

            migrationBuilder.CreateTable(
                name: "copies",
                columns: table => new
                {
                    copy_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    book_id = table.Column<int>(type: "int", nullable: false),
                    shelf_location = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "available"),
                    condition_note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    acquisition_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_copies", x => x.copy_id);
                    table.ForeignKey(
                        name: "FK_copies_books_book_id",
                        column: x => x.book_id,
                        principalTable: "books",
                        principalColumn: "book_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "loans",
                columns: table => new
                {
                    loan_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    copy_id = table.Column<int>(type: "int", nullable: false),
                    member_id = table.Column<int>(type: "int", nullable: false),
                    loaned_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    due_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    returned_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    fine_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_by = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loans", x => x.loan_id);
                    table.ForeignKey(
                        name: "FK_loans_copies_copy_id",
                        column: x => x.copy_id,
                        principalTable: "copies",
                        principalColumn: "copy_id");
                    table.ForeignKey(
                        name: "FK_loans_members_created_by",
                        column: x => x.created_by,
                        principalTable: "members",
                        principalColumn: "member_id");
                    table.ForeignKey(
                        name: "FK_loans_members_member_id",
                        column: x => x.member_id,
                        principalTable: "members",
                        principalColumn: "member_id");
                });

            migrationBuilder.CreateTable(
                name: "loan_history",
                columns: table => new
                {
                    history_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    loan_id = table.Column<int>(type: "int", nullable: false),
                    action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    action_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    performed_by = table.Column<int>(type: "int", nullable: true),
                    old_status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    new_status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loan_history", x => x.history_id);
                    table.ForeignKey(
                        name: "FK_loan_history_loans_loan_id",
                        column: x => x.loan_id,
                        principalTable: "loans",
                        principalColumn: "loan_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_loan_history_members_performed_by",
                        column: x => x.performed_by,
                        principalTable: "members",
                        principalColumn: "member_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_books_isbn",
                table: "books",
                column: "isbn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_copies_book_id",
                table: "copies",
                column: "book_id");

            migrationBuilder.CreateIndex(
                name: "IX_loan_history_loan_id",
                table: "loan_history",
                column: "loan_id");

            migrationBuilder.CreateIndex(
                name: "IX_loan_history_performed_by",
                table: "loan_history",
                column: "performed_by");

            migrationBuilder.CreateIndex(
                name: "IX_loans_copy_id",
                table: "loans",
                column: "copy_id");

            migrationBuilder.CreateIndex(
                name: "IX_loans_created_by",
                table: "loans",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_loans_member_id",
                table: "loans",
                column: "member_id");

            migrationBuilder.CreateIndex(
                name: "IX_members_email",
                table: "members",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "loan_history");

            migrationBuilder.DropTable(
                name: "loans");

            migrationBuilder.DropTable(
                name: "copies");

            migrationBuilder.DropTable(
                name: "members");

            migrationBuilder.DropTable(
                name: "books");
        }
    }
}
