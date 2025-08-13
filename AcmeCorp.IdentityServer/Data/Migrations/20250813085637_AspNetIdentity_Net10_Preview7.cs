using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcmeCorp.IdentityServer.Data.Migrations
{
    /// <inheritdoc />
    public partial class AspNetIdentity_Net10_Preview7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttestationObject",
                table: "AspNetUserPasskeys");

            migrationBuilder.DropColumn(
                name: "ClientDataJson",
                table: "AspNetUserPasskeys");

            migrationBuilder.DropColumn(
                name: "IsBackedUp",
                table: "AspNetUserPasskeys");

            migrationBuilder.DropColumn(
                name: "IsBackupEligible",
                table: "AspNetUserPasskeys");

            migrationBuilder.DropColumn(
                name: "IsUserVerified",
                table: "AspNetUserPasskeys");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "AspNetUserPasskeys");

            migrationBuilder.DropColumn(
                name: "PublicKey",
                table: "AspNetUserPasskeys");

            migrationBuilder.DropColumn(
                name: "SignCount",
                table: "AspNetUserPasskeys");

            migrationBuilder.DropColumn(
                name: "Transports",
                table: "AspNetUserPasskeys");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "AspNetUserPasskeys",
                newName: "Data");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Data",
                table: "AspNetUserPasskeys",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<byte[]>(
                name: "AttestationObject",
                table: "AspNetUserPasskeys",
                type: "BLOB",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "ClientDataJson",
                table: "AspNetUserPasskeys",
                type: "BLOB",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<bool>(
                name: "IsBackedUp",
                table: "AspNetUserPasskeys",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsBackupEligible",
                table: "AspNetUserPasskeys",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsUserVerified",
                table: "AspNetUserPasskeys",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "AspNetUserPasskeys",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "PublicKey",
                table: "AspNetUserPasskeys",
                type: "BLOB",
                maxLength: 1024,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<uint>(
                name: "SignCount",
                table: "AspNetUserPasskeys",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<string>(
                name: "Transports",
                table: "AspNetUserPasskeys",
                type: "TEXT",
                nullable: true);
        }
    }
}
