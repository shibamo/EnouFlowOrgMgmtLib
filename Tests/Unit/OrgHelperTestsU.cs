using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;
using NSubstitute;

namespace EnouFlowOrgMgmtLib.Tests.Unit
{
  [TestFixture]
  public class OrgHelperTestsU
  {
    [Test]
    [Category("Unit")]
    public void constructor_WithDBContext_HasDBContext()
    {
      using (var db = new EnouFlowOrgMgmtContext())
      {
        OrgHelper o = new OrgHelper(db);
        Assert.IsNotNull(o._dbContext);
      }
    }

    [Test]
    [Category("Unit")]
    public void createObject_NotSetDBContext_ThrowsException()
    {
      OrgHelper o = new OrgHelper();

      Assert.Throws<DataLogicException>(() => o.createObject());
    }

  }
}
