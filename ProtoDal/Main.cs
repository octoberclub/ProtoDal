using System;
using System.Data.Common;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;
using NUnit.Framework;

namespace ProtoDal
{
	public interface IConnectionProvider
	{
		IDbConnection GetOpenConnection();	
	}
	
	public class ConnectionProviderException : Exception
	{
		public ConnectionProviderException(string connectionString)
			: base(string.Format("Could not extract provider for {0}", connectionString))
		{
		}
	}
	
	public class ConnectionProvider : IConnectionProvider
	{
		public ConnectionProvider(string connectionString)
		{
			if (string.IsNullOrEmpty(connectionString))
			{
				throw new ArgumentNullException("connectionString");
			}
			
			this.connectionString = connectionString;
			
			var providerName = GetProviderName(connectionString);
			
			provider = GetProvider(providerName);
		}
		
		private readonly string connectionString;
		
		private readonly DbProviderFactory provider;
		
		private static string GetProviderName(string connectionString)
		{
			var builder = new DbConnectionStringBuilder();
			builder.ConnectionString = connectionString;
			
			object providerObject;
			
			if (!builder.TryGetValue("provider", out providerObject))
			{
				return (string)providerObject;
			}
			
			// TODO - look at deducing provider from related config
			throw new ConnectionProviderException(connectionString);
		}
		
		private static DbProviderFactory GetProvider(string provider)
		{
			return DbProviderFactories.GetFactory(provider);
		}
		
		public IDbConnection GetOpenConnection()
		{
			var connection = provider.CreateConnection();
			connection.ConnectionString = connectionString;
			connection.Open();
			
			return connection;
		}
	}
	
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
		
		public static Task<IEnumerable<TRow>> GetRowsTask<TRow>(
			this IConnectionProvider provider, 
			Action<IDbCommand> prepareCommand, 
			Func<IDataReader, TRow> rowFilter)
		{			
			// TODO - can special case if DbCommand as SqlCommand and use BeginExecuteReader
			return Task<IEnumerable<TRow>>.Factory.StartNew(() => GetRows(provider, prepareCommand, rowFilter), TaskCreationOptions.LongRunning);
		}
	}
	
	[TestFixture]
	public class SprocExtensionsTests
	{
		[Test]
		public void Test()
		{
			Assert.IsTrue(true);
		}
		
	}
	
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine ("Hello World!");
		}
	}
}
