using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Diagnostics.Contracts;
using System.Security.Cryptography;

namespace EnouFlowOrgMgmtLib
{
  public class UserHelper :
    BaseHelper<User, UserDTO, EnouFlowOrgMgmtContext, UserHelper>,
    IDBObjectSimpleSaveObject<User>
  {
    public UserHelper() { }

    public UserHelper(EnouFlowOrgMgmtContext dbContext) : base(dbContext)
    {
    }

    public override UserDTO convert2DTO(User obj)
    {
      return convert2DTO(obj, false);
    }

    public UserDTO convert2DTO(User obj, bool includeAssistSearchInfo = false)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

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
          obj.getDepartmentsBelongTo(_dbContext).Select(
            department => department.name).ToList() :
          new List<string>(),
        roleNames = includeAssistSearchInfo ?
          obj.getRolesBelongTo(_dbContext).Select(
            role => role.name).ToList() :
          new List<string>(),
        defaultDepartmentId = obj.getDepartmentsBelongTo(_dbContext).
          FirstOrDefault()?.departmentId,
        defaultDepartmentGuid = obj.getDepartmentsBelongTo(_dbContext).
          FirstOrDefault()?.guid,
        defaultDepartmentName = obj.getDepartmentsBelongTo(_dbContext).
          FirstOrDefault()?.name
        // 去掉以下的引用上层对象，否则会形成循环引用
        //departments = obj.getDepartmentsBelongTo(db).Select(
        //  department => convertDepartment2DTO(department, db)).ToList(),
        //roles = obj.getRolesBelongTo(db).Select(
        //  role => convertRole2DTO(role)).ToList()
      };
    }

    public override User createObject()
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      return _dbContext.users.Create();
    }

    public override User getObject(string id)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      return _dbContext.users.Where(user => user.guid == id).
        ToList().FirstOrDefault();
    }

    public override User getObject(int id)
    {
      throw new NotImplementedException();
    }

    public override bool isObjectChangeAllowed(int id, User value)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");
      Contract.Requires<DataLogicException>(value != null, "User不能为空");

      if (_dbContext.users.AsNoTracking().Where(obj => obj.userId == id).
        ToList().FirstOrDefault().guid != value.guid)
      {
        throw new GuidNotAllowedToChangeException("不能修改对象GUID!");
      }
      return true;
    }

    public override bool isObjectExists(int id)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      return _dbContext.users.Count(e => e.userId == id) > 0;
    }

    public override bool recoverObject(int id)
    {
      throw new NotImplementedException();
    }

    public override bool removeObject(int id)
    {
      throw new NotImplementedException();
    }

    public UserDTO getUserDTO(int id)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      return convert2DTO(_dbContext.users.Find(id));
    }

    public UserDTO getUserDTO(string guid)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      return convert2DTO(getObject(guid));
    }

    public void saveCreatedObject(User obj)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      _dbContext.users.Add(obj);
      _dbContext.SaveChanges();
    }

    public void saveUpdatedObject(User obj)
    {
      throw new NotImplementedException();
    }

    public User logonUser(string logonUser, string logonPassword)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, 
        "DbContext不能为空");

      var _user = _dbContext.users.Where(
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
        else
        {
          return null;
        }
      }

      return null;
    }

    public static Tuple<string, string> generatePasswordHashAndSalt(
      string logonPasswordOrigin)
    {
      string salt = Guid.NewGuid().ToString();
      byte[] passwordAndSaltBytes = Encoding.UTF8.GetBytes(
        logonPasswordOrigin + salt);
      byte[] hashBytes = new SHA256Managed().ComputeHash(passwordAndSaltBytes);
      string hashString = Convert.ToBase64String(hashBytes);

      return new Tuple<string, string>(hashString, salt);
    }

    public List<UserDTO> getAllUserDTOs(bool includeAssistSearchInfo = false)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      return _dbContext.users.ToList().Select(user =>
        convert2DTO(user, includeAssistSearchInfo)).ToList();
    }

    public List<UserDTO> getUserDTOsOfRole(int id)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      return _dbContext.roles.Find(id).getUsersBelongTo(_dbContext).
        Select(u => convert2DTO(u)).ToList();
    }

    #region DepartmentUserRelation related
    public void setUserDepartment(int userId, Department department,
      UserPositionToDepartment userPosition)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");
      Contract.Requires<DataLogicException>(department != null, "Department不能为空");

      var user = _dbContext.users.Find(userId);
      if (user == null)
      {
        throw new DataLogicException(string.Format("不存在的用户：id={0}!", userId));
      }

      // check whether relation already setted, then just re-set to valid and the userPosition
      if (user.getDepartmentsBelongTo(_dbContext, false).Contains(department))
      {
        bool changed = false;
        var rs = user.departmentUserRelations.ToList();
        foreach (var r in rs)
        {
          if (r.assistDepartmentId == department.departmentId &&
            (!r.isValid || r.userPosition != userPosition)) // only save if need modification
          {
            r.isValid = true;
            r.userPosition = userPosition;
            changed = true;
          }
          else if(r.assistDepartmentId != department.departmentId && 
            OrgMgmtDBHelper.schemeMode == SchemeMode.simpleMode)
          {// 如果是简单模式，则将该用户与其他部门存在的关系删除
            r.isValid = false;
            changed = true;
          }
        }
        if (changed) _dbContext.SaveChanges();
      }
      else // otherwise create new relation object（如果DepartmentUserRelation不存在）
      {
        createDepartmentUserRelation(department, user, userPosition);
      }
    }

    public void unsetUserDepartment(int id, Department department)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");
      Contract.Requires<DataLogicException>(department != null, "Department不能为空");

      var user = _dbContext.users.Find(id);
      if (user == null)
      {
        throw new DataLogicException(string.Format("不存在的用户：id={0}!", id));
      }

      if (user.getDepartmentsBelongTo(_dbContext).Contains(department))
      {
        var r = user.departmentUserRelations.Where(x =>
          x.assistDepartmentId == department.departmentId).
          ToList().FirstOrDefault();
        r.isValid = false;
        //_dbContext.departmentUserRelations.Remove(r);
        _dbContext.SaveChanges();
      }
    }

    public void createDepartmentUserRelation(Department department, User user,
      UserPositionToDepartment position = UserPositionToDepartment.normal)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");
      Contract.Requires<DataLogicException>(department != null, "Department不能为空");
      Contract.Requires<DataLogicException>(user != null, "User不能为空");

      //不能重复建立部门-用户隶属关系(这里不限制用户可以同时属于多个部门)
      if (user.departmentUserRelations.ToList().Select(
        r => r.assistDepartmentId).ToList().Contains(
          department.departmentId))
      {
        throw new DataLogicException(
          string.Format("用户'{0}'曾经属于部门'{1}', 需调用setUserDepartment调整",
          user.name, department.name));
      }

      // simpleMode下一个用户不能同时属于多个部门
      if (OrgMgmtDBHelper.schemeMode == SchemeMode.simpleMode)
      {
        if (user.departmentUserRelations.Where(r=>r.isValid).ToList().Count() > 0)
        {
          throw new DataLogicException(
            string.Format(@"目前模式（simpleMode）下,一个用户不能同时属于多个部门.
可调用setUserDepartment强制取消其他部门的隶属关系"));
        }
      }

      var departmentUserRelation = _dbContext.departmentUserRelations.Create();
      user.departmentUserRelations.Add(departmentUserRelation);
      department.departmentUserRelations.Add(departmentUserRelation);
      departmentUserRelation.assistDepartmentId = department.departmentId;
      departmentUserRelation.assistUserId = user.userId;
      departmentUserRelation.userPosition = position;

      _dbContext.SaveChanges();
    }

    public Department getUserDefaultDepartment(int userId)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      var departmentUserRelation = _dbContext.departmentUserRelations.Where(
        obj => obj.assistUserId == userId && obj.isValid).FirstOrDefault();
      if (departmentUserRelation != null)
      {
        return _dbContext.departments.Find(
          departmentUserRelation.assistDepartmentId);
      }
      else
      {
        return null;
      }
    }

    #endregion

    #region RoleUserRelation related
    public void setUserRole(int userId, Role role)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");
      Contract.Requires<DataLogicException>(role != null, "Role不能为空");

      var user = _dbContext.users.Find(userId);
      if (user == null)
      {
        throw new DataLogicException(string.Format("不存在的用户：id={0}!", userId));
      }

      #region check whether relation already setted, then re-set to valid
      if (user.getRolesBelongTo(_dbContext, false).Contains(role))
      {
        var r = user.roleUserRelations.Where(x =>
          x.assistRoleId == role.roleId).
          ToList().FirstOrDefault();
        r.isValid = true;
        _dbContext.SaveChanges();
        return;
      }
      #endregion

      createRoleUserRelation(role, user);
    }

    public void unsetUserRole(int userId, Role role)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");
      Contract.Requires<DataLogicException>(role != null, "Role不能为空");

      var user = _dbContext.users.Find(userId);
      if (user == null)
      {
        throw new DataLogicException(string.Format("不存在的用户：id={0}!", userId));
      }

      if (user.getRolesBelongTo(_dbContext)
        .Contains(role))
      {
        var r = user.roleUserRelations.Where(x =>
          x.assistRoleId == role.roleId).
          ToList().FirstOrDefault();
        r.isValid = false;
        _dbContext.SaveChanges();
      }
    }

    public void createRoleUserRelation(Role role, User user)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");
      Contract.Requires<DataLogicException>(role != null, "Role不能为空");
      Contract.Requires<DataLogicException>(user != null, "User不能为空");

      //不能重复建立角色-用户隶属关系(但不限制用户可以同时属于多个角色)
      if (user.roleUserRelations.ToList().Select(
        r => r.assistRoleId).ToList().Contains(role.roleId))
      {
        throw new DataLogicException(
          string.Format("用户'{0}'已属于角色'{1}', 需调用setUserRole调整", 
            user.name, role.name));
      }

      var roleUserRelation = _dbContext.roleUserRelations.Create();

      user.roleUserRelations.Add(roleUserRelation);
      role.roleUserRelations.Add(roleUserRelation);
      roleUserRelation.assistRoleId = role.roleId;
      roleUserRelation.assistUserId = user.userId;

      _dbContext.SaveChanges();
    }
    #endregion
  }
}
