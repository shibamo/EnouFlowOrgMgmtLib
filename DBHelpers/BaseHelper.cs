using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Diagnostics.Contracts;

namespace EnouFlowOrgMgmtLib
{
  public abstract class BaseHelper<TObject, TDTO, TDbContext, TRealType> 
    : IDBObjectHelperBase<TObject, TDTO, TDbContext, TRealType>
    where TDbContext : DbContext
    where TRealType : class
  {
    public BaseHelper()
    {
    }
    public BaseHelper(TDbContext dbContext)
    {
      //Contract.Requires不能用于构造函数中
      if (dbContext == null) throw new DataLogicException("DbContext不能为空"); 

      _dbContext = dbContext;
    }
    internal TDbContext _dbContext { get; set; }

    public abstract TDTO convert2DTO(TObject obj);

    public abstract TObject createObject();

    public abstract TObject getObject(int id);

    public abstract TObject getObject(string id);

    public virtual TDTO getDTOObject(int id)
    {
      Contract.Requires<DataLogicException>(_dbContext != null, "DbContext不能为空");

      return convert2DTO(getObject(id));
    }

    public abstract bool isObjectChangeAllowed(int id, TObject value);

    public abstract bool isObjectExists(int id);

    public abstract bool removeObject(int id);

    public abstract bool recoverObject(int id);
    
    public virtual TRealType setDbContext(TDbContext dbContext)
    {
      Contract.Requires<DataLogicException>(dbContext != null, "DbContext不能为空");

      _dbContext = dbContext;

      return this as TRealType;
    }
  }
}
