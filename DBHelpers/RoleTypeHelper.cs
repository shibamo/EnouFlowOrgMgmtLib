using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Diagnostics.Contracts;

namespace EnouFlowOrgMgmtLib
{
  public class RoleTypeHelper :
    BaseHelper<RoleType, RoleTypeDTO, EnouFlowOrgMgmtContext, RoleTypeHelper>,
    IDBObjectSimpleSaveObject<RoleType>
  {
    public RoleTypeHelper() { }

    public RoleTypeHelper(EnouFlowOrgMgmtContext dbContext) : base(dbContext)
    {
    }

    public override RoleTypeDTO convert2DTO(RoleType obj)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      RoleHelper roleHelper = new RoleHelper(_dbContext);
      return new RoleTypeDTO()
      {
        roleTypeId = obj.roleTypeId,
        guid = obj.guid,
        name = obj.name,
        isVisible = obj.isVisible,
        createTime = obj.createTime,
        roles = obj.getRolesBelongTo(_dbContext).Select
          (role => roleHelper.convert2DTO(role)).ToList()
      };
    }

    public override RoleType createObject()
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      return _dbContext.roleTypes.Create();
    }

    public override RoleType getObject(string id)
    {
      throw new NotImplementedException();
    }

    public override RoleType getObject(int id)
    {
      throw new NotImplementedException();
    }

    public override bool isObjectChangeAllowed(int id, RoleType value)
    {
      Contract.Requires<DataLogicException>(_dbContext != null,
        "DbContext不能为空");

      if (_dbContext.roleTypes.AsNoTracking().Where(
        obj => obj.roleTypeId == id).ToList().FirstOrDefault().guid != value.guid)
      {
        throw new GuidNotAllowedToChangeException("不能修改对象GUID!");
      }
      return true;
    }

    public override bool isObjectExists(int id)
    {
      Contract.Requires<DataLogicException>(_dbContext != null,
        "DbContext不能为空");

      return _dbContext.roleTypes.Count(e => e.roleTypeId == id) > 0;
    }

    public override bool recoverObject(int id)
    {
      throw new NotImplementedException();
    }

    public override bool removeObject(int id)
    {
      throw new NotImplementedException();
    }

    public void saveCreatedObject(RoleType obj)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      _dbContext.roleTypes.Add(obj);
      _dbContext.SaveChanges();
    }

    public void saveUpdatedObject(RoleType obj)
    {
      throw new NotImplementedException();
    }
  }
}
