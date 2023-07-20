using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryTerminal.Migrations
{
    /// <inheritdoc />
    public partial class AddReturns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Return",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<long>(type: "bigint", nullable: false),
                    SupplierId = table.Column<long>(type: "bigint", nullable: false),
                    SendDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReceiveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReturnDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RepName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Return", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Return_Partner_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Partner",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Return_Partner_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Partner",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SupplierReturn",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierId = table.Column<long>(type: "bigint", nullable: false),
                    WarningCount = table.Column<int>(type: "int", nullable: false),
                    LastWarning = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierReturn", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierReturn_Partner_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Partner",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Return_ClientId",
                table: "Return",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Return_SupplierId",
                table: "Return",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierReturn_SupplierId",
                table: "SupplierReturn",
                column: "SupplierId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Return");

            migrationBuilder.DropTable(
                name: "SupplierReturn");
        }
    }
}
