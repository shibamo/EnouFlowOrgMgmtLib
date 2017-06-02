using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnouFlowOrgMgmtLib
{
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
}
