using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Diagnostics.Contracts;

namespace EnouFlowOrgMgmtLib
{
  public class BizEntityHelper
    : BaseHelper<BizEntity, BizEntityDTO, EnouFlowOrgMgmtContext, BizEntityHelper>
  {
    public BizEntityHelper() { }

    public BizEntityHelper(EnouFlowOrgMgmtContext dbContext) : base(dbContext)
    {
    }

    public override BizEntityDTO convert2DTO(BizEntity obj)
    {
      Contract.Requires<DataLogicException>(obj != null,
        "BizEntity不能为空");

      var firstbizEntitySchema = obj.bizEntitySchemas.ToList().Where(
          bizEntitySchema => bizEntitySchema.isVisible).ToList().FirstOrDefault();
      return new BizEntityDTO()
      {
        bizEntityId = obj.bizEntityId,
        guid = obj.guid,
        name = obj.name,
        englishName = obj.englishName,
        shortName = obj.shortName,
        displayName = obj.displayName,
        code = obj.code,
        indexNumber = obj.indexNumber,
        createTime = obj.createTime,
        url = obj.url,
        DunsNumber = obj.DunsNumber,
        isVisible = obj.isVisible,
        firstbizEntitySchemaId = firstbizEntitySchema?.bizEntitySchemaId ?? 0,
        firstbizEntitySchemaName = firstbizEntitySchema?.name
      };
    }

    public override BizEntity createObject()
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      return _dbContext.bizEntities.Create();
    }

    public override BizEntity getObject(string id)
    {
      throw new NotImplementedException();
    }

    public override BizEntity getObject(int id)
    {
      throw new NotImplementedException();
    }

    public override bool isObjectChangeAllowed(int id, BizEntity value)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      if (_dbContext.bizEntities.AsNoTracking().Where(
        bizEntity => bizEntity.bizEntityId == id).ToList().FirstOrDefault().guid
        != value.guid)
      {
        throw new GuidNotAllowedToChangeException("不能修改对象GUID!");
      }

