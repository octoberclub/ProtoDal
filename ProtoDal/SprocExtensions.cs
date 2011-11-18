using System;
using System.Data.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ProtoDal
{
	public static class SprocExtensions
	{
		public static void AddInParameter(this DbCommand command, string name, string value)
		{
			var parameter = command.CreateParameter();
			parameter.Direction = ParameterDirection.Input;
			parameter.DbType = DbType.String;
			parameter.Value = value;
		}
		
		public static IEnumerable<TRow> GetRows<TRow>(
			this IConnectionProvider provider, 
			Action<IDbCommand> prepareCommand, 
			Func<IDataReader, TRow> rowFilter)
		{
			using (var connection = provider.GetOpenConnection())
			{
				using (var command = connection.CreateCommand())
				{
					prepareCommand(command);
			
					using (var reader = command.ExecuteReader(CommandBehavior.SequentialAccess))
					{
						while (reader.Read())
						{
							yield return rowFilter(reader);
						}
					}
				}
			}
		}
		
		// It's hard to deal with the case where the IEnumerable is returned but never enumerated over since
		// the Dispose method is on the enumerator. The Task doesn't provide a Dispose method either
		// so you are pretty much forced to gather the result in memory to allow clean resource management.
		
		public static Task<IEnumerable<TRow>> GetRowsTask<TRow>(
			this IConnectionProvider provider, 
			Action<IDbCommand> prepareCommand, 
			Func<IDataReader, TRow> rowFilter)
		{	
			// TODO - can special case if DbCommand as SqlCommand and use BeginExecuteReader
			return Task<IEnumerable<TRow>>.Factory.StartNew(() => GetRows(provider, prepareCommand, rowFilter).ToList(), TaskCreationOptions.LongRunning);
		}
	}
}

