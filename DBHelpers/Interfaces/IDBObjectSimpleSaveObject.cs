using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnouFlowOrgMgmtLib
{
  public interface IDBObjectSimpleSaveObject<TObject>
  {
    void saveCreatedObject(TObject obj);

    void saveUpdatedObject(TObject obj);
  }
}
