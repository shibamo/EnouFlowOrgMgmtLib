using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;
using EnouFlowOrgMgmtLib.Tests.Attributes;
using NSubstitute;


namespace EnouFlowOrgMgmtLib.Tests.Integration
{
  [TestFixture]
  public class RoleTypeHelperTestsI
  {
    private EnouFlowOrgMgmtContext db = null;

    [SetUp]
    public void Setup()
    {
      db = new EnouFlowOrgMgmtContext();
    }











    [TearDown]
    public void TearDown()
    {
      db.Dispose();
    }
  }
}
