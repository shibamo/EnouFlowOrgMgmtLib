using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnouFlowOrgMgmtLib
{
  //不能修改GUID
  public class GuidNotAllowedToChangeException : Exception
  {
    public GuidNotAllowedToChangeException()
    {
    }

    public GuidNotAllowedToChangeException(string message)
        : base(message)
    {
    }

    public GuidNotAllowedToChangeException(string message, Exception inner)
        : base(message, inner)
    {
    }
  }
}
