using System;
using Bogus;
using Microsoft.EntityFrameworkCore.Migrations;
using RazorWeb.Models;

#nullable disable

namespace RazorWeb.Migrations
{
    /// <inheritdoc />
    public partial class InitDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Articles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Content = table.Column<string>(type: "ntext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articles", x => x.Id);
                });

            // Insert data
            // Fake data: Bogus

            Randomizer.Seed = new Random(8675309);

            var fakerArticle = new Faker<Article>()
                .RuleFor(a => a.Title, f => f.Lorem.Sentence(5, 5))
                .RuleFor(a => a.Created, f => f.Date.Between(new DateTime(2024, 1, 1), new DateTime(2024, 4, 1)))
                .RuleFor(a => a.Content, f => f.Lorem.Paragraphs(1, 4));

            for (int i = 0; i < 150; i++)
            {
                Article article = fakerArticle.Generate();

                migrationBuilder.InsertData(
                    table: "Articles",
                    columns: new[] { "Title", "Created", "Content" },
                    values: new object[] {
                    article.Title,
                    article.Created,
                    article.Content
                    });
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Articles");
        }
    }
}
