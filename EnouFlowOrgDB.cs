using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Infrastructure;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace EnouFlowOrgMgmtLib
{
  //TODO: 为实体类实现验证接口 IValidatableObject, 范例如下:
  //public class Product : IValidatableObject
  //{
  //  public int ProductID { get; set; }
  //  public string Name { get; set; }
  //  public decimal Price { get; set; }
  //  public bool IncludeInSale { get; set; }
  //  public IEnumerable<ValidationResult> Validate(ValidationContext
  //  validationContext)
  //  {
  //    List<ValidationResult> errors = new List<ValidationResult>();
  //    if (Name == null || Name == string.Empty)
  //    {
  //      errors.Add(new ValidationResult(
  //      "A value is required for the Name property"));
  //    }
  //    if (Price == 0)
  //    {
  //      errors.Add(new ValidationResult(
  //      "A value is required for the Price property"));
  //    }
  //    else if (Price < 1 || Price > 2000)
  //    {
  //      errors.Add(new ValidationResult("The Price value is out of range"));
  //    }
  //    if (IncludeInSale)
  //    {
  //      errors.Add(new ValidationResult(
  //      "Request cannot contain values for IncludeInSale"));
  //    }
  //    return errors;
  //  }
  //}


  public class EnouFlowOrgMgmtContext : DbContext
  {
    #region Some tedious configuration
    public EnouFlowOrgMgmtContext()
      : base("name=EnouFlowPlatformDatabase") // DB connection name
    {
    }
    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
      //去掉系统自带的级联删除
      modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
      modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
    }
    #endregion

    public DbSet<Org> orgs { get; set; }
    public DbSet<OrgSchema> orgSchemas { get; set; }
    public DbSet<BizEntity> bizEntities { get; set; }
    public DbSet<BizEntitySchema> bizEntitySchemas { get; set; }
    public DbSet<BizEntityRelationOnOrgSchema> bizEntityRelationOnOrgSchemas { get; set; }
    public DbSet<Department> departments { get; set; }
    public DbSet<DepartmentParentChildRelation> departmentParentChildRelations { get; set; }

    public DbSet<User> users { get; set; }
    public DbSet<DepartmentUserRelation> departmentUserRelations { get; set; }
    public DbSet<Role> roles { get; set; }
    public DbSet<RoleUserRelation> roleUserRelations { get; set; }
    public DbSet<RoleType> roleTypes { get; set; }
    public DbSet<Role_RoleTypeRelation> role_RoleTypeRelations { get; set; }

    public DbSet<SystemManager> systemManagers { get; set; }
  }

  [Table("Enou_Org")]
  public class Org // The root, Organization = Client in SAP
  {
    [Key]
    public int orgId { get; set; }
    [Required]
    public string guid { get; set; } = Guid.NewGuid().ToString();
    [Required]
    public string name { get; set; }
    public string shortName { get; set; }
    public string displayName { get; set; }
    public string code { get; set; }
    public string indexNumber { get; set; }
    public string url { get; set; }
    public string DunsNumber { get; set; } //邓白氏
    public DateTime createTime { get; set; } = DateTime.Now;
    public bool isDefault { get; set; } = true;
    public bool isVisible { get; set; } = true;
    //其他组织与本系统所服务的组织的亲缘性级别, 越小则越亲近
    public int affintyLevel { get; set; } = 0;

    public virtual List<OrgSchema> orgSchemas { get; set; }

    [NotMapped]
    public OrgSchema orgSchema
    {
      get
      {
        if(this.orgSchemas != null) { 
          return this.orgSchemas.Where(orgSchema=>orgSchema.isVisible).
            ToList().FirstOrDefault();
        }
        else
        {
          return null;
        }
      }
    }
  }

  [Table("Enou_OrgSchema")]
  public class OrgSchema // 组织结构方案, 一个组织下的实体集合可按行业,地理或其他归类方式做不同的层次分解

  {
    [Key]
    public int orgSchemaId { get; set; }
    [Required]
    public string guid { get; set; } = Guid.NewGuid().ToString();
    [Required]
    public string name { get; set; } = "Default OrgSchema";
    public string shortName { get; set; }
    public string displayName { get; set; }
    public string code { get; set; }
    public string indexNumber { get; set; }
    public bool isDefault { get; set; } = true;
    public bool isVisible { get; set; } = true;
    public DateTime createTime { get; set; } = DateTime.Now;

    public virtual List<BizEntityRelationOnOrgSchema> 
      bizEntityRelationOnOrgSchemas { get; set; }

    [NotMapped]
    public List<BizEntity> rootBizEntities //该组织结构方案下的顶级业务实体集
    { 
      get
      {
        if(this.bizEntityRelationOnOrgSchemas==null ||
          this.bizEntityRelationOnOrgSchemas.Count <= 0)
        {
          return new List<BizEntity>() ;
        }

        return this.bizEntityRelationOnOrgSchemas.ToList().Where(
          r =>r.bizEntityIdParent==null).ToList().Select(
          r => r.bizEntityChild).ToList();
      }
    }
  }
  
  [Table("Enou_BizEntity")]
  public class BizEntity // Can be company, business unit, sub-company, branch, plant etc.
  {
    [Key]
    public int bizEntityId { get; set; }
    public string guid { get; set; } = Guid.NewGuid().ToString();
    [Required]
    public string name { get; set; }
    public string shortName { get; set; }
    public string displayName { get; set; }
    public string englishName { get; set; }
    public string code { get; set; }
    public string indexNumber { get; set; }
    public string url { get; set; }
    public string DunsNumber { get; set; } //邓白氏
    public bool isVisible { get; set; } = true;
    public DateTime createTime { get; set; } = DateTime.Now;

    public List<BizEntity> getBizEntitiesChildren(EnouFlowOrgMgmtContext db, OrgSchema orgSchema)
    {
      return db.bizEntityRelationOnOrgSchemas.ToList().Where(
        r => r.bizEntityIdParent == this.bizEntityId && 
        r.assistOrgSchemaId == orgSchema.orgSchemaId).Select(
        r => r.bizEntityChild).ToList();
    }

    public BizEntity getBizEntitiyParent(EnouFlowOrgMgmtContext db, OrgSchema orgSchema)
    {
      return db.bizEntityRelationOnOrgSchemas.ToList().Where(
        r => r.bizEntityIdChild == this.bizEntityId &&
        r.assistOrgSchemaId == orgSchema.orgSchemaId).Select(
        r => r.bizEntityParent).ToList().FirstOrDefault();
    }

    [NotMapped]
    public List<BizEntity> bizEntitiesParentAll { get; }

    public virtual List<BizEntitySchema> bizEntitySchemas { get; set; }
  }

  [Table("Enou_BizEntitiesRelationOnOrgSchema")]
  public class BizEntityRelationOnOrgSchema
  {
    [Key]
    public int BizEntityRelationOnOrgSchemaId { get; set; }

    public int? bizEntityIdParent { get; set; }
    [ForeignKey("bizEntityIdParent")]
    public virtual BizEntity bizEntityParent { get; set; }

    public int bizEntityIdChild { get; set; }
    [ForeignKey("bizEntityIdChild")]
    public virtual BizEntity bizEntityChild { get; set; }

    [Required]
    public int assistOrgSchemaId { get; set; } //assist navigation through relation object

    public DateTime createTime { get; set; } = DateTime.Now;
    public bool isValid { get; set; } = true;
  }


  [Table("Enou_BizEntitySchema")]
  public class BizEntitySchema // 
  {
    [Key]
    public int bizEntitySchemaId { get; set; }
    public string guid { get; set; } = Guid.NewGuid().ToString();
    [Required]
    public string name { get; set; } = "Default BizEntitySchema";
    public string shortName { get; set; }
    public string displayName { get; set; }
    public string englishName { get; set; }
    public string code { get; set; }
    public string indexNumber { get; set; }
    public bool isDefault { get; set; } = true;
    public bool isVisible { get; set; } = true;
    public DateTime createTime { get; set; } = DateTime.Now;

    public virtual List<Department> departments { get; set; } //该实体结构方案下的所有部门(项目组)集

    public virtual List<DepartmentParentChildRelation> departmentParentChildRelations { get; set; } //该实体结构方案下的部门父子关系集

    //该实体结构方案下的顶级部门(项目组)集
    public List<Department> getRootDepartments(EnouFlowOrgMgmtContext db)
    {
      if (this.departmentParentChildRelations == null) return new List<Department>();
      return this.departmentParentChildRelations.ToList().
        Where(
          r => r.departmentParent == null).ToList().Select(
            r => r.departmentChild).ToList();
    }
  }

  [Table("Enou_Department")]
  public class Department
  {
    [Key]
    public int departmentId { get; set; }
    public string guid { get; set; } = Guid.NewGuid().ToString();
    public int assistBizEntitySchemaId { get; set; }
    [Required]
    public string name { get; set; }
    public string shortName { get; set; }
    public string displayName { get; set; }
    public string englishName { get; set; }
    public string code { get; set; }
    public string indexNumber { get; set; }
    public bool isVisible { get; set; } = true;
    public DateTime createTime { get; set; } = DateTime.Now;

    //该部门包含的所有用户列表
    public List<User> getUserChildren(EnouFlowOrgMgmtContext db)
    {
      if (this.departmentUserRelations == null) return new List<User>();

      return db.users.ToList().Where(u =>
       this.departmentUserRelations.ToList().Select(
         r => r.assistUserId).ToList().Contains(
           u.userId)).ToList();
    }

    //该部门包含的所有子部门列表
    public List<Department> getDepartmentChildren(EnouFlowOrgMgmtContext db)
    {
       return db.departmentParentChildRelations.ToList().Where(
        r => r.departmentIdParent == this.departmentId).ToList().Select(
        r => r.departmentChild).ToList();
    }

    //该部门的父部门
    public Department getParentDepartment(EnouFlowOrgMgmtContext db)
    {
      return db.departmentParentChildRelations.ToList().Where(
        r => r.departmentIdChild == this.departmentId).
        ToList().FirstOrDefault().departmentParent;
    }

    public virtual List<DepartmentUserRelation> departmentUserRelations { get; set; }

  }

  [Table("Enou_DepartmentParentChildRelation")]
  public class DepartmentParentChildRelation
  {
    [Key]
    public int departmentParentChildRelationId { get; set; }

    public int? departmentIdParent { get; set; }
    [ForeignKey("departmentIdParent")]
    public virtual Department departmentParent { get; set; }

    public int departmentIdChild { get; set; }
    [ForeignKey("departmentIdChild")]
    public virtual Department departmentChild { get; set; }

    [Required]
    public int assistBizEntitySchemaId { get; set; } //assist navigation through relation object

    public DateTime createTime { get; set; } = DateTime.Now;

    public bool isValid { get; set; } = true;
  }

  [Table("Enou_User")]
  public class User
  {
    [Key]
    public int userId { get; set; }
    public string guid { get; set; } = Guid.NewGuid().ToString();
    [Required]
    public string name { get; set; }
    public string displayName { get; set; }
    public string englishName { get; set; }
    public string code { get; set; }
    public string indexNumber { get; set; }
    [EmailAddress]
    public string email { get; set; }
    public string accountInNT { get; set; }
    public string logonName { get; set; } //用户的登录账户号
    public string logonSalt { get; set; } //用户的加密盐 http://www.cnblogs.com/xizz/p/5007531.html
    public string logonPasswordHash { get; set; } //用户的加密密码
    public string officeTel { get; set; }
    public string personalTel { get; set; }
    public string personalMobile { get; set; }
    public bool isVisible { get; set; } = true;
    public DateTime createTime { get; set; } = DateTime.Now;
    public DateTime? validTimeFrom { get; set; }
    public DateTime? validTimeTo { get; set; }

    public virtual List<DepartmentUserRelation> departmentUserRelations { get; set; }
    public virtual List<RoleUserRelation> roleUserRelations { get; set; }

    //该用户所属的所有(根据输入参数筛选有效性)部门列表
    public List<Department> getDepartmentsBelongTo(
      EnouFlowOrgMgmtContext db, bool onlyValid = true)
    {
      return db.departments.ToList().Where(d =>
        this.departmentUserRelations.Where(
          r=> onlyValid? r.isValid : true).ToList().Select(
          r => r.assistDepartmentId).ToList().Contains(
            d.departmentId)).ToList();
    }

    //该用户所属的所有(根据输入参数筛选有效性)角色列表
    public List<Role> getRolesBelongTo(EnouFlowOrgMgmtContext db
      , bool onlyValid = true)
    {
      return db.roles.ToList().Where(role =>
        this.roleUserRelations.Where(
          r=> onlyValid ? r.isValid : true).ToList().Select(
          r => r.assistRoleId).ToList().Contains(
          role.roleId)).ToList();
    }

    //TODO 其他的补充属性可后面再加或者考虑用complex type来完成,尤其是可自定义的custom attributes
  }

  [Table("Enou_DepartmentUserRelation")]
  public class DepartmentUserRelation
  {
    [Key]
    public int departmentUserRelationId { get; set; }

    //assist navigation through relation object
    [Required]
    public int assistDepartmentId { get; set; }

    //assist navigation through relation object
    [Required]
    public int assistUserId { get; set; }

    [Required]
    public UserPositionToDepartment userPosition { get; set; } = UserPositionToDepartment.normal;

    public DateTime createTime { get; set; } = DateTime.Now;
    public bool isValid { get; set; } = true;
  }

  [Table("Enou_Role")]
  public class Role
  {
    [Key]
    public int roleId { get; set; }
    public string guid { get; set; } = Guid.NewGuid().ToString();
    [Required]
    public string name { get; set; }
    public string displayName { get; set; }
    public string englishName { get; set; }
    public string code { get; set; }
    public string indexNumber { get; set; }
    public bool isVisible { get; set; } = true;
    public DateTime createTime { get; set; } = DateTime.Now;

    public virtual List<RoleUserRelation> roleUserRelations { get; set; }

    public virtual List<Role_RoleTypeRelation> role_RoleTypeRelations { get; set; }

    //该角色所拥有的所有用户列表
    public List<User> getUsersBelongTo(
      EnouFlowOrgMgmtContext db, bool onlyValid = true)
    {
      if (this.roleUserRelations == null) return new List<User>();
      return db.users.ToList().Where(
        u => this.roleUserRelations.Where(
          r => onlyValid ? r.isValid : true).ToList().Select(
            r => r.assistUserId).ToList().Contains(u.userId)).ToList();
    }

    //该角色所属的所有(根据输入参数筛选有效性)角色类型列表
    public List<RoleType> getRoleTypesBelongTo(
      EnouFlowOrgMgmtContext db, bool onlyValid = true)
    {
      return db.roleTypes.ToList().Where(x =>
        this.role_RoleTypeRelations.Where(
          r => onlyValid ? r.isValid : true).ToList().Select(
          r => r.assistRoleTypeId).ToList().Contains(
            x.roleTypeId)).ToList();
    }
  }

  [Table("Enou_RoleUserRelation")]
  public class RoleUserRelation
  {
    [Key]
    public int roleUserRelationId { get; set; }

    [Required]
    public int assistRoleId { get; set; } //assist navigation through relation object
    [Required]
    public int assistUserId { get; set; } //assist navigation through relation object

    public DateTime createTime { get; set; } = DateTime.Now;
    public bool isValid { get; set; } = true;
  }

  [Table("Enou_RoleType")]
  public class RoleType //为未来需求扩展保留
  {
    [Key]
    public int roleTypeId { get; set; }
    public string guid { get; set; } = Guid.NewGuid().ToString();
    [Required]
    public string name { get; set; }
    public bool isVisible { get; set; } = true;
    public DateTime createTime { get; set; } = DateTime.Now;

    public virtual List<Role_RoleTypeRelation> role_RoleTypeRelations { get; set; }

    //该角色类型所包含的所有(根据输入参数筛选有效性)角色列表
    public List<Role> getRolesBelongTo(
      EnouFlowOrgMgmtContext db, bool onlyValid = true)
    {
      if (this.role_RoleTypeRelations == null) return new List<Role>();

      return db.roles.ToList().Where(x =>
        this.role_RoleTypeRelations.Where(
          r => onlyValid ? r.isValid : true).ToList().Select(
          r => r.assistRoleId).ToList().Contains(
            x.roleId)).ToList();
    }
  }

  [Table("Enou_Role_RoleTypeRelation")]
  public class Role_RoleTypeRelation //为未来需求扩展保留
  {
    [Key]
    public int role_RoleTypeRelationId { get; set; }

    [Required]
    public int assistRoleTypeId { get; set; } //assist navigation through relation object

    [Required]
    public int assistRoleId { get; set; } //assist navigation through relation object

    public DateTime createTime { get; set; } = DateTime.Now;
    public bool isValid { get; set; } = true;
  }

  [Table("Enou_SystemManager")]
  public class SystemManager
  {
    [Key]
    public int systemManagerId { get; set; }
    public string guid { get; set; } = Guid.NewGuid().ToString();
    [Required]
    public string name { get; set; }
    [Required]
    public string logonName { get; set; }
    [Required]
    public string logonSalt { get; set; }
    [Required]
    public string logonPasswordHash { get; set; }
    public DateTime createTime { get; set; } = DateTime.Now;
    public DateTime? lastLogonTime { get; set; }
  }
}
