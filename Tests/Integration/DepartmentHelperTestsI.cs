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
  public class DepartmentHelperTestsI
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
    public void saveCreatedObject_validObjWithNoParent_willExist()
    {
      DepartmentHelper o = new DepartmentHelper(db);
      Department department = o.createObject();
      department.name = "Integration_Test_XXXYYYZZZ";
      BizEntitySchema bizEntitySchema = db.bizEntitySchemas.FirstOrDefault();
      if (bizEntitySchema == null) Assert.Ignore("Not found BizEntitySchema object");

      o.saveCreatedObject(bizEntitySchema, department, null);

      Assert.AreEqual(1, 
        db.departments.AsNoTracking().Where(
          d=>d.departmentId==department.departmentId).Count());
      Assert.AreEqual(1,
        db.departmentParentChildRelations.Where(
          dpcr => dpcr.departmentIdParent==null && 
            dpcr.departmentIdChild==department.departmentId &&
            dpcr.assistBizEntitySchemaId == bizEntitySchema.bizEntitySchemaId).Count());
    }

    [Test]
    [Category("Integration")]
    [Rollback]
    public void saveCreatedObject_validObjWithParent_willExist()
    {
      DepartmentHelper o = new DepartmentHelper(db);
      Department department = o.createObject();
      department.name = "Integration_Test_XXXYYYZZZ";
      BizEntitySchema bizEntitySchema = db.bizEntitySchemas.FirstOrDefault();
      if (bizEntitySchema == null) Assert.Ignore("Not found BizEntitySchema object");
      Department departmentParent = bizEntitySchema.getRootDepartments(db).FirstOrDefault();
      if (departmentParent == null) Assert.Ignore("Not found departmentParent object");

      o.saveCreatedObject(bizEntitySchema, department, departmentParent);

      Assert.AreEqual(1,
        db.departments.AsNoTracking().Where(
          d => d.departmentId == department.departmentId).Count());
      Assert.AreEqual(1,
        db.departmentParentChildRelations.Where(
          dpcr => dpcr.departmentIdParent == departmentParent.departmentId &&
            dpcr.departmentIdChild == department.departmentId &&
            dpcr.assistBizEntitySchemaId == bizEntitySchema.bizEntitySchemaId).Count());
    }

    [Test]
    [Category("Integration")]
    [Rollback]
    public void saveCreatedObject_validObjWithMultipleParent_willThrow()
    {
      DepartmentHelper o = new DepartmentHelper(db);
      Department department = o.createObject();
      department.name = "Integration_Test_XXXYYYZZZ";
      BizEntitySchema bizEntitySchema = db.bizEntitySchemas.FirstOrDefault();
      if (bizEntitySchema == null) Assert.Ignore("Not found BizEntitySchema object");
      Department departmentParent = bizEntitySchema.getRootDepartments(db).FirstOrDefault();
      if (departmentParent == null) Assert.Ignore("Not found departmentParent object");
      Department departmentParent2 = o.createObject();
      departmentParent2.name = "Integration_Test_Another_Department";
      o.saveCreatedObject(bizEntitySchema, departmentParent2, departmentParent);
      o.saveCreatedObject(bizEntitySchema, department, departmentParent);

      Assert.Throws<DataLogicException>(()=>
        o.saveCreatedObject(bizEntitySchema, department, departmentParent2),
        string.Format("部门'{0}'已被创建并属于其他部门， 请调用setParentDepartment方法",
            department.name));
    }

    [Test]
    [Category("Integration")]
    [Rollback]
    public void setParentDepartment_validObjToNoParent_willCreateDepartmentParentChildRelation()
    {
      DepartmentHelper o = new DepartmentHelper(db);
      Department department = o.createObject();
      department.name = "Integration_Test_XXXYYYZZZ";
      BizEntitySchema bizEntitySchema = db.bizEntitySchemas.FirstOrDefault();
      if (bizEntitySchema == null) Assert.Ignore("Not found BizEntitySchema object");
      Department departmentParent = bizEntitySchema.getRootDepartments(db).FirstOrDefault();
      if (departmentParent == null) Assert.Ignore("Not found departmentParent object");
      o.saveCreatedObject(bizEntitySchema, department, departmentParent);

      o.setParentDepartment(department.departmentId, null, bizEntitySchema.bizEntitySchemaId);

      var dpcrs = db.departmentParentChildRelations.Where(dpcr =>
        dpcr.assistBizEntitySchemaId == bizEntitySchema.bizEntitySchemaId &&
        dpcr.departmentIdChild == department.departmentId);

      Assert.AreEqual(1, dpcrs.Count());
      Assert.IsNull(dpcrs.FirstOrDefault().departmentParent);
    }

    [Test]
    [Category("Integration")]
    [Rollback]
    public void setParentDepartment_validObjToValidParent_willCreateDepartmentParentChildRelation()
    {
      DepartmentHelper o = new DepartmentHelper(db);
      Department department = o.createObject();
      department.name = "Integration_Test_XXXYYYZZZ";
      BizEntitySchema bizEntitySchema = db.bizEntitySchemas.FirstOrDefault();
      if (bizEntitySchema == null) Assert.Ignore("Not found BizEntitySchema object");
      Department departmentParent = bizEntitySchema.getRootDepartments(db).FirstOrDefault();
      if (departmentParent == null) Assert.Ignore("Not found departmentParent object");
      o.saveCreatedObject(bizEntitySchema, department, null);

      o.setParentDepartment(department.departmentId, departmentParent, bizEntitySchema.bizEntitySchemaId);

      var dpcrs = db.departmentParentChildRelations.Where(dpcr =>
        dpcr.assistBizEntitySchemaId == bizEntitySchema.bizEntitySchemaId &&
        dpcr.departmentIdChild == department.departmentId);

      Assert.AreEqual(1, dpcrs.Count());
      Assert.IsNotNull(dpcrs.FirstOrDefault().departmentParent);
    }

    [Test]
    [Category("Integration")]
    [Rollback]
    public void setParentDepartment_validObjToSelfChildren_willThrow()
    {
      DepartmentHelper o = new DepartmentHelper(db);
      Department department = o.createObject();
      department.name = "Integration_Test_XXXYYYZZZ";
      BizEntitySchema bizEntitySchema = db.bizEntitySchemas.FirstOrDefault();
      if (bizEntitySchema == null) Assert.Ignore("Not found BizEntitySchema object");
      Department departmentParent = bizEntitySchema.getRootDepartments(db).FirstOrDefault();
      if (departmentParent == null) Assert.Ignore("Not found departmentParent object");
      o.saveCreatedObject(bizEntitySchema, department, departmentParent);

      Assert.Throws<DataLogicException>(() =>
        o.setParentDepartment(departmentParent.departmentId, department, 
          bizEntitySchema.bizEntitySchemaId),
        "设置的祖先不能为自己的子孙节点!");
    }

    [Test]
    [Category("Integration")]
    [Rollback]
    public void setParentDepartment_validObjToSelfDescedent_willThrow()
    {
      DepartmentHelper o = new DepartmentHelper(db);
      Department department = o.createObject();
      department.name = "Integration_Test_XXXYYYZZZ";
      BizEntitySchema bizEntitySchema = db.bizEntitySchemas.FirstOrDefault();
      if (bizEntitySchema == null) Assert.Ignore("Not found BizEntitySchema object");
      Department departmentParent = bizEntitySchema.getRootDepartments(db).FirstOrDefault();
      if (departmentParent == null) Assert.Ignore("Not found departmentParent object");
      o.saveCreatedObject(bizEntitySchema, department, departmentParent);
      Department departmentLastLevel = o.createObject();
      departmentLastLevel.name = "Integration_Test_XXXYYYZZZ_LastLevel";
      o.saveCreatedObject(bizEntitySchema, departmentLastLevel, department);

      Assert.Throws<DataLogicException>(() =>
        o.setParentDepartment(departmentParent.departmentId, departmentLastLevel,
          bizEntitySchema.bizEntitySchemaId),
        "设置的祖先不能为自己的子孙节点!");
    }

    [Test]
    [Category("Integration")]
    [Rollback]
    public void removeDepartment_validObjWithNoChildren_willRemove()
    {
      DepartmentHelper o = new DepartmentHelper(db);
      Department department = o.createObject();
      department.name = "Integration_Test_XXXYYYZZZ";
      BizEntitySchema bizEntitySchema = db.bizEntitySchemas.FirstOrDefault();
      if (bizEntitySchema == null) Assert.Ignore("Not found BizEntitySchema object");
      Department departmentParent = bizEntitySchema.getRootDepartments(db).FirstOrDefault();
      if (departmentParent == null) Assert.Ignore("Not found departmentParent object");
      o.saveCreatedObject(bizEntitySchema, department, departmentParent);

      o.removeDepartment(department.departmentId);

      var dpcrs = db.departmentParentChildRelations.Where(dpcr =>
        dpcr.assistBizEntitySchemaId == bizEntitySchema.bizEntitySchemaId &&
        dpcr.departmentIdChild == department.departmentId);

      Assert.IsFalse(dpcrs.FirstOrDefault().departmentChild.isVisible);
    }

    [Test]
    [Category("Integration")]
    [Rollback]
    public void removeDepartment_validObjWithChildren_willThrow()
    {
      DepartmentHelper o = new DepartmentHelper(db);
      Department department = o.createObject();
      department.name = "Integration_Test_XXXYYYZZZ";
      BizEntitySchema bizEntitySchema = db.bizEntitySchemas.FirstOrDefault();
      if (bizEntitySchema == null) Assert.Ignore("Not found BizEntitySchema object");
      Department departmentParent = bizEntitySchema.getRootDepartments(db).FirstOrDefault();
      if (departmentParent == null) Assert.Ignore("Not found departmentParent object");
      o.saveCreatedObject(bizEntitySchema, department, departmentParent);

      Assert.Throws<DataLogicException>(()=>
        o.removeDepartment(departmentParent.departmentId),
        "不能直接删除带有子节点的Department");
    }

    [Test]
    [Category("Integration")]
    [Rollback]
    public void getUserDTOsOfPositionInDepartment_validCriteria_returnCorrectList()
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
      userHelper.createDepartmentUserRelation(
        department, user, UserPositionToDepartment.manager);
      User user2 = userHelper.createObject();
      user2.name = "Integration_Test_XXXYYYZZZ_2";
      userHelper.saveCreatedObject(user2);
      userHelper.createDepartmentUserRelation(
        department, user2, UserPositionToDepartment.normal);

      var userDTOs = o.getUserDTOsOfPositionInDepartment(
        department.departmentId, UserPositionToDepartment.manager);

      Assert.AreEqual(1, userDTOs.Where(u=>u.userId==user.userId).Count());
      Assert.Zero(userDTOs.Where(u => u.userId == user2.userId).Count());
    }

    [Test]
    [Category("Integration")]
    [Rollback]
    public void convert2DTO_departmentHasNoChildren_returnCorrectObject()
    {
      DepartmentHelper o = new DepartmentHelper(db);
      BizEntitySchema bizEntitySchema = db.bizEntitySchemas.FirstOrDefault();
      if (bizEntitySchema == null) Assert.Ignore("Not found BizEntitySchema object");
      Department departmentParent = bizEntitySchema.getRootDepartments(db).FirstOrDefault();
      if (departmentParent == null) Assert.Ignore("Not found departmentParent object");
      Department department = o.createObject();
      department.name = "Integration_Test_XXXYYYZZZ";
      o.saveCreatedObject(bizEntitySchema, department, departmentParent);

      var departmentDTO = o.convert2DTO(department);

      Assert.Zero(departmentDTO.users.Count());
      Assert.Zero(departmentDTO.departments.Count());
    }
    

    [TearDown]
    public void TearDown()
    {
      db.Dispose();
    }
  }
}
