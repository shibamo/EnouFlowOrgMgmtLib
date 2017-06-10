using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Diagnostics.Contracts;

namespace EnouFlowOrgMgmtLib
{
  public class DepartmentHelper : BaseHelper<Department, DepartmentDTO,
                  EnouFlowOrgMgmtContext, DepartmentHelper>
  {
    public DepartmentHelper() { }

    public DepartmentHelper(EnouFlowOrgMgmtContext dbContext) : base(dbContext)
    {
    }

    public override DepartmentDTO convert2DTO(Department obj)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");
      Contract.Requires<DataLogicException>(obj != null, "Department不能为空");

      UserHelper userHelper = new UserHelper(_dbContext);

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
        users = obj.getUserChildren(_dbContext)
          .Select(u => userHelper.convert2DTO(u)).ToList(),
        departments = obj.getDepartmentChildren(_dbContext)
          .Select(d => convert2DTO(d)).ToList(),
      };
    }

    public override Department createObject()
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      return _dbContext.departments.Create();
    }

    public override Department getObject(string id)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      return _dbContext.departments.Where(dep =>
        dep.guid == id).FirstOrDefault();
    }

    public override Department getObject(int id)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");
      return _dbContext.departments.Find(id);
    }

    public override bool isObjectChangeAllowed(int id, Department value)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      if (_dbContext.departments.AsNoTracking().Where(
        obj => obj.departmentId == id).ToList().FirstOrDefault().guid != value.guid)
      {
        throw new GuidNotAllowedToChangeException("不能修改对象GUID!");
      }

      return true;
    }

    public override bool isObjectExists(int id)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      return _dbContext.departments.Count(e => e.departmentId == id) > 0;
    }

    public override bool recoverObject(int id)
    {
      throw new NotImplementedException();
    }

    public override bool removeObject(int id)
    {
      throw new NotImplementedException();
    }

    public void saveCreatedObject(BizEntitySchema bizEntitySchema,
      Department department, Department departmentParent)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");
      Contract.Requires<DataLogicException>(bizEntitySchema != null, "BizEntitySchema不能为空");
      Contract.Requires<DataLogicException>(department != null, "Department不能为空");
      Contract.Requires<DataLogicException>(department != departmentParent,
        "Department的父Department不能为自身");

      # region 特定BizEntitySchema中被创建而尚未存入DB的Department
      if (!_dbContext.departments.ToList().Contains(department)) //
      {
        department.assistBizEntitySchemaId = bizEntitySchema.bizEntitySchemaId;
        bizEntitySchema.departments.Add(department);
      }
      #endregion

      # region 验证一个Department不能有多个Parent Department
      if (_dbContext.departmentParentChildRelations.ToList().Where(
        r => r.departmentIdChild == department.departmentId).
        ToList().Count() > 0)
      {
        throw new DataLogicException(
          string.Format("部门'{0}'已被创建并属于其他部门， 请调用setParentDepartment方法",
            department.name));
      }
      #endregion

      #region 建立指定BizEntitySchema下的部门父子关系（顶级部门其父部门为null）
      DepartmentParentChildRelation departmentParentChildRelation
        = _dbContext.departmentParentChildRelations.Create();
      departmentParentChildRelation.assistBizEntitySchemaId =
        bizEntitySchema.bizEntitySchemaId;
      departmentParentChildRelation.departmentChild = department;
      departmentParentChildRelation.departmentParent = departmentParent;
      bizEntitySchema.departmentParentChildRelations.Add(departmentParentChildRelation);
      #endregion

      _dbContext.SaveChanges();
    }

    public void setParentDepartment(int id, Department departmentParent,
      int bizEntitySchemaId)
    {
      var department = _dbContext.departments.Find(id);
      var bizEntitySchema = _dbContext.bizEntitySchemas.Find(bizEntitySchemaId);

      #region check validity
      if (!isDepartmentSetParentAllowed(id, department, departmentParent,
        bizEntitySchemaId, bizEntitySchema))
      {
        return;
      }
      #endregion

      #region Skip the processing if existing parent-child relation match the requirement
#warning To be implemented
      #endregion

      #region Delete current existing parent-child relation
      var currentDepartmentParentChildRelation = _dbContext.departmentParentChildRelations
          .Where(r => r.assistBizEntitySchemaId == bizEntitySchemaId &&
          r.departmentIdChild == id).ToList().FirstOrDefault();
      _dbContext.departmentParentChildRelations.Remove(currentDepartmentParentChildRelation);
      #endregion

      #region construct new parent-child relation
      DepartmentParentChildRelation departmentParentChildRelation
        = _dbContext.departmentParentChildRelations.Create();
      departmentParentChildRelation.assistBizEntitySchemaId =
        bizEntitySchema.bizEntitySchemaId;
      departmentParentChildRelation.departmentChild = department;
      if (departmentParent != null)
      {
        departmentParentChildRelation.departmentParent = departmentParent;
      }
      bizEntitySchema.departmentParentChildRelations.Add(departmentParentChildRelation);
      #endregion

      _dbContext.SaveChanges();
    }

    private bool isDepartmentSetParentAllowed(int departmentId,
      Department department, Department departmentParent,
      int bizEntitySchemaId, BizEntitySchema bizEntitySchema)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");
      Contract.Requires<DataLogicException>(department != null, "Department不能为空");
      Contract.Requires<DataLogicException>(department != departmentParent,
        "Department的父Department不能为自身");
      Contract.Requires<DataLogicException>(bizEntitySchema != null,
        "指定的BizEntitySchema不存在");

      #region Parent不能为自己的子孙,判断方法为从parent开始逐级找祖先,判断departmentId是否为id
      Department currentParent = departmentParent;
      while (currentParent != null)
      {
        if (currentParent.departmentId == departmentId)
        {
          throw new DataLogicException("设置的祖先不能为自己的子孙节点!");
        }
        var currentDepartmentRelation = _dbContext.departmentParentChildRelations
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

    public bool removeDepartment(int id)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      var department = _dbContext.departments.Find(id);
      if (isDepartmentRemovable(department))
      {
        // 目前选择只更改department的状态，不反映到相关的DepartmentUserRelation对象上
        department.isVisible = false;
        _dbContext.SaveChanges();
      }

      return true;
    }

    private bool isDepartmentRemovable(Department department)
    {
      Contract.Requires<DataLogicException>(department != null, "Department不能为空");

      #region 不能删除带有子节点的Department
      if (_dbContext.departmentParentChildRelations.ToList().Where(
        r => r.departmentParent == department &&
        r.departmentChild.isVisible).Count() > 0)
      {
        throw new DataLogicException("不能直接删除带有子节点的Department");
      }
      #endregion

      return true;
    }

    public List<DepartmentDTO> getAllDepartmentDTOs()
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      return _dbContext.departments.ToList().Select(
        department => convert2DTO(department)).ToList();
    }

    public List<UserDTO> getUserDTOsOfPositionInDepartment(
      int id, UserPositionToDepartment position)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      var relations = _dbContext.departmentUserRelations.Where(r => 
        r.assistDepartmentId == id && r.isValid && r.userPosition == position)
        .ToList();

      if (relations == null || relations.Count() == 0)
      {
        return new List<UserDTO>();
      }
      else
      {
        UserHelper userHelper = new UserHelper(_dbContext);

        return relations.Select(obj =>
          userHelper.getUserDTO(obj.assistUserId)).ToList();
      }
    }
  }
}
