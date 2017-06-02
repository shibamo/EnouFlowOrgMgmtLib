using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnouFlowOrgMgmtLib
{
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
