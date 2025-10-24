using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StockManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStockSnapshotsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    SnapshotDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    QuantityInStock = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockSnapshots", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockSnapshots_ProductId_SnapshotDate",
                table: "StockSnapshots",
                columns: new[] { "ProductId", "SnapshotDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockSnapshots");
        }
    }
}
