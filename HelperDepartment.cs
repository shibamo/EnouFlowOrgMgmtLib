using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnouFlowOrgMgmtLib
{
  public static partial class OrgMgmtDBHelper
  {
    #region Department related
    public static Department createDepartment(EnouFlowOrgMgmtContext db)
    {
      return db.departments.Create();
    }

    public static void saveCreatedDepartment(BizEntitySchema bizEntitySchema, Department department,
      Department departmentParent, EnouFlowOrgMgmtContext db)
    {
      if (!db.departments.ToList().Contains(department)) //新创建尚未存入DB的Department
      {
        //db.departments.Add(department);
        department.assistBizEntitySchemaId = bizEntitySchema.bizEntitySchemaId;
        bizEntitySchema.departments.Add(department);
      }

      //验证一个Department不能有多个Parent Department
      if (db.departmentParentChildRelations.ToList().Where(
        r => r.departmentIdChild == department.departmentId).
        ToList().Count() > 0)
      {
        throw new Exception(
          string.Format("部门'{0}'已属于其他部门", department.name));
      }

      DepartmentParentChildRelation departmentParentChildRelation
        = db.departmentParentChildRelations.Create();
      departmentParentChildRelation.assistBizEntitySchemaId =
        bizEntitySchema.bizEntitySchemaId;
      departmentParentChildRelation.departmentChild = department;
      if (departmentParent != null)
      {
        departmentParentChildRelation.departmentParent = departmentParent;
      }
      bizEntitySchema.departmentParentChildRelations.Add(departmentParentChildRelation);

      db.SaveChanges();
    }

    public static void setParentDepartment(int id, Department bizDepartmentParent, int bizEntitySchemaId, EnouFlowOrgMgmtContext db)
    {
      var bizEntitySchema = db.bizEntitySchemas.Find(bizEntitySchemaId);
      var department = db.departments.Find(id);

      #region check validity
      if (!isDepartmentSetParentAllowed(
        id, bizDepartmentParent, bizEntitySchemaId, db))
      {
        return;
      }
      #endregion

      #region Delete current existing parent-child relation
      var currentDepartmentParentChildRelation = db.departmentParentChildRelations
          .Where(r => r.assistBizEntitySchemaId == bizEntitySchemaId &&
          r.departmentIdChild == id).ToList().FirstOrDefault();
      db.departmentParentChildRelations.Remove(currentDepartmentParentChildRelation);
      #endregion

      #region construct new parent-child relation
      DepartmentParentChildRelation departmentParentChildRelation
        = db.departmentParentChildRelations.Create();
      departmentParentChildRelation.assistBizEntitySchemaId =
        bizEntitySchema.bizEntitySchemaId;
      departmentParentChildRelation.departmentChild = department;
      if (bizDepartmentParent != null)
      {
        departmentParentChildRelation.departmentParent = bizDepartmentParent;
      }
      bizEntitySchema.departmentParentChildRelations.Add(departmentParentChildRelation);
      #endregion

      db.SaveChanges();
    }

    public static bool removeDepartment(int id, EnouFlowOrgMgmtContext db)
    {
      var department = db.departments.Find(id);

      if (department != null && isDepartmentRemovable(id, db))
      {
        department.isVisible = false;
      }

      db.SaveChanges();

      return true;
    }

    public static DepartmentDTO convertDepartment2DTO(Department obj,
      EnouFlowOrgMgmtContext db)
    {
      return new DepartmentDTO()
      {
        departmentId = obj.departmentId,
        guid = obj.guid,
        assistBizEntitySchemaId = obj.assistBizEntitySchemaId,
        name = obj.name,
        englishName = obj.englishName,
        displayName = obj.displayName,
        shortName = obj.shortName,
        code = obj.code,
        indexNumber = obj.indexNumber,
        isVisible = obj.isVisible,
        createTime = obj.createTime,
        users = obj.getUserChildren(db).Select(
          u => convertUser2DTO(u, db)).ToList(),
        departments = obj.getDepartmentChildren(db)
        .Select(d=>convertDepartment2DTO(d,db)).ToList(),
      };
    }

    public static List<DepartmentDTO> getAllDepartmentDTOs(
      EnouFlowOrgMgmtContext db) {
      return db.departments.ToList().Select(department =>
        convertDepartment2DTO(department, db)).ToList();
    }

    private static bool isDepartmentRemovable(int id, EnouFlowOrgMgmtContext db)
    {
      var department = db.departments.Find(id);
      #region 不能删除带有子节点的Department
      if (db.departmentParentChildRelations.ToList().Where(
        r => r.departmentParent == department &&
        r.departmentChild.isVisible).Count() > 0)
      {
        throw new DataLogicException("不能删除带有子节点的Department");
      }
      #endregion

      return true;
    }

    private static bool isDepartmentSetParentAllowed(int id, Department parent,
      int bizEntitySchemaId, EnouFlowOrgMgmtContext db)
    {
      #region Cannot be the parent of self
      if (parent != null)
      {
        if (id == parent.departmentId)
        {
          throw new DataLogicException("Cannot be the parent of self!");
        }
      }
      #endregion

      #region Parent不能为自己的子孙,判断方法为从parent开始逐级找祖先,判断departmentId是否为id
      Department currentParent = parent;
      while (currentParent != null)
      {
        if (currentParent.departmentId == id)
        {
          throw new DataLogicException("设置的祖先不能为自己的子孙节点!");
        }
        var currentDepartmentRelation = db.departmentParentChildRelations
          .Where(r => r.assistBizEntitySchemaId == bizEntitySchemaId &&
            r.departmentIdChild == currentParent.departmentId).ToList().FirstOrDefault();
        if (currentDepartmentRelation != null)
        {
          currentParent = currentDepartmentRelation.departmentParent;
        }
        else
        {
          currentParent = null;
        }
      }
      #endregion

      return true;
    }

    public static bool isDepartmentChangeAllowed(int id, Department value,
      EnouFlowOrgMgmtContext db)
    {
      if (db.departments.AsNoTracking().Where(
        obj => obj.departmentId == id).ToList().FirstOrDefault().guid != value.guid)
      {
        throw new GuidNotAllowedToChangeException("不能修改对象GUID!");
      }

      return true;
    }

    public static bool isDepartmentExists(int id, EnouFlowOrgMgmtContext db)
    {
      return db.departments.Count(e => e.departmentId == id) > 0;
    }

    #endregion

    #region DepartmentUserRelation related
    public static void saveDepartmentUserRelation(Department department, User user,
        EnouFlowOrgMgmtContext db, UserPositionToDepartment
        userPositionToDepartment = UserPositionToDepartment.normal)
    {
      //不能重复建立部门-用户隶属关系(这里不限制用户可以同时属于多个部门)
      if (user.departmentUserRelations.ToList().Select(
        r => r.assistDepartmentId).ToList().Contains(
          department.departmentId))
      {
        return;

        throw new Exception(
          string.Format("用户'{0}'已属于部门'{1}'", user.name, department.name));
      }

      // simpleMode下一个用户不能同时属于多个部门
      if (schemeMode == SchemeMode.simpleMode)
      {
        if (user.departmentUserRelations.ToList().Count() > 0)
        {
          return;

          throw new Exception(
            string.Format("目前模式下,一个用户不能同时属于多个部门."));
        }
      }

      var departmentUserRelation = db.departmentUserRelations.Create();
      user.departmentUserRelations.Add(departmentUserRelation);
      department.departmentUserRelations.Add(departmentUserRelation);
      departmentUserRelation.assistDepartmentId = department.departmentId;
      departmentUserRelation.assistUserId = user.userId;
      departmentUserRelation.userPosition = userPositionToDepartment;

      db.SaveChanges();
    }
    #endregion
  }
}
