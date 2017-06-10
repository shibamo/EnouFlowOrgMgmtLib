using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics.Contracts;

namespace EnouFlowOrgMgmtLib
{
  public class OrgSchemaHelper 
    : BaseHelper<OrgSchema, OrgSchemaDTO, EnouFlowOrgMgmtContext, OrgSchemaHelper>,
    IDBObjectSimpleSaveObject<OrgSchema>
  {
    public OrgSchemaHelper() { }

    public OrgSchemaHelper(EnouFlowOrgMgmtContext dbContext) : base(dbContext)
    {
    }

    public override OrgSchemaDTO convert2DTO(OrgSchema obj)
    {
      Contract.Requires<DataLogicException>(obj != null, "OrgSchema对象不能为空");

      BizEntityHelper bizEntityHelper = new BizEntityHelper();
      return new OrgSchemaDTO()
      {
        orgSchemaId = obj.orgSchemaId,
        guid = obj.guid,
        name = obj.name,
        shortName = obj.shortName,
        displayName = obj.displayName,
        code = obj.code,
        indexNumber = obj.indexNumber,
        createTime = obj.createTime,
        isDefault = obj.isDefault,
        isVisible = obj.isVisible,
        rootBizEntities = obj.rootBizEntities.ToList().Select(
          bizEntity => bizEntityHelper.convert2DTO(bizEntity)).ToList()
      };
    }

    public override OrgSchema createObject()
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      return _dbContext.orgSchemas.Create();
    }

    public override OrgSchema getObject(string id)
    {
      throw new NotImplementedException();
    }

    public override OrgSchema getObject(int id)
    {
      throw new NotImplementedException();
    }

    public override bool isObjectChangeAllowed(int id, OrgSchema value)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      if (_dbContext.orgSchemas.AsNoTracking().
        Where(orgSchema => orgSchema.orgSchemaId == id).ToList().FirstOrDefault().
        guid != value.guid)
      {
        throw new GuidNotAllowedToChangeException("不能修改对象GUID!");
      }

      return true;
    }

    public override bool isObjectExists(int id)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      return _dbContext.orgSchemas.Count(e => e.orgSchemaId == id) > 0;
    }

    public override bool removeObject(int id)
    {
      throw new NotImplementedException();
    }

    public override bool recoverObject(int id)
    {
      throw new NotImplementedException();
    }

    public void saveCreatedObject(OrgSchema obj)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");
      Contract.Requires<DataLogicException>(obj.Org != null, "OrgSchema的父级Org不能为空");

      //简单模式下一个Org下只有一个OrgSchema
      var org = obj.Org;
      if (OrgMgmtDBHelper.schemeMode != SchemeMode.multliOrgSchemaMode &&
        OrgMgmtDBHelper.schemeMode != SchemeMode.multiBizEntitySchemaMode)
      {
        if (org.orgSchemas.ToList().Where(os => os.isVisible).Count() > 0)
        {
          throw new DataLogicException("目前的组织架构模式下一个Org下只有一个OrgSchema.");
        }
      }

      //一个Org下只能有一个默认OrgSchema
      if (org.orgSchemas.ToList().Exists(
        o => o.isDefault && o.isVisible &&
        o.orgSchemaId != obj.orgSchemaId))
      {
        obj.isDefault = false;
      }

      org.orgSchemas.Add(obj);
      _dbContext.SaveChanges();
    }

    public void saveUpdatedObject(OrgSchema obj)
    {
      throw new NotImplementedException();
    }
  }
}
