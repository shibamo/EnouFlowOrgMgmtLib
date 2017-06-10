using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Diagnostics.Contracts;

namespace EnouFlowOrgMgmtLib
{
  public class OrgHelper 
    : BaseHelper<Org, OrgDTO, EnouFlowOrgMgmtContext, OrgHelper>, 
    IDBObjectSimpleSaveObject<Org>
  {
    public OrgHelper() { }

    public OrgHelper(EnouFlowOrgMgmtContext dbContext) : base(dbContext)
    {
    } 

    public override OrgDTO convert2DTO(Org obj)
    {
      Contract.Requires<DataLogicException>(obj != null, "Org对象不能为空");

      return new OrgDTO()
      {
        orgId = obj.orgId,
        guid = obj.guid,
        name = obj.name,
        shortName = obj.shortName,
        displayName = obj.displayName,
        code = obj.code,
        indexNumber = obj.indexNumber,
        url = obj.url,
        DunsNumber = obj.DunsNumber,
        createTime = obj.createTime,
        isDefault = obj.isDefault,
        isVisible = obj.isVisible,
        affintyLevel = obj.affintyLevel,
        firstOrgSchemaId = obj.orgSchema == null ? 0 : obj.orgSchema.orgSchemaId,
        firstOrgSchemaName = obj.orgSchema == null ? "无" : obj.orgSchema.name
      };
    }

    public override Org createObject()
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");
      var org = _dbContext.orgs.Create();
      if (_dbContext.orgs.Count() != 0) //不是第一个Org将被自动初始化设为非默认顶级机构
      {
        org.isDefault = false;
      }
      return org;
    }

    public override Org getObject(int id)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      return _dbContext.orgs.Find(id);
    }

    public override Org getObject(string id)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      return _dbContext.orgs.Where(org => org.guid == id).FirstOrDefault();
    }

    public override bool isObjectChangeAllowed(int id, Org value)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      if (_dbContext.orgs.AsNoTracking().
          Where(org => org.orgId == id).ToList().FirstOrDefault().guid
          != value.guid)
      {
        throw new GuidNotAllowedToChangeException("不能修改对象GUID!");
      }

      return true;
    }

    public override bool isObjectExists(int id)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      return _dbContext.orgs.Count(e => e.orgId == id) > 0;
    }

    public override bool removeObject(int id)
    {
      var org = _dbContext.orgs.Find(id);
      if (org == null)
      {
        throw new KeyNotFoundException(string.Format("id为{0}的Org在数据库中不存在", id));
      }

      org.isVisible = false;
      _dbContext.SaveChanges();

      return true;
    }

    public override bool recoverObject(int id)
    {
      var org = _dbContext.orgs.Find(id);
      if (org == null)
      {
        throw new KeyNotFoundException(string.Format("id为{0}的Org在数据库中不存在", id));
      }

      org.isVisible = true;
      _dbContext.SaveChanges();

      return true;
    }

    public void saveCreatedObject(Org obj)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      //顶级机构名字唯一性判断
      if (_dbContext.orgs.ToList().Exists(
        o => o.name == obj.name && o.orgId != obj.orgId))
      {
        throw new NameDuplicationException("不能创建同名的机构Org.");
      }

      //只能有一个默认顶级机构
      if (_dbContext.orgs.ToList().Exists(
        o => o.isDefault && o.orgId != obj.orgId))
      {
        obj.isDefault = false;
      }

      _dbContext.orgs.Add(obj);
      _dbContext.SaveChanges();
    }

    public void saveUpdatedObject(Org obj)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      _dbContext.Entry(obj).State = EntityState.Modified;
      _dbContext.SaveChanges();
    }

    public ICollection<OrgSchemaDTO> getOrgSchemaDTOs(Org obj)
    {
      Contract.Requires<DataLogicException>(obj != null, "Org不能为空");

      OrgSchemaHelper orgSchemaHelper = new OrgSchemaHelper();
      return obj.orgSchemas.ToList().Select(
        orgSchema => orgSchemaHelper.convert2DTO(orgSchema)).ToList();
    }
  }
}
