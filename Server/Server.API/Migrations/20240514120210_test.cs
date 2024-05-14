using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.API.Migrations
{
    /// <inheritdoc />
    public partial class test : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Friendships_AspNetUsers_User2Id",
                table: "Friendships");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1c7e97d3-a982-4a1b-8d8e-b6b9d7e32c0f",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "c430b9d2-7a91-4887-8a3b-e1448e6bcebd", "AQAAAAIAAYagAAAAEFT73J2kr5n6yrCgXEP0G/gjh5jtNm59uEBfKPwkyZD7+y0U3gRSaM94Tnr/st41qQ==", "b8655213-23f1-4d11-adf4-a54d8defe422" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3de1a4b2-2b03-4b9d-b04d-d02cbef1f447",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "7edb2ebb-443d-4fbb-9de7-59cc8f3089ea", "AQAAAAIAAYagAAAAEKmZIcAphhTsDivIMCq/FOpXFZww9Ck/gX50oUsBAMchyVCMz8J18ZlmX/J1x0rAHg==", "c7b25cc2-954b-45de-9422-5a8cc51438db" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59fbd0c8-0e0b-4cba-980e-f196b905a249",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "85fd46c5-5ade-4803-9850-e5801c871ae2", "AQAAAAIAAYagAAAAEA5Bb6crWafDFmOrCSQ9PntnjwgiOwMi7aFH5g/wudDqgiad3KuZZWORu2xkQvvBSQ==", "ce9e7136-1596-4fe3-8ac0-5cdfc265448d" });

            migrationBuilder.UpdateData(
                table: "Friendships",
                keyColumns: new[] { "User1Id", "User2Id" },
                keyValues: new object[] { "59fbd0c8-0e0b-4cba-980e-f196b905a249", "3de1a4b2-2b03-4b9d-b04d-d02cbef1f447" },
                column: "date",
                value: new DateTime(2024, 5, 14, 12, 2, 9, 922, DateTimeKind.Utc).AddTicks(609));

            migrationBuilder.AddForeignKey(
                name: "FK_Friendships_AspNetUsers_User2Id",
                table: "Friendships",
                column: "User2Id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Friendships_AspNetUsers_User2Id",
                table: "Friendships");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1c7e97d3-a982-4a1b-8d8e-b6b9d7e32c0f",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "751daeb4-9aea-469f-9773-51b6b8fa8b48", "AQAAAAIAAYagAAAAEHllCw9HY8axsfKyjdWs0r1s3V+xgD/k3ApSqSFeFwCM0O640+hjRaJNlMkLK+Zcrw==", "0c51f13e-ca3d-4a95-8ab9-3ba7b1ab8338" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3de1a4b2-2b03-4b9d-b04d-d02cbef1f447",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "8b2240d6-f153-4a0a-bbd5-c5a6d04ce544", "AQAAAAIAAYagAAAAEPaeehFkm0hZUFNwQ6elb0fEfIh9HQHMnmP+7FwPzrCJBCkXuhDzbqTgAt4cQGyeiQ==", "54e412be-48d7-470c-8191-60beaab39064" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59fbd0c8-0e0b-4cba-980e-f196b905a249",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "08ac1e46-a7db-40c2-9bc7-dedc1fd14a06", "AQAAAAIAAYagAAAAEBByMrXjw9e+ZnlFjTvYClJGsv6iQ9PMRouyTAFNpp3ySjq1GfEInrBfY2ywsJalCg==", "cd3557ae-e910-4839-a53d-f37f23236538" });

            migrationBuilder.UpdateData(
                table: "Friendships",
                keyColumns: new[] { "User1Id", "User2Id" },
                keyValues: new object[] { "59fbd0c8-0e0b-4cba-980e-f196b905a249", "3de1a4b2-2b03-4b9d-b04d-d02cbef1f447" },
                column: "date",
                value: new DateTime(2024, 4, 26, 10, 16, 58, 566, DateTimeKind.Utc).AddTicks(263));

            migrationBuilder.AddForeignKey(
                name: "FK_Friendships_AspNetUsers_User2Id",
                table: "Friendships",
                column: "User2Id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
