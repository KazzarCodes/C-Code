using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace iSync.Api
{
    public interface IDynamicRepository
    {
        void Add(EntityBase entity);

        void Attach(EntityBase entity);

        void Delete(EntityBase entity);

        /// <summary>
        /// Executes a stored procedure.
        /// </summary>
        DataSet ExecuteStoredProcedure(string procedureName, params DbParameter[] parameters);

        /// <summary>
        /// Executes a stored procedure with a specified timeout period.
        /// </summary>
        DataSet ExecuteStoredProcedureWithTimeout(string procedureName, int timeout, params DbParameter[] parameters);

        IEnumerable<SqlParameter> GetStoredProcedureParameters(string procedureName);

        void Persist();
    }
}
