using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBookDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Books",
                type: "TEXT",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: "A hands-on guide to the craft of software development. Through dozens of practical tips, it covers writing flexible and maintainable code, avoiding duplication with the DRY principle, using tracer bullets and prototypes, automating with the right tools, and taking pragmatic responsibility for your work. A career-spanning classic for developers who want to think about programming as a craft.");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 2,
                column: "Description",
                value: "A manual of software craftsmanship focused on writing code that humans can read. It argues that clean code is a matter of discipline: meaningful names, small functions that do one thing, clear comments only when needed, and rigorous handling of errors and boundaries. Includes detailed case studies that refactor messy code into clean code step by step.");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "The foundational text on tackling complexity in software by modeling the business domain. It introduces a shared ubiquitous language between developers and domain experts, and building blocks such as entities, value objects, aggregates, repositories, and bounded contexts. A deep, strategic look at designing software that reflects how a business actually works.");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 4,
                column: "Description",
                value: "Set on the desert planet Arrakis, the only source of the spice melange, this epic follows young Paul Atreides as his family is betrayed and he is drawn into a struggle for control of the planet and its people. A sweeping story of politics, religion, ecology, and prophecy that became one of the most influential science fiction novels ever written.");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 5,
                column: "Description",
                value: "The first day in the recounted life of Kvothe: a gifted young man who grows up among traveling performers, survives tragedy and poverty, and talks his way into a legendary university of magic. Framed as a memoir told over three days, it is a lyrical coming-of-age fantasy about music, magic, love, and the gap between a legend and the truth behind it.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Books");
        }
    }
}
