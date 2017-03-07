using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnouFlowOrgMgmtLib
{
  public class OrgDTO
  {
    public int orgId { get; set; }
    public string guid { get; set; }
    public string name { get; set; }
    public string shortName { get; set; }
    public string displayName { get; set; }
    public string code { get; set; }
    public string indexNumber { get; set; }
    public string url { get; set; }
    public string DunsNumber { get; set; }
    public DateTime createTime { get; set; }
    public bool isDefault { get; set; }
    public bool isVisible { get; set; }
    public int affintyLevel { get; set; }
    public int firstOrgSchemaId { get; set; }
    public string firstOrgSchemaName { get; set; }
  }

  public class OrgSchemaDTO
  {
    public int orgSchemaId { get; set; }
    public string guid { get; set; }
    public string name { get; set; }
    public string shortName { get; set; }
    public string displayName { get; set; }
    public string code { get; set; }
    public string indexNumber { get; set; }
    public bool isDefault { get; set; }
    public bool isVisible { get; set; }
    public DateTime createTime { get; set; }
    public ICollection<BizEntityDTO> rootBizEntities { get; set; }
  }

  public class BizEntityDTO
  {
    public int bizEntityId { get; set; }
    public string guid { get; set; }
    public string name { get; set; }
    public string shortName { get; set; }
    public string displayName { get; set; }
    public string englishName { get; set; }
    public string code { get; set; }
    public string indexNumber { get; set; }
    public string url { get; set; }
    public string DunsNumber { get; set; }
    public bool isVisible { get; set; }
    public DateTime createTime { get; set; }
    public int firstbizEntitySchemaId { get; set; }
    public string firstbizEntitySchemaName { get; set; }
  }

  public class BizEntitySchemaDTO
  {
    public int bizEntitySchemaId { get; set; }
    public string guid { get; set; }
    public string name { get; set; }
    public string shortName { get; set; }
    public string displayName { get; set; }
    public string englishName { get; set; }
    public string code { get; set; }
    public string indexNumber { get; set; }
    public bool isVisible { get; set; }
    public DateTime createTime { get; set; }
    public List<DepartmentDTO> rootDepartments { get; set; }
  }

  public class DepartmentDTO
  {
    public int departmentId { get; set; }
    public string guid { get; set; }
    public int assistBizEntitySchemaId { get; set; }
    public string name { get; set; }
    public string shortName { get; set; }
    public string displayName { get; set; }
    public string englishName { get; set; }
    public string code { get; set; }
    public string indexNumber { get; set; }
    public bool isVisible { get; set; }
    public DateTime createTime { get; set; }
    public List<UserDTO> users { get; set; }
    public List<DepartmentDTO> departments { get; set; }
  }

  public class RoleDTO
  {
    public int roleId { get; set; }
    public string guid { get; set; }
    public string name { get; set; }
    public string displayName { get; set; }
    public string englishName { get; set; }
    public string code { get; set; }
    public string indexNumber { get; set; }
    public bool isVisible { get; set; }
    public DateTime createTime { get; set; }
    public List<UserDTO> users { get; set; }
  }

  public class UserDTO
  {
    public int userId { get; set; }
    public string guid { get; set; }
    public string name { get; set; }
    public string displayName { get; set; }
    public string englishName { get; set; }
    public string code { get; set; }
    public string indexNumber { get; set; }
    public string email { get; set; }
    public string accountInNT { get; set; }
    public string logonName { get; set; } //用户的登录账户号
    public string officeTel { get; set; }
    public string personalTel { get; set; }
    public string personalMobile { get; set; }
    public bool isVisible { get; set; }
    public DateTime createTime { get; set; }
    public DateTime? validTimeFrom { get; set; }
    public DateTime? validTimeTo { get; set; }
    public List<string> departmentNames { get; set; } // 用于辅助search
    public string defaultDepartmentName { get; set; }
    public string defaultDepartmentGuid { get; set; }
    public int? defaultDepartmentId { get; set; }
    public List<string> roleNames { get; set; } // 用于辅助search

    //TODO 其他的补充属性可后面再加或者考虑用complex type来完成,尤其是可自定义的custom attributes
  }

  public class RoleTypeDTO 
  {
    public int roleTypeId { get; set; }
    public string guid { get; set; }
    public string name { get; set; }
    public bool isVisible { get; set; } = true;
    public DateTime createTime { get; set; }

    public List<RoleDTO> roles { get; set; }
  }
}
