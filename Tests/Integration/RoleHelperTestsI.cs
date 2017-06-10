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
  public class RoleHelperTestsI
  {
    private EnouFlowOrgMgmtContext db = null;

    [SetUp]
    public void Setup()
    {
      db = new EnouFlowOrgMgmtContext();
    }

    [Test]
    [Category("Integration")]
    [Rollback]
    public void setRole_RoleType_newRelation_willExist()
    {
      RoleTypeHelper o = new RoleTypeHelper(db);
      RoleType roleType = o.createObject();
      roleType.name = "Integration_Test_XXXYYYZZZ_RoleType";
      o.saveCreatedObject(roleType);
      RoleHelper roleHelper = new RoleHelper(db);
      Role role = roleHelper.createObject();
      role.name = "Integration_Test_XXXYYYZZZ_Role";
      roleHelper.saveCreatedObject(role);

      roleHelper.setRole_RoleType(role.roleId, roleType);

      Assert.AreEqual(1,
        db.role_RoleTypeRelations.Where(
          r => r.assistRoleId == role.roleId &&
          r.assistRoleTypeId == roleType.roleTypeId).Count());
    }

    [Test]
    [Category("Integration")]
    [Rollback]
    public void setRole_RoleType_oldInvalidRelation_willExist()
    {
      RoleTypeHelper o = new RoleTypeHelper(db);
      RoleType roleType = o.createObject();
      roleType.name = "Integration_Test_XXXYYYZZZ_RoleType";
      o.saveCreatedObject(roleType);
      RoleHelper roleHelper = new RoleHelper(db);
      Role role = roleHelper.createObject();
      role.name = "Integration_Test_XXXYYYZZZ_Role";
      roleHelper.saveCreatedObject(role);
      roleHelper.setRole_RoleType(role.roleId, roleType);
      roleHelper.unsetRole_RoleType(role.roleId, roleType);

      roleHelper.setRole_RoleType(role.roleId, roleType);

      Assert.AreEqual(1,
        db.role_RoleTypeRelations.Where(
          r => r.assistRoleId == role.roleId &&
          r.assistRoleTypeId == roleType.roleTypeId).Count());
    }

    [Test]
    [Category("Integration")]
    [Rollback]
    public void unsetRole_RoleType_oldRelation_willNotExist()
    {
      RoleTypeHelper o = new RoleTypeHelper(db);
      RoleType roleType = o.createObject();
      roleType.name = "Integration_Test_XXXYYYZZZ_RoleType";
      o.saveCreatedObject(roleType);
      RoleHelper roleHelper = new RoleHelper(db);
      Role role = roleHelper.createObject();
      role.name = "Integration_Test_XXXYYYZZZ_Role";
      roleHelper.saveCreatedObject(role);
      roleHelper.setRole_RoleType(role.roleId, roleType);

      roleHelper.unsetRole_RoleType(role.roleId, roleType);

      Assert.Zero(db.role_RoleTypeRelations.Where(
          r => r.assistRoleId == role.roleId &&
          r.assistRoleTypeId == roleType.roleTypeId &&
          r.isValid).Count());
    }

    [TearDown]
    public void TearDown()
    {
      db.Dispose();
    }
  }
}
