using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnouFlowOrgMgmtLib
{
  public enum SchemeMode
  {
    simpleMode,                     //简单模式, 不开启所有多方案的层次架构支持
    multiDepartmentForOneUserMode,  //只开启一个用户可以同时隶属于多个部门的支持
    multiBizEntitySchemaMode,       //开启业务实体级多方案的层次架构(包括了组织结构级多方案)支持
    multliOrgSchemaMode,            //开启组织结构级多方案的层次架构支持
  }

  //用户在部门中的作用
  public enum UserPositionToDepartment
  {
    normal,     //普通人员
    manager,    //管理者
    temporary,  //临时人员
    other,      //其他
  }
}
