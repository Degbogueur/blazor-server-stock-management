using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditAndSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Operations");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "Operations");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "InventoryRows");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "InventoryRows");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "Inventories");

            migrationBuilder.RenameColumn(
                name: "DeletedOn",
                table: "Suppliers",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Suppliers",
                newName: "DeletedById");

            migrationBuilder.RenameColumn(
                name: "DeletedOn",
                table: "Products",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Products",
                newName: "DeletedById");

            migrationBuilder.RenameColumn(
                name: "LastUpdatedOn",
                table: "Operations",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "LastUpdatedBy",
                table: "Operations",
                newName: "UpdatedById");

            migrationBuilder.RenameColumn(
                name: "DeletedOn",
                table: "Operations",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Operations",
                newName: "DeletedById");

            migrationBuilder.RenameColumn(
                name: "LastUpdatedOn",
                table: "InventoryRows",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "LastUpdatedBy",
                table: "InventoryRows",
                newName: "UpdatedById");

            migrationBuilder.RenameColumn(
                name: "DeletedOn",
                table: "InventoryRows",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "InventoryRows",
                newName: "DeletedById");

            migrationBuilder.RenameColumn(
                name: "LastUpdatedOn",
                table: "Inventories",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "LastUpdatedBy",
                table: "Inventories",
                newName: "UpdatedById");

            migrationBuilder.RenameColumn(
                name: "DeletedOn",
                table: "Inventories",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Inventories",
                newName: "DeletedById");

            migrationBuilder.RenameColumn(
                name: "DeletedOn",
                table: "Employees",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Employees",
                newName: "DeletedById");

            migrationBuilder.RenameColumn(
                name: "DeletedOn",
                table: "Categories",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Categories",
                newName: "DeletedById");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Operations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "Operations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "InventoryRows",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "InventoryRows",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Inventories",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "Inventories",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_DeletedById",
                table: "Suppliers",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_IsDeleted",
                table: "Suppliers",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Products_DeletedById",
                table: "Products",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsDeleted",
                table: "Products",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_CreatedAt",
                table: "Operations",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_CreatedById",
                table: "Operations",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_DeletedById",
                table: "Operations",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_IsDeleted",
                table: "Operations",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_UpdatedAt",
                table: "Operations",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_UpdatedById",
                table: "Operations",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryRows_CreatedAt",
                table: "InventoryRows",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryRows_CreatedById",
                table: "InventoryRows",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryRows_DeletedById",
                table: "InventoryRows",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryRows_IsDeleted",
                table: "InventoryRows",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryRows_UpdatedAt",
                table: "InventoryRows",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryRows_UpdatedById",
                table: "InventoryRows",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_CreatedAt",
                table: "Inventories",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_CreatedById",
                table: "Inventories",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_DeletedById",
                table: "Inventories",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_IsDeleted",
                table: "Inventories",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_UpdatedAt",
                table: "Inventories",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_UpdatedById",
                table: "Inventories",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DeletedById",
                table: "Employees",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_IsDeleted",
                table: "Employees",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_DeletedById",
                table: "Categories",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_IsDeleted",
                table: "Categories",
                column: "IsDeleted");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Users_DeletedById",
                table: "Categories",
                column: "DeletedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Users_DeletedById",
                table: "Employees",
                column: "DeletedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Users_CreatedById",
                table: "Inventories",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Users_DeletedById",
                table: "Inventories",
                column: "DeletedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Users_UpdatedById",
                table: "Inventories",
                column: "UpdatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryRows_Users_CreatedById",
                table: "InventoryRows",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryRows_Users_DeletedById",
                table: "InventoryRows",
                column: "DeletedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryRows_Users_UpdatedById",
                table: "InventoryRows",
                column: "UpdatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Operations_Users_CreatedById",
                table: "Operations",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Operations_Users_DeletedById",
                table: "Operations",
                column: "DeletedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Operations_Users_UpdatedById",
                table: "Operations",
                column: "UpdatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Users_DeletedById",
                table: "Products",
                column: "DeletedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Suppliers_Users_DeletedById",
                table: "Suppliers",
                column: "DeletedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Users_DeletedById",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Users_DeletedById",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Users_CreatedById",
                table: "Inventories");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Users_DeletedById",
                table: "Inventories");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Users_UpdatedById",
                table: "Inventories");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryRows_Users_CreatedById",
                table: "InventoryRows");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryRows_Users_DeletedById",
                table: "InventoryRows");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryRows_Users_UpdatedById",
                table: "InventoryRows");

            migrationBuilder.DropForeignKey(
                name: "FK_Operations_Users_CreatedById",
                table: "Operations");

            migrationBuilder.DropForeignKey(
                name: "FK_Operations_Users_DeletedById",
                table: "Operations");

            migrationBuilder.DropForeignKey(
                name: "FK_Operations_Users_UpdatedById",
                table: "Operations");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Users_DeletedById",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Suppliers_Users_DeletedById",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_DeletedById",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_IsDeleted",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Products_DeletedById",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_IsDeleted",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Operations_CreatedAt",
                table: "Operations");

            migrationBuilder.DropIndex(
                name: "IX_Operations_CreatedById",
                table: "Operations");

            migrationBuilder.DropIndex(
                name: "IX_Operations_DeletedById",
                table: "Operations");

            migrationBuilder.DropIndex(
                name: "IX_Operations_IsDeleted",
                table: "Operations");

            migrationBuilder.DropIndex(
                name: "IX_Operations_UpdatedAt",
                table: "Operations");

            migrationBuilder.DropIndex(
                name: "IX_Operations_UpdatedById",
                table: "Operations");

            migrationBuilder.DropIndex(
                name: "IX_InventoryRows_CreatedAt",
                table: "InventoryRows");

            migrationBuilder.DropIndex(
                name: "IX_InventoryRows_CreatedById",
                table: "InventoryRows");

            migrationBuilder.DropIndex(
                name: "IX_InventoryRows_DeletedById",
                table: "InventoryRows");

            migrationBuilder.DropIndex(
                name: "IX_InventoryRows_IsDeleted",
                table: "InventoryRows");

            migrationBuilder.DropIndex(
                name: "IX_InventoryRows_UpdatedAt",
                table: "InventoryRows");

            migrationBuilder.DropIndex(
                name: "IX_InventoryRows_UpdatedById",
                table: "InventoryRows");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_CreatedAt",
                table: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_CreatedById",
                table: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_DeletedById",
                table: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_IsDeleted",
                table: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_UpdatedAt",
                table: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_UpdatedById",
                table: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_Employees_DeletedById",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_IsDeleted",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Categories_DeletedById",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_IsDeleted",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Operations");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Operations");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "InventoryRows");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "InventoryRows");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Inventories");

            migrationBuilder.RenameColumn(
                name: "DeletedById",
                table: "Suppliers",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "Suppliers",
                newName: "DeletedOn");

            migrationBuilder.RenameColumn(
                name: "DeletedById",
                table: "Products",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "Products",
                newName: "DeletedOn");

            migrationBuilder.RenameColumn(
                name: "UpdatedById",
                table: "Operations",
                newName: "LastUpdatedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Operations",
                newName: "LastUpdatedOn");

            migrationBuilder.RenameColumn(
                name: "DeletedById",
                table: "Operations",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "Operations",
                newName: "DeletedOn");

            migrationBuilder.RenameColumn(
                name: "UpdatedById",
                table: "InventoryRows",
                newName: "LastUpdatedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "InventoryRows",
                newName: "LastUpdatedOn");

            migrationBuilder.RenameColumn(
                name: "DeletedById",
                table: "InventoryRows",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "InventoryRows",
                newName: "DeletedOn");

            migrationBuilder.RenameColumn(
                name: "UpdatedById",
                table: "Inventories",
                newName: "LastUpdatedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Inventories",
                newName: "LastUpdatedOn");

            migrationBuilder.RenameColumn(
                name: "DeletedById",
                table: "Inventories",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "Inventories",
                newName: "DeletedOn");

            migrationBuilder.RenameColumn(
                name: "DeletedById",
                table: "Employees",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "Employees",
                newName: "DeletedOn");

            migrationBuilder.RenameColumn(
                name: "DeletedById",
                table: "Categories",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "Categories",
                newName: "DeletedOn");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Operations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "Operations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "InventoryRows",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "InventoryRows",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Inventories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "Inventories",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
