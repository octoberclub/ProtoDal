using System;
using System.Data.Common;
using System.Data;

namespace ProtoDal
{
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

}

