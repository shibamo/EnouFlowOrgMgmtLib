using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Diagnostics.Contracts;

namespace EnouFlowOrgMgmtLib
{
  public class BizEntitySchemaHelper
    : BaseHelper<BizEntitySchema, BizEntitySchemaDTO, 
                  EnouFlowOrgMgmtContext, BizEntitySchemaHelper>,
    IDBObjectSimpleSaveObject<BizEntitySchema>
  {
    public BizEntitySchemaHelper() { }

    public BizEntitySchemaHelper(EnouFlowOrgMgmtContext dbContext) : base(dbContext)
    {
    }

    public override BizEntitySchemaDTO convert2DTO(BizEntitySchema obj)
    {
      Contract.Requires<DataLogicException>(_dbContext != null,
        "DbContext不能为空");
      Contract.Requires<DataLogicException>(obj != null, 
        "BizEntitySchema不能为空");

      DepartmentHelper departmentHelper = new DepartmentHelper(_dbContext);
      return new BizEntitySchemaDTO()
      {
        bizEntitySchemaId = obj.bizEntitySchemaId,
        guid = obj.guid,
        name = obj.name,
        shortName = obj.shortName,
        displayName = obj.displayName,
        code = obj.code,
        indexNumber = obj.indexNumber,
        createTime = obj.createTime,
        isVisible = obj.isVisible,
        rootDepartments = obj.getRootDepartments(_dbContext).
          Select(d => departmentHelper.convert2DTO(d)).ToList()
      };
    }

    public override BizEntitySchema createObject()
    {
      Contract.Requires<DataLogicException>(_dbContext != null, 
        "DbContext不能为空");

      return _dbContext.bizEntitySchemas.Create();
    }

    public override BizEntitySchema getObject(string id)
    {
      throw new NotImplementedException();
    }

    public override BizEntitySchema getObject(int id)
    {
      throw new NotImplementedException();
    }

    public override bool isObjectChangeAllowed(int id, BizEntitySchema value)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, 
        "DbContext不能为空");

      if (_dbContext.bizEntitySchemas.AsNoTracking().Where(
        obj => obj.bizEntitySchemaId == id).ToList().FirstOrDefault().guid
        != value.guid)
      {
        throw new GuidNotAllowedToChangeException("不能修改对象GUID!");
      }

      return true;
    }

    public override bool isObjectExists(int id)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, 
        "DbContext不能为空");

      return _dbContext.bizEntitySchemas.Count(
        e => e.bizEntitySchemaId == id) > 0;
    }

    public override bool recoverObject(int id)
    {
      throw new NotImplementedException();
    }

    public override bool removeObject(int id)
    {
      throw new NotImplementedException();
    }

    public void saveCreatedObject(BizEntitySchema obj)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, 
        "DbContext不能为空");
      Contract.Requires<DataLogicException>(obj.BizEntity != null, 
        "BizEntitySchema的父级BizEntity不能为空");

      BizEntity bizEntity = obj.BizEntity;
      // 只有multiBizEntitySchemaMode和multliOrgSchemaMode模式下,
      // BizEntity有多个BizEntitySchema
      if (OrgMgmtDBHelper.schemeMode == SchemeMode.simpleMode ||
        OrgMgmtDBHelper.schemeMode == SchemeMode.multiDepartmentForOneUserMode)
      {
        if (bizEntity.bizEntitySchemas.ToList().Count() > 0)
        {
          throw new DataLogicException(
            "目前模式下一个BizEntity下只有一个BizEntitySchema.");
        }
      }

      //一个BizEntity下只能有一个默认BizEntitySchema
      if (bizEntity.bizEntitySchemas.ToList().Exists(
        bs => bs.isDefault && 
        bs.bizEntitySchemaId != obj.bizEntitySchemaId))
      {
        obj.isDefault = false;
      }

      bizEntity.bizEntitySchemas.Add(obj);
      _dbContext.SaveChanges();
    }

    public void saveUpdatedObject(BizEntitySchema obj)
    {
      throw new NotImplementedException();
    }
  }
}
