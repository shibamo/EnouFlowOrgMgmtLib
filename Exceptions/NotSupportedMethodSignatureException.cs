using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnouFlowOrgMgmtLib
{
  class NotSupportedMethodSignatureException : Exception
  {
    public NotSupportedMethodSignatureException()
    {
    }

    public NotSupportedMethodSignatureException(string message)
        : base(message)
    {
    }

    public NotSupportedMethodSignatureException(string message, Exception inner)
        : base(message, inner)
    {
    }
  }
}
