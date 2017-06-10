using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;
using EnouFlowOrgMgmtLib.Tests.Attributes;
using NSubstitute;

namespace EnouFlowOrgMgmtLib.Tests.Integration
{
  [TestFixture]
  public class BizEntityHelperTestsI
  {
    private EnouFlowOrgMgmtContext db = null;

    [SetUp]
    public void Setup()
    {
      db = new EnouFlowOrgMgmtContext();
    }

    [Test]
    [Category("Integration")]
    [Rollback]
    public void saveCreatedObject_validObjWithNoParent_willExist()
    {
      BizEntityHelper o = new BizEntityHelper(db);
      BizEntity bizEntity = o.createObject();
      bizEntity.name = "Integration_Test_XXXYYYZZZ";
      OrgSchema orgSchema = db.orgSchemas.FirstOrDefault();

      o.saveCreatedObject(orgSchema, bizEntity,null);

      Assert.True(o.isObjectExists(bizEntity.bizEntityId));
      Assert.AreEqual(1, db.bizEntityRelationOnOrgSchemas.Where(
          ber => ber.assistOrgSchemaId == orgSchema.orgSchemaId &&
            ber.bizEntityIdChild == bizEntity.bizEntityId).Count());
    }

    [Test]
    [Category("Integration")]
    [Rollback]
    public void saveCreatedObject_validObjWithValidParent_willExist()
    {
      BizEntityHelper o = new BizEntityHelper(db);
      BizEntity bizEntity = o.createObject();
      bizEntity.name = "Integration_Test_XXXYYYZZZ";
      OrgSchema orgSchema = db.orgSchemas.FirstOrDefault();
      BizEntity bizParentEntity = orgSchema.
        bizEntityRelationOnOrgSchemas.FirstOrDefault().bizEntityChild;

      o.saveCreatedObject(orgSchema, bizEntity, bizParentEntity);

      Assert.True(o.isObjectExists(bizEntity.bizEntityId));
      Assert.AreEqual(1, db.bizEntityRelationOnOrgSchemas.Where(
          ber => ber.assistOrgSchemaId == orgSchema.orgSchemaId &&
            ber.bizEntityIdChild == bizEntity.bizEntityId &&
            ber.bizEntityIdParent == bizParentEntity.bizEntityId
            ).Count());
    }

    [Test]
    [Category("Integration")]
    [Rollback]
    public void saveCreatedObject_validObjWithInvalidParent_willThrows()
    {
      BizEntityHelper o = new BizEntityHelper(db);
      BizEntity bizEntity = o.createObject();
      bizEntity.name = "Integration_Test_XXXYYYZZZ";
      OrgSchema orgSchema = db.orgSchemas.FirstOrDefault();
      BizEntity bizParentEntity = o.createObject();

      Assert.Throws<DataLogicException>(
        ()=>o.saveCreatedObject(orgSchema, bizEntity, bizParentEntity));
    }

    [Test]
    [Category("Integration")]
    [Rollback]
    public void setParentBizEntity_validObjWithValidParent_willExist()
    {
      BizEntityHelper o = new BizEntityHelper(db);
      BizEntity bizEntity = o.createObject();
      bizEntity.name = "Integration_Test_XXXYYYZZZ";
      OrgSchema orgSchema = db.orgSchemas.FirstOrDefault();
      BizEntity bizEntityParent = o.createObject();
      bizEntityParent.name = "Integration_Test_Temp_Parent";
      o.saveCreatedObject(orgSchema, bizEntityParent, null);
      o.saveCreatedObject(orgSchema, bizEntity, null);

      o.setParentBizEntity(bizEntity.bizEntityId, bizEntityParent, orgSchema.orgSchemaId);

      Assert.AreEqual(1, db.bizEntityRelationOnOrgSchemas.Where(
          ber => ber.assistOrgSchemaId == orgSchema.orgSchemaId &&
            ber.bizEntityIdChild == bizEntity.bizEntityId &&
            ber.bizEntityIdParent == bizEntityParent.bizEntityId
            ).Count());
    }

    [Test]
    [Category("Integration")]
    [Rollback]
    public void removeObject_objWithChild_willThrows()
    {
      BizEntityHelper o = new BizEntityHelper(db);
      BizEntity bizEntity = o.createObject();
      bizEntity.name = "Integration_Test_XXXYYYZZZ";
      OrgSchema orgSchema = db.orgSchemas.FirstOrDefault();
      BizEntity bizParentEntity = orgSchema.
        bizEntityRelationOnOrgSchemas.FirstOrDefault().bizEntityChild;
      o.saveCreatedObject(orgSchema, bizEntity, bizParentEntity);

      Assert.Throws<DataLogicException>(
        () => o.removeObject(bizParentEntity.bizEntityId));
    }

    [Test]
    [Category("Integration")]
    [Rollback]
    public void removeObject_obj_willSetVibleToFalse()
    {
      BizEntityHelper o = new BizEntityHelper(db);
      BizEntity bizEntity = o.createObject();
      bizEntity.name = "Integration_Test_XXXYYYZZZ";
      OrgSchema orgSchema = db.orgSchemas.FirstOrDefault();
      BizEntity bizParentEntity = orgSchema.
        bizEntityRelationOnOrgSchemas.FirstOrDefault().bizEntityChild;
      o.saveCreatedObject(orgSchema, bizEntity, bizParentEntity);

      o.removeObject(bizEntity.bizEntityId);

      Assert.False(bizEntity.isVisible);
    }

    [Test]
    [Category("Integration")]
    [Rollback]
    public void convert2DTO_noBizEntitySchemaObj_willSetfirstbizEntitySchemaIdTo0()
    {
      BizEntityHelper o = new BizEntityHelper(db);
      BizEntity bizEntity = o.createObject();
      bizEntity.name = "Integration_Test_XXXYYYZZZ";
      OrgSchema orgSchema = db.orgSchemas.FirstOrDefault();
      BizEntity bizParentEntity = orgSchema.
        bizEntityRelationOnOrgSchemas.FirstOrDefault().bizEntityChild;
      o.saveCreatedObject(orgSchema, bizEntity, bizParentEntity);

      BizEntityDTO bizEntityDTO = o.convert2DTO(bizEntity);

      Assert.AreEqual(0, bizEntityDTO.firstbizEntitySchemaId);
      Assert.IsNull(bizEntityDTO.firstbizEntitySchemaName);
    }

    [Test]
    [Category("Integration")]
    [Rollback]
    public void convert2DTO_hasBizEntitySchemaObj_willSetfirstbizEntitySchemaIdNotTo0()
    {
      BizEntityHelper o = new BizEntityHelper(db);
      OrgSchema orgSchema = db.orgSchemas.FirstOrDefault();
      BizEntity bizEntity = orgSchema.
        bizEntityRelationOnOrgSchemas.FirstOrDefault().bizEntityChild;

      BizEntityDTO bizEntityDTO = o.convert2DTO(bizEntity);

      Assert.AreNotEqual(0, bizEntityDTO.firstbizEntitySchemaId);
      Assert.IsNotNull(bizEntityDTO.firstbizEntitySchemaName);
    }

    [TearDown]
    public void TearDown()
    {
      db.Dispose();
    }
  }
}
