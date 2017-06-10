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
  public class UserHelperTestsI
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
    public void setUserDepartment_newRelation_willExist()
    {
      DepartmentHelper o = new DepartmentHelper(db);
      BizEntitySchema bizEntitySchema = db.bizEntitySchemas.FirstOrDefault();
      if (bizEntitySchema == null) Assert.Ignore("Not found BizEntitySchema object");
      Department department = bizEntitySchema.getRootDepartments(db).FirstOrDefault();
      if (department == null) Assert.Ignore("Not found department object");
      UserHelper userHelper = new UserHelper(db);
      User user = userHelper.createObject();
      user.name = "Integration_Test_XXXYYYZZZ_1";
      userHelper.saveCreatedObject(user);

      userHelper.setUserDepartment(user.userId, department,
        UserPositionToDepartment.other);

      Assert.AreEqual(1,
        db.departmentUserRelations.Where(
          r => r.assistDepartmentId == department.departmentId &&
          r.assistUserId == user.userId).Count());

      Assert.AreEqual(UserPositionToDepartment.other,
        db.departmentUserRelations.Where(
          r => r.assistDepartmentId == department.departmentId &&
          r.assistUserId == user.userId).First().userPosition);
    }

    [Test]
    [Category("Integration")]
    [Rollback]
    public void setUserDepartment_existingRelation_willUpdate()
    {
      DepartmentHelper o = new DepartmentHelper(db);
      BizEntitySchema bizEntitySchema = db.bizEntitySchemas.FirstOrDefault();
      if (bizEntitySchema == null) Assert.Ignore("Not found BizEntitySchema object");
      Department department = bizEntitySchema.getRootDepartments(db).FirstOrDefault();
      if (department == null) Assert.Ignore("Not found department object");
      UserHelper userHelper = new UserHelper(db);
      User user = userHelper.createObject();
      user.name = "Integration_Test_XXXYYYZZZ_1";
      userHelper.saveCreatedObject(user);
      userHelper.setUserDepartment(user.userId, department,
        UserPositionToDepartment.other);

      userHelper.setUserDepartment(user.userId, department,
        UserPositionToDepartment.manager);

      Assert.AreEqual(1,
        db.departmentUserRelations.Where(
          r => r.assistDepartmentId == department.departmentId &&
          r.assistUserId == user.userId).Count());
      Assert.AreEqual(UserPositionToDepartment.manager,
        db.departmentUserRelations.Where(
          r => r.assistDepartmentId == department.departmentId &&
          r.assistUserId == user.userId).First().userPosition);
    }

    [Test]
    [Category("Integration")]
    [Rollback]
    public void createDepartmentUserRelation_existingSameRelation_willThrow()
    {
      DepartmentHelper o = new DepartmentHelper(db);
      BizEntitySchema bizEntitySchema = db.bizEntitySchemas.FirstOrDefault();
      if (bizEntitySchema == null) Assert.Ignore("Not found BizEntitySchema object");
      Department department = bizEntitySchema.getRootDepartments(db).FirstOrDefault();
      if (department == null) Assert.Ignore("Not found department object");
      UserHelper userHelper = new UserHelper(db);
      User user = userHelper.createObject();
      user.name = "Integration_Test_XXXYYYZZZ_1";
      userHelper.saveCreatedObject(user);
      userHelper.setUserDepartment(user.userId, department,
        UserPositionToDepartment.other);

      Assert.Throws<DataLogicException>(() =>
        userHelper.createDepartmentUserRelation(department, user,
          UserPositionToDepartment.manager));
    }

    [Test]
    [Category("Integration")]
    [Rollback]
    public void createDepartmentUserRelation_existingOtherRelationOfSimpleMode_willThrow()
    {
      DepartmentHelper o = new DepartmentHelper(db);
      BizEntitySchema bizEntitySchema = db.bizEntitySchemas.FirstOrDefault();
      if (bizEntitySchema == null) Assert.Ignore("Not found BizEntitySchema object");
      Department departmentParent = bizEntitySchema.getRootDepartments(db).FirstOrDefault();
      if (departmentParent == null) Assert.Ignore("Not found departmentParent object");
      Department department = o.createObject();
      department.name = "Integration_Test_XXXYYYZZZ";
      o.saveCreatedObject(bizEntitySchema, department, departmentParent);
      UserHelper userHelper = new UserHelper(db);
      User user = userHelper.createObject();
      user.name = "Integration_Test_XXXYYYZZZ_1";
      userHelper.saveCreatedObject(user);
      userHelper.setUserDepartment(user.userId, department,
        UserPositionToDepartment.other);

      Assert.Throws<DataLogicException>(() =>
        userHelper.createDepartmentUserRelation(department, user,
          UserPositionToDepartment.manager));
    }

    [Test]
    [Category("Integration")]
    [Rollback]
    public void logonUsert_validInformation_willReturnNotNullUser()
    {
      UserHelper userHelper = new UserHelper(db);
      var obj = userHelper.createObject();
      obj.name = "Integration_Test_XXXYYYZZZ";
      obj.logonName = "Integration_Test_XXXYYYZZZ";
      string logonPasswordOrigin = "Fake_Password@Integration_Test123";
      var PasswordHashAndSalt =
        UserHelper.generatePasswordHashAndSalt(logonPasswordOrigin);
      obj.logonPasswordHash = PasswordHashAndSalt.Item1;
      obj.logonSalt = PasswordHashAndSalt.Item2;
      userHelper.saveCreatedObject(obj);

      var user = userHelper.logonUser(
        "Integration_Test_XXXYYYZZZ",
        "Fake_Password@Integration_Test123");

      Assert.NotNull(user);
    }

    [Test]
    [Category("Integration")]
    [Rollback]
    public void logonUsert_invalidInformation_willReturnNull()
    {
      UserHelper userHelper = new UserHelper(db);
      var obj = userHelper.createObject();
      obj.name = "Integration_Test_XXXYYYZZZ";
      obj.logonName = "Integration_Test_XXXYYYZZZ";
      string logonPasswordOrigin = "Fake_Password@Integration_Test123";
      var PasswordHashAndSalt =
        UserHelper.generatePasswordHashAndSalt(logonPasswordOrigin);
      obj.logonPasswordHash = PasswordHashAndSalt.Item1;
      obj.logonSalt = PasswordHashAndSalt.Item2;
      userHelper.saveCreatedObject(obj);

      Assert.Null(userHelper.logonUser(
        "Integration_Test_XXXYYYZZZ",
        "Wrong_Password"));
      Assert.Null(userHelper.logonUser(
        "Wrong_User",
        "Fake_Password@Integration_Test123"));
    }

    [Test]
    [Category("Integration")]
    [Rollback]
    public void setUserRole_newRelation_willExist()
    {
      RoleHelper o = new RoleHelper(db);
      Role role = o.createObject();
      role.name = "Integration_Test_XXXYYYZZZ";
      o.saveCreatedObject(role);
      UserHelper userHelper = new UserHelper(db);
      User user = userHelper.createObject();
      user.name = "Integration_Test_XXXYYYZZZ_1";
      userHelper.saveCreatedObject(user);

      userHelper.setUserRole(user.userId, role);

      Assert.AreEqual(1,
        db.roleUserRelations.Where(r =>
          r.assistRoleId == role.roleId &&
          r.assistUserId == user.userId &&
          r.isValid).Count());
      Assert.AreEqual(1,
        user.roleUserRelations.Where(r =>
          r.assistRoleId == role.roleId &&
          r.assistUserId == user.userId &&
          r.isValid).Count());
      Assert.AreEqual(1,
        role.roleUserRelations.Where(r =>
          r.assistRoleId == role.roleId &&
          r.assistUserId == user.userId &&
          r.isValid).Count());
    }

    [Test]
    [Category("Integration")]
    [Rollback]
    public void setUserRole_oldRelation_willExist()
    {
      RoleHelper o = new RoleHelper(db);
      Role role = o.createObject();
      role.name = "Integration_Test_XXXYYYZZZ";
      o.saveCreatedObject(role);
      UserHelper userHelper = new UserHelper(db);
      User user = userHelper.createObject();
      user.name = "Integration_Test_XXXYYYZZZ_1";
      userHelper.saveCreatedObject(user);
      userHelper.setUserRole(user.userId, role);
      userHelper.unsetUserRole(user.userId, role);

      userHelper.setUserRole(user.userId, role);

      Assert.AreEqual(1,
        db.roleUserRelations.Where(r =>
          r.assistRoleId == role.roleId &&
          r.assistUserId == user.userId &&
          r.isValid).Count());
      Assert.AreEqual(1,
        user.roleUserRelations.Where(r =>
          r.assistRoleId == role.roleId &&
          r.assistUserId == user.userId &&
          r.isValid).Count());
      Assert.AreEqual(1,
        role.roleUserRelations.Where(r =>
          r.assistRoleId == role.roleId &&
          r.assistUserId == user.userId &&
          r.isValid).Count());
    }

    [Test]
    [Category("Integration")]
    [Rollback]
    public void createRoleUserRelation_oldRelation_willThrow()
    {
      RoleHelper o = new RoleHelper(db);
      Role role = o.createObject();
      role.name = "Integration_Test_XXXYYYZZZ";
      o.saveCreatedObject(role);
      UserHelper userHelper = new UserHelper(db);
      User user = userHelper.createObject();
      user.name = "Integration_Test_XXXYYYZZZ_1";
      userHelper.saveCreatedObject(user);
      userHelper.setUserRole(user.userId, role);

      Assert.Throws<DataLogicException>(()=> 
        userHelper.createRoleUserRelation(role, user));
    }

    [TearDown]
    public void TearDown()
    {
      db.Dispose();
    }
  }
}
