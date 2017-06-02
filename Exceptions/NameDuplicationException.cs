using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnouFlowOrgMgmtLib
{
  //不能重名
  public class NameDuplicationException : Exception
  {
    public NameDuplicationException()
    {
    }

    public NameDuplicationException(string message)
        : base(message)
    {
    }

    public NameDuplicationException(string message, Exception inner)
        : base(message, inner)
    {
    }
  }
}
