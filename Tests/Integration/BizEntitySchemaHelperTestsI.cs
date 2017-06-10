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
  public class BizEntitySchemaHelperTestsI
  {
    private EnouFlowOrgMgmtContext db = null;

    [SetUp]
    public void Setup()
    {
      db = new EnouFlowOrgMgmtContext();
    }

    [TestCase(SchemeMode.simpleMode)]
    [TestCase(SchemeMode.multiDepartmentForOneUserMode)]
    [Category("Integration")]
    [Rollback]
    public void saveCreatedObject_duplicateObjUnderBizEntityWithNotValidModes_willThrows(SchemeMode schemeMode)
    {
      BizEntitySchemaHelper o = new BizEntitySchemaHelper(db);
      OrgSchema orgSchema = db.orgSchemas.FirstOrDefault();
      BizEntity bizParentEntity = orgSchema.
              bizEntityRelationOnOrgSchemas.FirstOrDefault().bizEntityChild;
      BizEntitySchema bizEntitySchema = new BizEntitySchema();
      bizEntitySchema.BizEntity = bizParentEntity;
      OrgMgmtDBHelper.schemeMode = schemeMode;

      Assert.Throws<DataLogicException>(
        () => o.saveCreatedObject(bizEntitySchema));
    }

    [TestCase(SchemeMode.simpleMode)]
    [TestCase(SchemeMode.multiDepartmentForOneUserMode)]
    [Category("Integration")]
    [Rollback]
    public void saveCreatedObject_validObj_willExists(SchemeMode schemeMode)
    {
      OrgMgmtDBHelper.schemeMode = schemeMode;
      BizEntityHelper o = new BizEntityHelper(db);
      BizEntity bizEntity = o.createObject();
      bizEntity.name = "Integration_Test_XXXYYYZZZ";
      OrgSchema orgSchema = db.orgSchemas.FirstOrDefault();
      BizEntity bizParentEntity = orgSchema.
        bizEntityRelationOnOrgSchemas.FirstOrDefault().bizEntityChild;
      o.saveCreatedObject(orgSchema, bizEntity, bizParentEntity);
      BizEntitySchemaHelper bizEntitySchemaHelper = new BizEntitySchemaHelper(db);
      BizEntitySchema bizEntitySchema = new BizEntitySchema();
      bizEntitySchema.BizEntity = bizEntity;

      bizEntitySchemaHelper.saveCreatedObject(bizEntitySchema);

      Assert.True(bizEntitySchemaHelper.isObjectExists(bizEntitySchema.bizEntitySchemaId));
    }


    [TearDown]
    public void TearDown()
    {
      db.Dispose();
    }
  }
}
