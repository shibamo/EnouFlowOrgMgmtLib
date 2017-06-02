using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnouFlowOrgMgmtLib
{
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
}
