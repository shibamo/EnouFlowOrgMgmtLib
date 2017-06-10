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
  public class OrgHelperTestsI
  {
    private EnouFlowOrgMgmtContext db = null;

    [SetUp]
    public void Setup()
    {
      db = new EnouFlowOrgMgmtContext();
    }

    [Test]
    [Category("Integration")]
    public void convert2DTO_withOrgObject_returnDTOObject()
    {
      OrgHelper o = new OrgHelper(db);
      var org = db.orgs.FirstOrDefault();

      var dto = o.convert2DTO(org);

      Assert.IsInstanceOf<OrgDTO>(dto);
    }

    [Test]
    [Category("Integration")]
    public void getObject_validIntId_returnObject()
    {
      OrgHelper o = new OrgHelper(db);
      var org = db.orgs.FirstOrDefault();
      var testOrgId = org.orgId;

      var testOrg = o.getObject(testOrgId);

      Assert.IsInstanceOf<Org>(testOrg);
    }

    [Test]
    [Category("Integration")]
    public void isObjectChangeAllowed_changedGuidObject_ThrowsException()
    {
      OrgHelper o = new OrgHelper(db);
      var org = db.orgs.FirstOrDefault();
      org.guid = new Guid().ToString();

      Assert.Throws<GuidNotAllowedToChangeException>(() => o.isObjectChangeAllowed(org.orgId, org));
    }

    [Test]
    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(99999)]
    [Category("Integration")]
    public void isObjectExists_invalidId_returnFalse(int id)
    {
      OrgHelper o = new OrgHelper(db);

      var bResult = o.isObjectExists(id);

      Assert.False(bResult);
    }

    [Test]
    [Category("Integration")]
    public void isObjectExists_validId_returnTrue()
    {
      OrgHelper o = new OrgHelper(db);
      var org = db.orgs.FirstOrDefault();

      var bResult = o.isObjectExists(org.orgId);

      Assert.True(bResult);
    }

    [Test]
    [Category("Integration")]
    [Rollback]
    public void removeObject_validId_willSetVisibleToFalse()
    {
      OrgHelper o = new OrgHelper(db);
      var org = db.orgs.FirstOrDefault();

      var bResult = o.removeObject(org.orgId);

      Assert.False(org.isVisible);
    }

    [Test]
    [Category("Integration")]
    [Rollback]
    public void saveCreatedObject_validObj_willExist()
    {
      OrgHelper o = new OrgHelper(db);
      var org = o.createObject();
      org.name = "Integration_Test_XXXYYYZZZ";

      o.saveCreatedObject(org);

      Assert.True(o.isObjectExists(org.orgId));
    }

    [Test]
    [Category("Integration")]
    [Rollback]
    public void saveUpdatedObject_validObj_willExist()
    {
      OrgHelper o = new OrgHelper(db);
      var org = db.orgs.FirstOrDefault();
      var orgGuid = org.guid;
      org.name = "Integration_Test_XXXYYYZZZ_00";

      o.saveUpdatedObject(org);

      Assert.AreEqual("Integration_Test_XXXYYYZZZ_00", o.getObject(orgGuid).name);
    }

    [TearDown]
    public void TearDown()
    {
      db.Dispose();
    }
  }
}
