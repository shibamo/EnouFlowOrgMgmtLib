using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace EnouFlowOrgMgmtLib
{
  public static partial class OrgMgmtDBHelper
  {
    #region User related
    public static User createUser(EnouFlowOrgMgmtContext db)
    {
      return db.users.Create();
    }

    public static void saveCreatedUser(User user, EnouFlowOrgMgmtContext db)
    {
      db.users.Add(user);
      db.SaveChanges();
    }

    public static void setUserDepartment(int id, Department department,
      UserPositionToDepartment userPosition, EnouFlowOrgMgmtContext db)
    {
      var user = db.users.Find(id);

      #region check whether relation already setted, then re-set to valid and the userPosition
      if (user.getDepartmentsBelongTo(db, false)
        .Contains(department))
      {
        var r = user.departmentUserRelations.Where(x =>
          x.assistDepartmentId == department.departmentId).
          ToList().FirstOrDefault();
        r.isValid = true;
        r.userPosition = userPosition;
        db.SaveChanges();
        return;
      }
      #endregion

      saveDepartmentUserRelation(department, user, db, userPosition);
    }

    public static void unsetUserDepartment(int id, Department department,
      EnouFlowOrgMgmtContext db)
    {
      var user = db.users.Find(id);

      if (user.getDepartmentsBelongTo(db)
        .Contains(department))
      {
        var r = user.departmentUserRelations.Where(x =>
          x.assistDepartmentId == department.departmentId).
          ToList().FirstOrDefault();
        r.isValid = false;
        db.SaveChanges();
      }
    }

    public static void setUserRole(int id, Role role, EnouFlowOrgMgmtContext db)
    {
      var user = db.users.Find(id);

      #region check whether relation already setted, then re-set to valid
      if (user.getRolesBelongTo(db, false)
        .Contains(role))
      {
        var r = user.roleUserRelations.Where(x =>
          x.assistRoleId == role.roleId).
          ToList().FirstOrDefault();
        r.isValid = true;
        db.SaveChanges();
        return;
      }
      #endregion

      saveCreatedRoleUserRelation(role, user, db);
    }

    public static void unsetUserRole(int id, Role role,
      EnouFlowOrgMgmtContext db)
    {
      var user = db.users.Find(id);

      if (user.getRolesBelongTo(db)
        .Contains(role))
      {
        var r = user.roleUserRelations.Where(x =>
          x.assistRoleId == role.roleId).
          ToList().FirstOrDefault();
        r.isValid = false;
        db.SaveChanges();
      }
    }

    public static UserDTO getUserDTO(int id, EnouFlowOrgMgmtContext db)
    {
      return convertUser2DTO(db.users.Find(id), db);
    }

    public static UserDTO getUserDTO(string guid, EnouFlowOrgMgmtContext db)
    {
      return convertUser2DTO(getUser(guid, db), db);
    }

    public static UserDTO getUserDTO(int id)
    {
      using (EnouFlowOrgMgmtContext db = new EnouFlowOrgMgmtContext()) { 
        return getUserDTO(id,db);
      }
    }

    public static UserDTO getUserDTO(string guid)
    {
      using (EnouFlowOrgMgmtContext db = new EnouFlowOrgMgmtContext())
      {
        return getUserDTO(guid, db);
      }
    }

    public static User getUser(string guid, EnouFlowOrgMgmtContext db)
    {
      return db.users.Where(user => user.guid == guid).
        ToList().FirstOrDefault();
    }

    public static User logonUser(string logonUser, string logonPassword,
      EnouFlowOrgMgmtContext db)
    {
      var _user = db.users.Where(
        user => user.logonName.ToUpper() == logonUser.ToUpper())
        .ToList().FirstOrDefault();
      if (_user != null)
      {
        string salt = _user.logonSalt;
        byte[] passwordAndSaltBytes = Encoding.UTF8.GetBytes(
          logonPassword + salt);
        byte[] hashBytes = new SHA256Managed().ComputeHash(passwordAndSaltBytes);
        string hashString = Convert.ToBase64String(hashBytes);
        if (hashString == _user.logonPasswordHash)
        {
          return _user;
        }
      }

      return null;
    }

    public static List<UserDTO> getAllUserDTOs(
      EnouFlowOrgMgmtContext db, bool includeAssistSearchInfo = false)
    {
      return db.users.ToList().Select(user =>
        convertUser2DTO(user, db, includeAssistSearchInfo)).ToList();
    }

    public static UserDTO convertUser2DTO(User obj,
      EnouFlowOrgMgmtContext db,
      bool includeAssistSearchInfo = false)
    {
      return new UserDTO()
      {
        userId = obj.userId,
        guid = obj.guid,
        name = obj.name,
        englishName = obj.englishName,
        displayName = obj.displayName,
        code = obj.code,
        indexNumber = obj.indexNumber,
        email = obj.email,
        accountInNT = obj.accountInNT,
        logonName = obj.logonName,
        officeTel = obj.officeTel,
        isVisible = obj.isVisible,
        personalMobile = obj.personalMobile,
        createTime = obj.createTime,
        validTimeFrom = obj.validTimeFrom,
        validTimeTo = obj.validTimeFrom,
        departmentNames = includeAssistSearchInfo ?
          obj.getDepartmentsBelongTo(db).Select(
            department => department.name).ToList() :
          new List<string>(),
        roleNames = includeAssistSearchInfo ?
          obj.getRolesBelongTo(db).Select(
            role => role.name).ToList() :
          new List<string>(),
        defaultDepartmentId = obj.getDepartmentsBelongTo(db).
          FirstOrDefault()?.departmentId,
        defaultDepartmentGuid = obj.getDepartmentsBelongTo(db).
          FirstOrDefault()?.guid,
        defaultDepartmentName = obj.getDepartmentsBelongTo(db).
          FirstOrDefault()?.name
        //departments = obj.getDepartmentsBelongTo(db).Select(
        //  department => convertDepartment2DTO(department, db)).ToList(),
        //roles = obj.getRolesBelongTo(db).Select(
        //  role => convertRole2DTO(role)).ToList()
      };
    }

    public static bool isUserChangeAllowed(int id, User value,
      EnouFlowOrgMgmtContext db)
    {
      if (db.users.AsNoTracking().Where(
        obj => obj.userId == id).ToList().FirstOrDefault().guid != value.guid)
      {
        throw new GuidNotAllowedToChangeException("不能修改对象GUID!");
      }
      return true;
    }

    public static bool isUserExists(int id, EnouFlowOrgMgmtContext db)
    {
      return db.users.Count(e => e.userId == id) > 0;
    }

    public static Tuple<string, string> generatePasswordHashAndSalt(string logonPasswordOrigin)
    {
      string salt = Guid.NewGuid().ToString();
      byte[] passwordAndSaltBytes = Encoding.UTF8.GetBytes(
        logonPasswordOrigin + salt);
      byte[] hashBytes = new SHA256Managed().ComputeHash(passwordAndSaltBytes);
      string hashString = Convert.ToBase64String(hashBytes);

      return new Tuple<string, string>(hashString, salt);
    }



    #endregion
  }
}
