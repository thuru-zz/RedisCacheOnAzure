namespace AzureRedisCacheSample.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PostsUsernameAdded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Posts", "Username", c => c.String());
            DropColumn("dbo.Posts", "UserId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Posts", "UserId", c => c.Guid(nullable: false));
            DropColumn("dbo.Posts", "Username");
        }
    }
}
