using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace EnouFlowOrgMgmtLib
{
  public interface IDBObjectHelperBase<TObject, TDTO, TDbContext, TRealType>
    where TDbContext : DbContext
  {
    TRealType setDbContext(TDbContext dbContext);

    TObject createObject();

    bool removeObject(int id);

    bool recoverObject(int id);

    TDTO convert2DTO(TObject obj);

    TObject getObject(int id);

    TObject getObject(string id);

    bool isObjectChangeAllowed(int id, TObject value);

    bool isObjectExists(int id);

  }
}
