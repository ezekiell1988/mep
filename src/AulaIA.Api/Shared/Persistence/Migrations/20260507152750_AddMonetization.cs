using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AulaIA.Api.Shared.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMonetization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "referred_by_code",
                table: "users",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "exchange_rates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    usd_to_crc = table.Column<decimal>(type: "numeric(10,4)", nullable: false),
                    source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exchange_rates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "referral_codes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_referral_codes", x => x.id);
                    table.ForeignKey(
                        name: "FK_referral_codes_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "subscriptions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_trial = table.Column<bool>(type: "boolean", nullable: false),
                    current_period_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    current_period_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscriptions", x => x.id);
                    table.ForeignKey(
                        name: "FK_subscriptions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "commissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    referral_code_id = table.Column<Guid>(type: "uuid", nullable: false),
                    referred_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    month = table.Column<int>(type: "integer", nullable: false),
                    gross_revenue_crc = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    infra_cost_crc = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    base_amount_crc = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    commission_rate = table.Column<decimal>(type: "numeric(5,4)", nullable: false),
                    commission_amount_crc = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_commissions", x => x.id);
                    table.ForeignKey(
                        name: "FK_commissions_referral_codes_referral_code_id",
                        column: x => x.referral_code_id,
                        principalTable: "referral_codes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_commissions_users_referred_user_id",
                        column: x => x.referred_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payment_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    amount_usd = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    amount_crc = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    exchange_rate_used = table.Column<decimal>(type: "numeric(10,4)", nullable: false),
                    reference_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    voucher_blob_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    admin_note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    reviewed_by_auth0_sub = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_requests", x => x.id);
                    table.ForeignKey(
                        name: "FK_payment_requests_subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "subscriptions",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_payment_requests_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("bb000001-0000-0000-0000-000000000001"),
                column: "referred_by_code",
                value: null);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("bb000001-0000-0000-0000-000000000002"),
                column: "referred_by_code",
                value: null);

            migrationBuilder.CreateIndex(
                name: "ix_commissions_referral_user_month",
                table: "commissions",
                columns: new[] { "referral_code_id", "referred_user_id", "month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_commissions_referred_user_id",
                table: "commissions",
                column: "referred_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_exchange_rates_date",
                table: "exchange_rates",
                column: "date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_payment_requests_reference_code",
                table: "payment_requests",
                column: "reference_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payment_requests_SubscriptionId",
                table: "payment_requests",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "ix_payment_requests_user_id",
                table: "payment_requests",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_referral_codes_code",
                table: "referral_codes",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_referral_codes_user_id",
                table: "referral_codes",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_subscriptions_user_id",
                table: "subscriptions",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "commissions");

            migrationBuilder.DropTable(
                name: "exchange_rates");

            migrationBuilder.DropTable(
                name: "payment_requests");

            migrationBuilder.DropTable(
                name: "referral_codes");

            migrationBuilder.DropTable(
                name: "subscriptions");

            migrationBuilder.DropColumn(
                name: "referred_by_code",
                table: "users");
        }
    }
}
