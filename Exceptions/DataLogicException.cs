using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnouFlowOrgMgmtLib
{
  //数据逻辑错误
  public class DataLogicException : Exception
  {
    public DataLogicException()
    {
    }

    public DataLogicException(string message)
        : base(message)
    {
    }

    public DataLogicException(string message, Exception inner)
        : base(message, inner)
    {
    }
  }
}
