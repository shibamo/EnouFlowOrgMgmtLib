using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnouFlowOrgMgmtLib
{
  public static partial class OrgMgmtDBHelper
  {
    #region Role related
    public static Role createRole(EnouFlowOrgMgmtContext db)
    {
      return db.roles.Create();
    }

    public static void saveCreatedRole(Role role, EnouFlowOrgMgmtContext db)
    {
      db.roles.Add(role);
      db.SaveChanges();
    }

    public static void setRole_RoleType(int id, RoleType roleType, 
      EnouFlowOrgMgmtContext db)
    {
      var role = db.roles.Find(id);

      #region check whether relation already setted, then re-set to valid
      if (role.getRoleTypesBelongTo(db, false)
        .Contains(roleType))
      {
        var r = role.role_RoleTypeRelations.Where(x =>
          x.assistRoleTypeId == roleType.roleTypeId).
          ToList().FirstOrDefault();
        r.isValid = true;
        db.SaveChanges();
        return;
      }
      #endregion

      saveCreatedRole_RoleTypeRelation(role, roleType, db);
    }

    public static void unsetRole_RoleType(int id, RoleType roleType,
      EnouFlowOrgMgmtContext db)
    {
      var role = db.roles.Find(id);

      if (role.getRoleTypesBelongTo(db,false)
        .Contains(roleType))
      {
        var r = role.role_RoleTypeRelations.Where(x =>
          x.assistRoleTypeId == roleType.roleTypeId).
          ToList().FirstOrDefault();
        r.isValid = false;
        db.SaveChanges();
      }
    }

    public static bool isRoleExists(int id, EnouFlowOrgMgmtContext db)
    {
      return db.roles.Count(e => e.roleId == id) > 0;
    }

    public static bool isRoleChangeAllowed(int id, Role value,
      EnouFlowOrgMgmtContext db)
    {
      if (db.roles.AsNoTracking().Where(
        obj => obj.roleId == id).ToList().FirstOrDefault().guid != value.guid)
      {
        throw new GuidNotAllowedToChangeException("不能修改对象GUID!");
      }
      return true;
    }

    public static List<UserDTO> getUserDTOsOfRole(int id, 
      EnouFlowOrgMgmtContext db)
    {
      return db.roles.Find(id).getUsersBelongTo(db).Select(
        u => convertUser2DTO(u, db)).ToList();
    }

    public static List<RoleDTO> getAllRoleDTOs(
      EnouFlowOrgMgmtContext db, bool includingInvalid = true)
    {
      IEnumerable<Role> roles;
      if (includingInvalid)
      {
        roles = db.roles;
      }
      else
      {
        roles = db.roles.Where(role => role.isVisible == true);
      }

      return db.roles.ToList().Select(
        role => { return convertRole2DTO(role, db); }).ToList();
    }

    public static Role getRole(string guid, EnouFlowOrgMgmtContext db)
    {
      return db.roles.Where(role => role.guid == guid).
        ToList().FirstOrDefault();
    }

    public static RoleDTO convertRole2DTO(Role obj, 
      EnouFlowOrgMgmtContext db)
    {
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
        users = obj.getUsersBelongTo(db).Select(
          u => convertUser2DTO(u, db)).ToList(),
      };
    }
    #endregion

    #region RoleUserRelation related
    public static void saveCreatedRoleUserRelation(Role role, User user,
      EnouFlowOrgMgmtContext db)
    {
      //不能重复建立角色-用户隶属关系(但不限制用户可以同时属于多个角色)
      if (user.roleUserRelations.ToList().Select(
        r => r.assistRoleId).ToList().Contains(role.roleId))
      {
        return;

        throw new Exception(
          string.Format("用户'{0}'已属于角色'{1}'", user.name, role.name));
      }

      var roleUserRelation = db.roleUserRelations.Create();

      user.roleUserRelations.Add(roleUserRelation);
      role.roleUserRelations.Add(roleUserRelation);
      roleUserRelation.assistRoleId = role.roleId;
      roleUserRelation.assistUserId = user.userId;

      db.SaveChanges();
    }
    #endregion
  }
}
