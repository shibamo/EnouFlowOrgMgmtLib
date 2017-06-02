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
}
