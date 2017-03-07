namespace EnouFlowOrgMgmtLib.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addBizEntitySchemaDepartmentUser : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Enou_BizEntitySchema",
                c => new
                    {
                        bizEntitySchemaId = c.Int(nullable: false, identity: true),
                        guid = c.String(),
                        name = c.String(),
                        shortName = c.String(),
                        displayName = c.String(),
                        englishName = c.String(),
                        code = c.String(),
                        indexNumber = c.String(),
                        isDefault = c.Boolean(nullable: false),
                        bizEntityId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.bizEntitySchemaId)
                .ForeignKey("dbo.Enou_BizEntity", t => t.bizEntityId, cascadeDelete: true)
                .Index(t => t.bizEntityId);
            
            CreateTable(
                "dbo.Enou_Department",
                c => new
                    {
                        departmentId = c.Int(nullable: false, identity: true),
                        guid = c.String(),
                        name = c.String(),
                        shortName = c.String(),
                        displayName = c.String(),
                        englishName = c.String(),
                        code = c.String(),
                        indexNumber = c.String(),
                        bizEntitySchemaId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.departmentId)
                .ForeignKey("dbo.Enou_BizEntity", t => t.bizEntitySchemaId, cascadeDelete: true)
                .ForeignKey("dbo.Enou_BizEntitySchema", t => t.bizEntitySchemaId, cascadeDelete: false)
                .Index(t => t.bizEntitySchemaId);
            
            CreateTable(
                "dbo.Enou_DepartmentParentChildRelation",
                c => new
                    {
                        departmentIdParent = c.Int(nullable: false),
                        departmentIdChild = c.Int(nullable: false),
                        Department_departmentId = c.Int(),
                    })
                .PrimaryKey(t => new { t.departmentIdParent, t.departmentIdChild })
                .ForeignKey("dbo.Enou_Department", t => t.departmentIdChild, cascadeDelete: false)
                .ForeignKey("dbo.Enou_Department", t => t.departmentIdParent, cascadeDelete: false)
                .ForeignKey("dbo.Enou_Department", t => t.Department_departmentId)
                .Index(t => t.departmentIdParent)
                .Index(t => t.departmentIdChild)
                .Index(t => t.Department_departmentId);
            
            CreateTable(
                "dbo.Enou_DepartmentUserRelation",
                c => new
                    {
                        departmentId = c.Int(nullable: false),
                        userId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.departmentId, t.userId })
                .ForeignKey("dbo.Enou_Department", t => t.departmentId, cascadeDelete: true)
                .ForeignKey("dbo.Enou_User", t => t.userId, cascadeDelete: true)
                .Index(t => t.departmentId)
                .Index(t => t.userId);
            
            CreateTable(
                "dbo.Enou_User",
                c => new
                    {
                        userId = c.Int(nullable: false, identity: true),
                        guid = c.String(),
                        name = c.String(),
                        displayName = c.String(),
                        englishName = c.String(),
                        code = c.String(),
                        indexNumber = c.String(),
                        email = c.String(),
                        accountInNT = c.String(),
                        officeTel = c.String(),
                        personalTel = c.String(),
                        personalMobile = c.String(),
                    })
                .PrimaryKey(t => t.userId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Enou_Department", "bizEntitySchemaId", "dbo.Enou_BizEntitySchema");
            DropForeignKey("dbo.Enou_DepartmentUserRelation", "userId", "dbo.Enou_User");
            DropForeignKey("dbo.Enou_DepartmentUserRelation", "departmentId", "dbo.Enou_Department");
            DropForeignKey("dbo.Enou_DepartmentParentChildRelation", "Department_departmentId", "dbo.Enou_Department");
            DropForeignKey("dbo.Enou_DepartmentParentChildRelation", "departmentIdParent", "dbo.Enou_Department");
            DropForeignKey("dbo.Enou_DepartmentParentChildRelation", "departmentIdChild", "dbo.Enou_Department");
            DropForeignKey("dbo.Enou_Department", "bizEntitySchemaId", "dbo.Enou_BizEntity");
            DropForeignKey("dbo.Enou_BizEntitySchema", "bizEntityId", "dbo.Enou_BizEntity");
            DropIndex("dbo.Enou_DepartmentUserRelation", new[] { "userId" });
            DropIndex("dbo.Enou_DepartmentUserRelation", new[] { "departmentId" });
            DropIndex("dbo.Enou_DepartmentParentChildRelation", new[] { "Department_departmentId" });
            DropIndex("dbo.Enou_DepartmentParentChildRelation", new[] { "departmentIdChild" });
            DropIndex("dbo.Enou_DepartmentParentChildRelation", new[] { "departmentIdParent" });
            DropIndex("dbo.Enou_Department", new[] { "bizEntitySchemaId" });
            DropIndex("dbo.Enou_BizEntitySchema", new[] { "bizEntityId" });
            DropTable("dbo.Enou_User");
            DropTable("dbo.Enou_DepartmentUserRelation");
            DropTable("dbo.Enou_DepartmentParentChildRelation");
            DropTable("dbo.Enou_Department");
            DropTable("dbo.Enou_BizEntitySchema");
        }
    }
}
