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
  public class OrgSchemaHelperTestsI
  {
    private EnouFlowOrgMgmtContext db = null;

    [SetUp]
    public void Setup()
    {
      db = new EnouFlowOrgMgmtContext();
    }

    [TestCase(SchemeMode.simpleMode)]
    [TestCase(SchemeMode.multiDepartmentForOneUserMode)]
    [Category("Integration")]
    [Rollback]
    public void saveCreatedObject_moreValidObjInDisallowedSchemeMode_throws(SchemeMode mode)
    {
      var org = db.orgs.FirstOrDefault();
      OrgSchemaHelper osHelper = new OrgSchemaHelper(db);
      var os = osHelper.createObject();
      os.name = "Integration_Test";
      os.Org = org;
      OrgMgmtDBHelper.schemeMode = mode;

      Assert.Throws<DataLogicException>(()=> osHelper.saveCreatedObject(os));
    }

    [TestCase(SchemeMode.multliOrgSchemaMode)]
    [TestCase(SchemeMode.multiBizEntitySchemaMode)]
    [Category("Integration")]
    [Rollback]
    public void saveCreatedObject_moreValidObjInAllowedSchemeMode_success(SchemeMode mode)
    {
      var org = db.orgs.FirstOrDefault();
      var count = org.orgSchemas.Count;
      OrgSchemaHelper osHelper = new OrgSchemaHelper(db);
      var os = osHelper.createObject();
      os.name = "Integration_Test";
      os.Org = org;
      OrgMgmtDBHelper.schemeMode = mode;

      osHelper.saveCreatedObject(os);

      Assert.AreEqual(count+1, org.orgSchemas.Count);
    }

    [TearDown]
    public void TearDown()
    {
      db.Dispose();
    }
  }
}
