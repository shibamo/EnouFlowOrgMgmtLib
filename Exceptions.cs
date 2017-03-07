using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnouFlowOrgMgmtLib
{
  //不能重名
  public class NameDuplicationException: Exception
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
