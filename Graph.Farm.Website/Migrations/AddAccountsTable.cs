using FluentMigrator;

namespace Graph.Farm.Website.Migrations
{
    [Migration(0)]
    public class AddAccountsTable : Migration
    {
        public override void Up() 
        {
            Create.Table("Accounts")
                .WithColumn("Id").AsInt64().PrimaryKey().Identity()
                .WithColumn("ApiKeyHash").AsString();
        }

        public override void Down()
        {
            Delete.Table("Accounts");
        }
    }
}
