using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Diagnostics.Contracts;

namespace EnouFlowOrgMgmtLib
{
  public class RoleHelper :
    BaseHelper<Role, RoleDTO, EnouFlowOrgMgmtContext, RoleHelper>,
    IDBObjectSimpleSaveObject<Role>
  {
    public RoleHelper() { }

    public RoleHelper(EnouFlowOrgMgmtContext dbContext) : base(dbContext)
    {
    }

    public override RoleDTO convert2DTO(Role obj)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      UserHelper userHelper = new UserHelper(_dbContext);
      return new RoleDTO()
      {
        roleId = obj.roleId,
        guid = obj.guid,
        name = obj.name,
        englishName = obj.englishName,
        displayName = obj.displayName,
        code = obj.code,
        indexNumber = obj.indexNumber,
        isVisible = obj.isVisible,
        createTime = obj.createTime,
        users = obj.getUsersBelongTo(_dbContext).
          Select(u => userHelper.convert2DTO(u)).ToList(),
      };
    }

    public override Role createObject()
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      return _dbContext.roles.Create();
    }

    public override Role getObject(string id)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      return _dbContext.roles.Where(r => r.guid == id).ToList().FirstOrDefault();
    }

    public override Role getObject(int id)
    {
      throw new NotImplementedException();
    }

    public override bool isObjectChangeAllowed(int id, Role value)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      if (_dbContext.roles.AsNoTracking().Where(
        obj => obj.roleId == id).ToList().FirstOrDefault().guid != value.guid)
      {
        throw new GuidNotAllowedToChangeException("不能修改对象GUID!");
      }
      return true;
    }

    public override bool isObjectExists(int id)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      return _dbContext.roles.Count(e => e.roleId == id) > 0;
    }

    public override bool recoverObject(int id)
    {
      throw new NotImplementedException();
    }

    public override bool removeObject(int id)
    {
      throw new NotImplementedException();
    }

    public void saveCreatedObject(Role obj)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      _dbContext.roles.Add(obj);
      _dbContext.SaveChanges();
    }

    public void saveUpdatedObject(Role obj)
    {
      throw new NotImplementedException();
    }

    public List<RoleDTO> getAllRoleDTOs(bool includingInvalid = true)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      IEnumerable<Role> roles;
      if (includingInvalid)
      {
        roles = _dbContext.roles;
      }
      else
      {
        roles = _dbContext.roles.Where(role => role.isVisible == true);
      }

      return _dbContext.roles.ToList().Select(r => convert2DTO(r)).ToList();
    }

    #region Role_RoleTypeRelation related
    public void createRole_RoleTypeRelation(Role role, RoleType roleType)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      //不能重复建立角色类型-角色隶属关系(但不限制角色可以同时属于多个角色类型)
      if (role.role_RoleTypeRelations.ToList().Select(
        r => r.assistRoleTypeId).ToList().Contains(roleType.roleTypeId))
      {
        return;

        throw new Exception(
          string.Format("角色'{0}'已属于角色类型'{1}'", role.name, roleType.name));
      }

      var role_RoleTypeRelation = _dbContext.role_RoleTypeRelations.Create();
      role.role_RoleTypeRelations.Add(role_RoleTypeRelation);
      roleType.role_RoleTypeRelations.Add(role_RoleTypeRelation);
      role_RoleTypeRelation.assistRoleId = role.roleId;
      role_RoleTypeRelation.assistRoleTypeId = roleType.roleTypeId;

      _dbContext.SaveChanges();
    }

    public void setRole_RoleType(int id, RoleType roleType)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      var role = _dbContext.roles.Find(id);

      #region check whether relation already setted, then re-set to valid
      if (role.getRoleTypesBelongTo(_dbContext, false)
        .Contains(roleType))
      {
        var r = role.role_RoleTypeRelations.Where(x =>
          x.assistRoleTypeId == roleType.roleTypeId).
          ToList().FirstOrDefault();
        r.isValid = true;
        _dbContext.SaveChanges();
        return;
      }
      #endregion

      createRole_RoleTypeRelation(role, roleType);
    }

    public void unsetRole_RoleType(int id, RoleType roleType)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      var role = _dbContext.roles.Find(id);

      if (role.getRoleTypesBelongTo(_dbContext, false)
        .Contains(roleType))
      {
        var r = role.role_RoleTypeRelations.Where(x =>
          x.assistRoleTypeId == roleType.roleTypeId).
          ToList().FirstOrDefault();
        r.isValid = false;
        _dbContext.SaveChanges();
      }
    }


    #endregion

  }
}
