using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Groove.SP.Persistence.Migrations
{
    public partial class Add_New_Permission_RoutingOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "CreatedBy", "CreatedDate", "Description", "Name", "Order", "UpdatedBy", "UpdatedDate" },
                values: new object[,]
                {
                    { 139L, "System", new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Order.RoutingOrderList", 1216, null, null },
                    { 140L, "System", new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Order.RoutingOrderDetail", 1217, null, null },
                    { 141L, "System", new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Order.RoutingOrderDetail.Edit", 1218, null, null },
                    { 142L, "System", new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Order.RoutingOrderDetail.Confirm", 1219, null, null }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Key", "CreatedBy", "CreatedDate", "English", "Note", "SimplifiedChinese", "TraditionalChinese", "UpdatedBy", "UpdatedDate" },
                values: new object[,]
                {
                    { "label.routingOrders", "System", new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Routing Orders", null, "Routing Orders (SC)", "Routing Orders (TC)", null, new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { "permission.order.routingOrderDetail", "System", new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Routing Order Detail", null, "Routing Order Detail (SC)", "Routing Order Detail (TC)", null, new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { "permission.order.routingOrderDetail.confirm", "System", new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Confirm", null, "Confirm (SC)", "Confirm (TC)", null, new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { "permission.order.routingOrderDetail.edit", "System", new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Edit", null, "Edit (SC)", "Edit (TC)", null, new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { "permission.order.routingOrderList", "System", new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "List of Routing Orders", null, "List of Routing Orders (SC)", "List of Routing Orders (TC)", null, new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId", "CreatedBy", "CreatedDate", "UpdatedBy", "UpdatedDate" },
                values: new object[,]
                {
                    { 139L, 1L, "System", new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null },
                    { 140L, 1L, "System", new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null },
                    { 141L, 1L, "System", new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null },
                    { 142L, 1L, "System", new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 139L, 1L });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 140L, 1L });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 141L, 1L });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 142L, 1L });

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "label.routingOrders");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "permission.order.routingOrderDetail");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "permission.order.routingOrderDetail.confirm");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "permission.order.routingOrderDetail.edit");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "permission.order.routingOrderList");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 139L);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 140L);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 141L);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 142L);
        }
    }
}
