namespace EnouFlowOrgMgmtLib.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addrestructure2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Enou_DepartmentParentChildRelation",
                c => new
                    {
                        departmentParentChildRelationId = c.Int(nullable: false, identity: true),
                        departmentIdParent = c.Int(nullable: false),
                        departmentIdChild = c.Int(nullable: false),
                        BizEntitySchema_bizEntitySchemaId = c.Int(),
                    })
                .PrimaryKey(t => t.departmentParentChildRelationId)
                .ForeignKey("dbo.Enou_Department", t => t.departmentIdChild, cascadeDelete: false)
                .ForeignKey("dbo.Enou_Department", t => t.departmentIdParent, cascadeDelete: false)
                .ForeignKey("dbo.Enou_BizEntitySchema", t => t.BizEntitySchema_bizEntitySchemaId)
                .Index(t => t.departmentIdParent)
                .Index(t => t.departmentIdChild)
                .Index(t => t.BizEntitySchema_bizEntitySchemaId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Enou_DepartmentParentChildRelation", "BizEntitySchema_bizEntitySchemaId", "dbo.Enou_BizEntitySchema");
            DropForeignKey("dbo.Enou_DepartmentParentChildRelation", "departmentIdParent", "dbo.Enou_Department");
            DropForeignKey("dbo.Enou_DepartmentParentChildRelation", "departmentIdChild", "dbo.Enou_Department");
            DropIndex("dbo.Enou_DepartmentParentChildRelation", new[] { "BizEntitySchema_bizEntitySchemaId" });
            DropIndex("dbo.Enou_DepartmentParentChildRelation", new[] { "departmentIdChild" });
            DropIndex("dbo.Enou_DepartmentParentChildRelation", new[] { "departmentIdParent" });
            DropTable("dbo.Enou_DepartmentParentChildRelation");
        }
    }
}