      return true;
    }

    public override bool isObjectExists(int id)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      return _dbContext.bizEntities.Count(o => o.bizEntityId == id) > 0;
    }

    public override bool removeObject(int id)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      var bizEntity = _dbContext.bizEntities.Find(id);

      if (bizEntity != null && isBizEntityRemovable(id))
      {
        bizEntity.isVisible = false;
      }

      _dbContext.SaveChanges();

      return true;
    }

    public override bool recoverObject(int id)
    {
      throw new NotImplementedException();
    }

    public void saveCreatedObject(OrgSchema orgSchema, BizEntity obj,
      BizEntity bizEntityParent, bool removeExistingRelation = false)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");
      Contract.Requires<DataLogicException>(obj != null, "BizEntity不能为空");
      Contract.Requires<DataLogicException>(obj != bizEntityParent, "BizEntity不能为自己的父业务实体");
      Contract.Requires<DataLogicException>(orgSchema != null, "OrgSchema不能为空");

      if (!_dbContext.bizEntities.ToList().Contains(obj)) //新创建尚未存入DB的BizEntity
      {
        _dbContext.bizEntities.Add(obj);
      }

      var hasExistingRelation = orgSchema.bizEntityRelationOnOrgSchemas.ToList()
        .Where(bro => bro.bizEntityIdChild == obj.bizEntityId).ToList()
        .Count() > 0;
      //验证BizEntity不能属于同一OrgSchema下的多于一个Parent BizEntity
      if (hasExistingRelation)
      {
        if (!removeExistingRelation)
        {
          throw new DataLogicException(
            string.Format("业务实体'{0}'在组织结构方案'{1}'内已有所属父实体.",
            obj.name, orgSchema.name));
        }
        else //Remove Existing Relation
        {
          orgSchema.bizEntityRelationOnOrgSchemas.Remove(
            orgSchema.bizEntityRelationOnOrgSchemas.Where(
              bro => bro.bizEntityIdChild == obj.bizEntityId)
                .FirstOrDefault());
        }
      }

      BizEntityRelationOnOrgSchema bizEntityRelationOnOrgSchema
        = _dbContext.bizEntityRelationOnOrgSchemas.Create();
      bizEntityRelationOnOrgSchema.assistOrgSchemaId = orgSchema.orgSchemaId;
      bizEntityRelationOnOrgSchema.bizEntityChild = obj;
      if (bizEntityParent != null)
      {
        //验证bizEntityParent必须已经在该OrgSchema中存在
        if (_dbContext.bizEntityRelationOnOrgSchemas.Where(
          o => o.assistOrgSchemaId == orgSchema.orgSchemaId
            && o.bizEntityIdChild == bizEntityParent.bizEntityId)
          .Count() <= 0)
        {
          throw new DataLogicException(
            string.Format("业务实体'{0}'在组织结构方案'{1}'内尚不存在，不能作为父业务实体.",
            bizEntityParent.name, orgSchema.name));
        }
        bizEntityRelationOnOrgSchema.bizEntityParent = bizEntityParent;
      }
      orgSchema.bizEntityRelationOnOrgSchemas.Add(bizEntityRelationOnOrgSchema);

      _dbContext.SaveChanges();
    }

    public void saveUpdatedObject(BizEntity obj)
    {
      throw new NotImplementedException();
    }

    public void setParentBizEntity(
      int id, BizEntity bizEntityParent, int orgSchemaId)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      var orgSchema = _dbContext.orgSchemas.Find(orgSchemaId);
      if (orgSchema == null)
      {
        throw new DataLogicException(
             string.Format("找不到orgSchemaId为'{0}'的组织结构方案.", orgSchemaId));
      }
      var bizEntity = _dbContext.bizEntities.Find(id);
      if (bizEntity == null)
      {
        throw new DataLogicException(
             string.Format("找不到bizEntityId为'{0}'的业务实体.", id));
      }

      #region check validity
      if (!isBizEntitySetParentAllowed(id, bizEntityParent, orgSchemaId))
      {
        return;
      }
      #endregion

      #region Delete current existing parent-child relation
      var currentbizEntityRelationOnOrgSchema = _dbContext.bizEntityRelationOnOrgSchemas
          .Where(r => r.assistOrgSchemaId == orgSchemaId &&
          r.bizEntityIdChild == id).ToList().FirstOrDefault();
      _dbContext.bizEntityRelationOnOrgSchemas.Remove(currentbizEntityRelationOnOrgSchema);
      #endregion

      #region construct new parent-child relation
      BizEntityRelationOnOrgSchema bizEntityRelationOnOrgSchema
        = _dbContext.bizEntityRelationOnOrgSchemas.Create();
      bizEntityRelationOnOrgSchema.assistOrgSchemaId = orgSchema.orgSchemaId;
      bizEntityRelationOnOrgSchema.bizEntityChild = bizEntity;
      if (bizEntityParent != null)
      {
        bizEntityRelationOnOrgSchema.bizEntityParent = bizEntityParent;
      }
      orgSchema.bizEntityRelationOnOrgSchemas.Add(bizEntityRelationOnOrgSchema);
      #endregion

      _dbContext.SaveChanges();
    }

    private bool isBizEntitySetParentAllowed(
      int id, BizEntity bizEntityParent, int orgSchemaId)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      var orgSchema = _dbContext.orgSchemas.Find(orgSchemaId);
      #region Cannot be the parent of self
      if (bizEntityParent != null)
      {
        if (id == bizEntityParent.bizEntityId)
        {
          throw new DataLogicException("Cannot be the parent of self!");
        }
        //验证bizEntityParent必须已经在该OrgSchema中存在
        if (_dbContext.bizEntityRelationOnOrgSchemas.Where(
          o => o.assistOrgSchemaId == orgSchemaId
            && o.bizEntityIdChild == bizEntityParent.bizEntityId)
          .Count() <= 0)
        {
          throw new DataLogicException(
            string.Format("业务实体'{0}'在组织结构方案'{1}'内尚不存在，不能作为父业务实体.",
            bizEntityParent.name, orgSchema.name));
        }
      }
      #endregion

      #region Parent不能为自己的子孙,判断方法为从parent开始逐级找祖先,判断bizEntityId是否为id
      BizEntity currentParent = bizEntityParent;
      while (currentParent != null)
      {
        if (currentParent.bizEntityId == id)
        {
          throw new DataLogicException("设置的祖先不能为自己的子孙节点!");
        }
        var currentbizEntityRelationOnOrgSchema = _dbContext.bizEntityRelationOnOrgSchemas
          .Where(r => r.assistOrgSchemaId == orgSchemaId &&
          r.bizEntityIdChild == currentParent.bizEntityId).ToList().FirstOrDefault();
        if (currentbizEntityRelationOnOrgSchema != null)
        {
          currentParent = currentbizEntityRelationOnOrgSchema.bizEntityParent;
        }
        else
        {
          currentParent = null;
        }
      }
      #endregion

      return true;
    }

    private bool isBizEntityRemovable(int id)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      var bizEntity = _dbContext.bizEntities.Find(id);
      #region 不能删除带有子节点的BizEntity
      if (_dbContext.bizEntityRelationOnOrgSchemas.ToList().Where(
        r => r.bizEntityParent == bizEntity &&
        r.bizEntityChild.isVisible).Count() > 0)
      {
        throw new DataLogicException(
          string.Format("不能删除带有子节点的业务实体BizEntity:{0}",bizEntity.name));
      }
      #endregion

      return true;
    }
  }
}
