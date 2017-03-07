using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnouFlowOrgMgmtLib
{
  public static partial class OrgMgmtDBHelper
  {
    #region RoleType related
    public static RoleType createRoleType(EnouFlowOrgMgmtContext db)
    {
      return db.roleTypes.Create();
    }

    public static void saveCreatedRoleType(RoleType roleType, EnouFlowOrgMgmtContext db)
    {
      db.roleTypes.Add(roleType);
      db.SaveChanges();
    }

    public static RoleTypeDTO convertRoleType2DTO(RoleType obj, EnouFlowOrgMgmtContext db)
    {
      return new RoleTypeDTO()
      {
        roleTypeId = obj.roleTypeId,
        guid = obj.guid,
        name = obj.name,
        isVisible = obj.isVisible,
        createTime = obj.createTime,
        roles = obj.getRolesBelongTo(db).Select(
          role => convertRole2DTO(role,db)).ToList()
      };
    }

    public static bool isRoleTypeExists(int id, EnouFlowOrgMgmtContext db)
    {
      return db.roleTypes.Count(e => e.roleTypeId == id) > 0;
    }

    public static bool isRoleTypeChangeAllowed(int id, RoleType value,
      EnouFlowOrgMgmtContext db)
    {
      if (db.roleTypes.AsNoTracking().Where(
        obj => obj.roleTypeId == id).ToList().FirstOrDefault().guid != value.guid)
      {
        throw new GuidNotAllowedToChangeException("不能修改对象GUID!");
      }
      return true;
    }
    #endregion

    #region Role_RoleTypeRelation related
    public static void saveCreatedRole_RoleTypeRelation(Role role, RoleType roleType, EnouFlowOrgMgmtContext db)
    {
      //不能重复建立角色类型-角色隶属关系(但不限制角色可以同时属于多个角色类型)
      if (role.role_RoleTypeRelations.ToList().Select(
        r => r.assistRoleTypeId).ToList().Contains(roleType.roleTypeId))
      {
        return;

        throw new Exception(
          string.Format("角色'{0}'已属于角色类型'{1}'", role.name, roleType.name));
      }

      var role_RoleTypeRelation = db.role_RoleTypeRelations.Create();
      role.role_RoleTypeRelations.Add(role_RoleTypeRelation);
      roleType.role_RoleTypeRelations.Add(role_RoleTypeRelation);
      role_RoleTypeRelation.assistRoleId = role.roleId;
      role_RoleTypeRelation.assistRoleTypeId = roleType.roleTypeId;

      db.SaveChanges();
    }
    #endregion
  }
}
