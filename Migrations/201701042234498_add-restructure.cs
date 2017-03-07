namespace EnouFlowOrgMgmtLib.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addrestructure : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Enou_BizEntitiesRelationOnOrgSchema",
                c => new
                    {
                        BizEntityRelationOnOrgSchemaId = c.Int(nullable: false, identity: true),
                        bizEntityIdParent = c.Int(nullable: false),
                        bizEntityIdChild = c.Int(nullable: false),
                        OrgSchema_orgSchemaId = c.Int(),
                    })
                .PrimaryKey(t => t.BizEntityRelationOnOrgSchemaId)
                .ForeignKey("dbo.Enou_BizEntity", t => t.bizEntityIdChild, cascadeDelete: false)
                .ForeignKey("dbo.Enou_BizEntity", t => t.bizEntityIdParent, cascadeDelete: false)
                .ForeignKey("dbo.Enou_OrgSchema", t => t.OrgSchema_orgSchemaId)
                .Index(t => t.bizEntityIdParent)
                .Index(t => t.bizEntityIdChild)
                .Index(t => t.OrgSchema_orgSchemaId);
            
            CreateTable(
                "dbo.Enou_BizEntity",
                c => new
                    {
                        bizEntityId = c.Int(nullable: false, identity: true),
                        guid = c.String(),
                        name = c.String(nullable: false),
                        shortName = c.String(),
                        displayName = c.String(),
                        englishName = c.String(),
                        code = c.String(),
                        indexNumber = c.String(),
                        url = c.String(),
                        DunsNumber = c.String(),
                        isVisible = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.bizEntityId);
            
            CreateTable(
                "dbo.Enou_BizEntitySchema",
                c => new
                    {
                        bizEntitySchemaId = c.Int(nullable: false, identity: true),
                        guid = c.String(),
                        name = c.String(nullable: false),
                        shortName = c.String(),
                        displayName = c.String(),
                        englishName = c.String(),
                        code = c.String(),
                        indexNumber = c.String(),
                        isDefault = c.Boolean(nullable: false),
                        isVisible = c.Boolean(nullable: false),
                        BizEntity_bizEntityId = c.Int(),
                    })
                .PrimaryKey(t => t.bizEntitySchemaId)
                .ForeignKey("dbo.Enou_BizEntity", t => t.BizEntity_bizEntityId)
                .Index(t => t.BizEntity_bizEntityId);
            
            CreateTable(
                "dbo.Enou_Department",
                c => new
                    {
                        departmentId = c.Int(nullable: false, identity: true),
                        guid = c.String(),
                        name = c.String(nullable: false),
                        shortName = c.String(),
                        displayName = c.String(),
                        englishName = c.String(),
                        code = c.String(),
                        indexNumber = c.String(),
                        isVisible = c.Boolean(nullable: false),
                        BizEntitySchema_bizEntitySchemaId = c.Int(),
                    })
                .PrimaryKey(t => t.departmentId)
                .ForeignKey("dbo.Enou_BizEntitySchema", t => t.BizEntitySchema_bizEntitySchemaId)
                .Index(t => t.BizEntitySchema_bizEntitySchemaId);
            
            CreateTable(
                "dbo.Enou_DepartmentUserRelation",
                c => new
                    {
                        departmentUserRelationId = c.Int(nullable: false, identity: true),
                        createTime = c.DateTime(nullable: false),
                        Department_departmentId = c.Int(),
                    })
                .PrimaryKey(t => t.departmentUserRelationId)
                .ForeignKey("dbo.Enou_Department", t => t.Department_departmentId)
                .Index(t => t.Department_departmentId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Enou_BizEntitiesRelationOnOrgSchema", "OrgSchema_orgSchemaId", "dbo.Enou_OrgSchema");
            DropForeignKey("dbo.Enou_BizEntitiesRelationOnOrgSchema", "bizEntityIdParent", "dbo.Enou_BizEntity");
            DropForeignKey("dbo.Enou_BizEntitiesRelationOnOrgSchema", "bizEntityIdChild", "dbo.Enou_BizEntity");
            DropForeignKey("dbo.Enou_BizEntitySchema", "BizEntity_bizEntityId", "dbo.Enou_BizEntity");
            DropForeignKey("dbo.Enou_Department", "BizEntitySchema_bizEntitySchemaId", "dbo.Enou_BizEntitySchema");
            DropForeignKey("dbo.Enou_DepartmentUserRelation", "Department_departmentId", "dbo.Enou_Department");
            DropIndex("dbo.Enou_DepartmentUserRelation", new[] { "Department_departmentId" });
            DropIndex("dbo.Enou_Department", new[] { "BizEntitySchema_bizEntitySchemaId" });
            DropIndex("dbo.Enou_BizEntitySchema", new[] { "BizEntity_bizEntityId" });
            DropIndex("dbo.Enou_BizEntitiesRelationOnOrgSchema", new[] { "OrgSchema_orgSchemaId" });
            DropIndex("dbo.Enou_BizEntitiesRelationOnOrgSchema", new[] { "bizEntityIdChild" });
            DropIndex("dbo.Enou_BizEntitiesRelationOnOrgSchema", new[] { "bizEntityIdParent" });
            DropTable("dbo.Enou_DepartmentUserRelation");
            DropTable("dbo.Enou_Department");
            DropTable("dbo.Enou_BizEntitySchema");
            DropTable("dbo.Enou_BizEntity");
            DropTable("dbo.Enou_BizEntitiesRelationOnOrgSchema");
        }
    }
}
