using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnouFlowOrgMgmtLib
{
  public static partial class OrgMgmtDBHelper
  {
    #region BizEntity related
    public static BizEntity createBizEntity(EnouFlowOrgMgmtContext db)
    {
      return db.bizEntities.Create();
    }

    public static void saveCreatedBizEntity(OrgSchema orgSchema,
      BizEntity bizEntity, BizEntity bizEntityParent,
      EnouFlowOrgMgmtContext db, bool removeExistingRelation = false)
    {
      if (!db.bizEntities.ToList().Contains(bizEntity)) //新创建尚未存入DB的BizEntity
      {
        db.bizEntities.Add(bizEntity);
      }

      var hasExistingRelation = orgSchema.bizEntityRelationOnOrgSchemas.ToList().Where(
          bro => bro.bizEntityIdChild == bizEntity.bizEntityId
      ).ToList().Count() > 0;
      //验证不能属于同一OrgSchema下的多于一个Parent BizEntity
      if (hasExistingRelation)
      {
        if (!removeExistingRelation)
        {
          throw new Exception(
            string.Format("业务实体'{0}'在组织结构方案'{1}'内已有所属父实体.",
            bizEntity.name, orgSchema.name));
        }
        else //Remove Existing Relation
        {
          orgSchema.bizEntityRelationOnOrgSchemas.Remove(
            orgSchema.bizEntityRelationOnOrgSchemas.Where(
              bro => bro.bizEntityIdChild == bizEntity.bizEntityId)
                .FirstOrDefault());
        }
      }

      BizEntityRelationOnOrgSchema bizEntityRelationOnOrgSchema
        = db.bizEntityRelationOnOrgSchemas.Create();
      bizEntityRelationOnOrgSchema.assistOrgSchemaId = orgSchema.orgSchemaId;
      bizEntityRelationOnOrgSchema.bizEntityChild = bizEntity;
      if (bizEntityParent != null)
      {
        bizEntityRelationOnOrgSchema.bizEntityParent = bizEntityParent;
      }
      orgSchema.bizEntityRelationOnOrgSchemas.Add(bizEntityRelationOnOrgSchema);

      db.SaveChanges();
    }

    public static void setParentBizEntity(int id, BizEntity bizEntityParent,
      int orgSchemaId, EnouFlowOrgMgmtContext db)
    {
      var orgSchema = db.orgSchemas.Find(orgSchemaId);
      var bizEntity = db.bizEntities.Find(id);

      #region check validity
      if (!isBizEntitySetParentAllowed(id, bizEntityParent, orgSchemaId, db))
      {
        return;
      }
      #endregion

      #region Delete current existing parent-child relation
      var currentbizEntityRelationOnOrgSchema = db.bizEntityRelationOnOrgSchemas
          .Where(r => r.assistOrgSchemaId == orgSchemaId &&
          r.bizEntityIdChild == id).ToList().FirstOrDefault();
      //if (currentbizEntityRelationOnOrgSchema != null)
      //{
      //  db.bizEntityRelationOnOrgSchemas.Remove(currentbizEntityRelationOnOrgSchema);
      //}
      db.bizEntityRelationOnOrgSchemas.Remove(currentbizEntityRelationOnOrgSchema);
      #endregion

      #region construct new parent-child relation
      BizEntityRelationOnOrgSchema bizEntityRelationOnOrgSchema
        = db.bizEntityRelationOnOrgSchemas.Create();
      bizEntityRelationOnOrgSchema.assistOrgSchemaId = orgSchema.orgSchemaId;
      bizEntityRelationOnOrgSchema.bizEntityChild = bizEntity;
      if (bizEntityParent != null)
      {
        bizEntityRelationOnOrgSchema.bizEntityParent = bizEntityParent;
      }
      orgSchema.bizEntityRelationOnOrgSchemas.Add(bizEntityRelationOnOrgSchema);
      #endregion

      db.SaveChanges();
    }

    public static bool removeBizEntity(int id, EnouFlowOrgMgmtContext db)
    {
      var bizEntity = db.bizEntities.Find(id);

      if (bizEntity != null && isBizEntityRemovable(id, db))
      {
        bizEntity.isVisible = false;
      }

      db.SaveChanges();

      return true;
    }

    public static bool isBizEntityExists(int id, EnouFlowOrgMgmtContext db)
    {
      return db.bizEntities.Count(e => e.bizEntityId == id) > 0;
    }

    public static bool isBizEntityChangeAllowed(int id, BizEntity value, EnouFlowOrgMgmtContext db)
    {
      if (db.bizEntities.AsNoTracking().Where(
        bizEntity => bizEntity.bizEntityId == id).ToList().FirstOrDefault().guid
        != value.guid)
      {
        throw new GuidNotAllowedToChangeException("不能修改对象GUID!");
      }

      return true;
    }

    private static bool isBizEntitySetParentAllowed(int id, BizEntity parent,
      int orgSchemaId, EnouFlowOrgMgmtContext db)
    {
      #region Cannot be the parent of self
      if (parent != null)
      {
        if (id == parent.bizEntityId)
        {
          throw new DataLogicException("Cannot be the parent of self!");
        }
      }
      #endregion

      #region Parent不能为自己的子孙,判断方法为从parent开始逐级找祖先,判断bizEntityId是否为id
      BizEntity currentParent = parent;
      while (currentParent != null)
      {
        if (currentParent.bizEntityId == id)
        {
          throw new DataLogicException("设置的祖先不能为自己的子孙节点!");
        }
        var currentbizEntityRelationOnOrgSchema = db.bizEntityRelationOnOrgSchemas
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

    public static bool isBizEntityRemovable(int id, EnouFlowOrgMgmtContext db)
    {
      var bizEntity = db.bizEntities.Find(id);
      #region 不能删除带有子节点的BizEntity
      if (db.bizEntityRelationOnOrgSchemas.ToList().Where(
        r => r.bizEntityParent == bizEntity &&
        r.bizEntityChild.isVisible).Count() > 0)
      {
        throw new DataLogicException("不能删除带有子节点的BizEntity");
      }
      #endregion

      return true;
    }

    public static BizEntityDTO convertBizEntity2DTO(BizEntity obj)
    {
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
        firstbizEntitySchemaId = (firstbizEntitySchema != null ?
          firstbizEntitySchema.bizEntitySchemaId : 0),
        firstbizEntitySchemaName = (firstbizEntitySchema != null ?
          firstbizEntitySchema.name : null)
      };
    }

    #endregion

    #region BizEntitySchema related
    public static BizEntitySchema createBizEntitySchema(EnouFlowOrgMgmtContext db)
    {
      return db.bizEntitySchemas.Create();
    }

    public static void saveCreatedBizEntitySchema(BizEntitySchema bizEntitySchema,
      BizEntity bizEntity, EnouFlowOrgMgmtContext db)
    {
      // 只有multiBizEntitySchemaMode和multliOrgSchemaMode模式下,
      // BizEntity有多个BizEntitySchema
      if (schemeMode == SchemeMode.simpleMode ||
        schemeMode == SchemeMode.multiDepartmentForOneUserMode)
      {
        if (bizEntity.bizEntitySchemas.ToList().Count() > 0)
        {
          throw new Exception("目前模式下一个BizEntity下只有一个BizEntitySchema.");
        }
      }

      //一个BizEntity下只能有一个默认BizEntitySchema
      if (bizEntity.bizEntitySchemas.ToList().Exists(
        bs => bs.isDefault && bs.bizEntitySchemaId != bizEntitySchema.bizEntitySchemaId))
      {
        bizEntitySchema.isDefault = false;
      }

      bizEntity.bizEntitySchemas.Add(bizEntitySchema);
      db.SaveChanges();
    }

    public static bool isBizEntitySchemaExists(int id, EnouFlowOrgMgmtContext db)
    {
      return db.bizEntitySchemas.Count(e => e.bizEntitySchemaId == id) > 0;
    }

    public static bool isBizEntitySchemaChangeAllowed(int id, BizEntitySchema value,
      EnouFlowOrgMgmtContext db)
    {
      if (db.bizEntitySchemas.AsNoTracking().Where(
        obj => obj.bizEntitySchemaId == id).ToList().FirstOrDefault().guid
        != value.guid)
      {
        throw new GuidNotAllowedToChangeException("不能修改对象GUID!");
      }

      return true;
    }

    public static BizEntitySchemaDTO convertBizEntitySchema2DTO(BizEntitySchema obj,
      EnouFlowOrgMgmtContext db)
    {
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
        rootDepartments = obj.getRootDepartments(db).
          Select(d => convertDepartment2DTO(d, db)).ToList()
      };
    }
    #endregion
  }
}
