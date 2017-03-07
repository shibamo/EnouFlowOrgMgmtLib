using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnouFlowOrgMgmtLib
{
  public static partial class OrgMgmtDBHelper
  {
    #region Org related
    public static Org createOrg(EnouFlowOrgMgmtContext db)
    {
      return db.orgs.Create();
    }

    public static void saveCreatedOrg(Org org, EnouFlowOrgMgmtContext db)
    {
      //顶级机构名字唯一性判断
      if (db.orgs.ToList().Exists(
        o => o.name == org.name && o.orgId != org.orgId))
      {
        throw new NameDuplicationException("不能创建同名的顶级机构.");
      }

      //只能有一个默认顶级机构
      if (db.orgs.ToList().Exists(
        o => o.isDefault && o.orgId != org.orgId))
      {
        org.isDefault = false;
      }

      db.orgs.Add(org);
      db.SaveChanges();
    }

    public static OrgDTO convertOrg2DTO(Org obj)
    {
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

    public static bool isOrgChangeAllowed(int id, Org value, EnouFlowOrgMgmtContext db)
    {
      if (db.orgs.AsNoTracking().Where(org => org.orgId == id).ToList().FirstOrDefault().guid != value.guid)
      {
        throw new GuidNotAllowedToChangeException("不能修改对象GUID!");
      }

      return true;
    }

    public static bool isOrgExists(int id, EnouFlowOrgMgmtContext db)
    {
      return db.orgs.Count(e => e.orgId == id) > 0;
    }

    public static ICollection<OrgSchemaDTO> getOrgSchemaDTOs(Org obj)
    {
      return obj.orgSchemas.ToList().Select(
        orgSchema => convertOrgSchema2DTO(orgSchema)).ToList();
    }

    #endregion

    #region OrgSchema related
    public static OrgSchema createOrgSchema(EnouFlowOrgMgmtContext db)
    {
      return db.orgSchemas.Create();
    }



    public static void saveCreatedOrgSchema(Org org, OrgSchema orgSchema, EnouFlowOrgMgmtContext db)
    {
      //简单模式下一个Org下只有一个OrgSchema
      if (schemeMode != SchemeMode.multliOrgSchemaMode)
      {
        if (org.orgSchemas.ToList().Where(os => os.isVisible).Count() > 0)
        {
          throw new Exception("目前的组织架构模式下一个Org下只有一个OrgSchema.");
        }
      }

      //一个Org下只能有一个默认OrgSchema
      if (org.orgSchemas.ToList().Exists(
        os => os.isDefault && os.isVisible &&
        os.orgSchemaId != orgSchema.orgSchemaId))
      {
        orgSchema.isDefault = false;
      }

      org.orgSchemas.Add(orgSchema);
      db.SaveChanges();
    }



    public static OrgSchemaDTO convertOrgSchema2DTO(OrgSchema obj)
    {
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
          bizEntity => convertBizEntity2DTO(bizEntity)).ToList()
      };
    }



    public static bool isOrgSchemaExists(int id, EnouFlowOrgMgmtContext db)
    {
      return db.orgSchemas.Count(e => e.orgSchemaId == id) > 0;
    }

    public static bool isOrgSchemaChangeAllowed(int id, OrgSchema value, EnouFlowOrgMgmtContext db)
    {
      if (db.orgSchemas.AsNoTracking().Where(
        orgSchema => orgSchema.orgSchemaId == id).ToList().FirstOrDefault().guid
        != value.guid)
      {
        throw new GuidNotAllowedToChangeException("不能修改对象GUID!");
      }

      return true;
    }
    #endregion
  }
}
