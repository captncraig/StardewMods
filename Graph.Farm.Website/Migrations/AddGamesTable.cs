using FluentMigrator;

namespace Graph.Farm.Website.Migrations
{
    [Migration(1)]
    public class AddGamesTable : Migration
    {
        public override void Up()
        {
            Create.Table("Games")
                .WithColumn("Id").AsInt64().PrimaryKey().Identity()
                .WithColumn("AccountId").AsInt64()
                .WithColumn("Seed").AsInt64()
                .WithColumn("Name").AsString()
                .WithColumn("FarmName").AsString()
                .WithColumn("FavoriteThing").AsString();

            Create.ForeignKey()
                .FromTable("Games").ForeignColumn("AccountId")
                .ToTable("Accounts").PrimaryColumn("Id");
        }

        public override void Down()
        {
            Delete.Table("Games");
        }
    }
}
