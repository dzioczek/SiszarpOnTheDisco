using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SiszarpOnTheDisco.Migrations
{
    public partial class NewTableForAllergenIcons : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AllergenIcons",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    IconName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllergenIcons", x => x.ID);
                });

            migrationBuilder.InsertData(
            table: "AllergenIcons",
            columns: new[] { "Name", "IconName" },
            values: new object[,]
            {
                {"Cladosporium", ":mushroom:" },
                {"Alternaria", ":mushroom:" },
                {"Jesion", ":deciduous_tree:"},
                {"Brzoza", ":deciduous_tree:" },
                {"Grab", ":deciduous_tree:" },
                {"Klon",":deciduous_tree:" },
                {"Dąb", ":deciduous_tree:" },
                {"Platan", ":deciduous_tree:" },
                {"Wierzba", ":deciduous_tree:" },
                {"Trawy", ":seedling:" },
                {"Szczaw", ":seedling:" },
                {"Buk", ":deciduous_tree:" },
                {"Sosna", ":evergreen_tree:" },
                {"Babka", ":seedling:" },
            }
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AllergenIcons");
        }
    }
}
