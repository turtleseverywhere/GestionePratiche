using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace x1_lavorazione_pratiche.DB.Interceptors
{
    public class DBCommandInterceptor : DbCommandInterceptor
    {
        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result)
        {
            ManipulateCommand(command);

            return result;
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            ManipulateCommand(command);

            return new ValueTask<InterceptionResult<DbDataReader>>(result);
        }

        private static void ManipulateCommand(DbCommand command)
        {
            System.Diagnostics.Debug.WriteLine("----------------------------------");
            System.Diagnostics.Debug.WriteLine(command.CommandText);
            if (command.Parameters.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine("------------PARAMETERS------------");
                foreach (DbParameter parameter in command.Parameters)
                    System.Diagnostics.Debug.WriteLine(parameter.ParameterName + " (" + parameter.DbType + "(" + parameter.Size + ")): " + parameter.Value);
            }
            System.Diagnostics.Debug.WriteLine("----------------------------------");
        }
    }
}