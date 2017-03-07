namespace EnouFlowOrgMgmtLib.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addBizEntityRelationOnOrgSchema : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Enou_BizEntitiesRelationOnOrgSchema",
                c => new
                    {
                        orgSchemaId = c.Int(nullable: false),
                        bizEntityIdParent = c.Int(nullable: false),
                        bizEntityIdChild = c.Int(nullable: false),
                        BizEntity_bizEntityId = c.Int(),
                        BizEntity_bizEntityId1 = c.Int(),
                    })
                .PrimaryKey(t => new { t.orgSchemaId, t.bizEntityIdParent, t.bizEntityIdChild })
                .ForeignKey("dbo.Enou_BizEntity", t => t.bizEntityIdChild, cascadeDelete: false)
                .ForeignKey("dbo.Enou_BizEntity", t => t.bizEntityIdParent, cascadeDelete: false)
                .ForeignKey("dbo.Enou_BizEntity", t => t.BizEntity_bizEntityId)
                .ForeignKey("dbo.Enou_BizEntity", t => t.BizEntity_bizEntityId1)
                .Index(t => t.bizEntityIdParent)
                .Index(t => t.bizEntityIdChild)
                .Index(t => t.BizEntity_bizEntityId)
                .Index(t => t.BizEntity_bizEntityId1);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Enou_BizEntitiesRelationOnOrgSchema", "BizEntity_bizEntityId1", "dbo.Enou_BizEntity");
            DropForeignKey("dbo.Enou_BizEntitiesRelationOnOrgSchema", "BizEntity_bizEntityId", "dbo.Enou_BizEntity");
            DropForeignKey("dbo.Enou_BizEntitiesRelationOnOrgSchema", "bizEntityIdParent", "dbo.Enou_BizEntity");
            DropForeignKey("dbo.Enou_BizEntitiesRelationOnOrgSchema", "bizEntityIdChild", "dbo.Enou_BizEntity");
            DropIndex("dbo.Enou_BizEntitiesRelationOnOrgSchema", new[] { "BizEntity_bizEntityId1" });
            DropIndex("dbo.Enou_BizEntitiesRelationOnOrgSchema", new[] { "BizEntity_bizEntityId" });
            DropIndex("dbo.Enou_BizEntitiesRelationOnOrgSchema", new[] { "bizEntityIdChild" });
            DropIndex("dbo.Enou_BizEntitiesRelationOnOrgSchema", new[] { "bizEntityIdParent" });
            DropTable("dbo.Enou_BizEntitiesRelationOnOrgSchema");
        }
    }
}
