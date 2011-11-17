using System;

namespace ProtoDal
{
	public class ConnectionProviderException : Exception
	{
		public ConnectionProviderException(string connectionString)
			: base(string.Format("Could not extract provider for {0}", connectionString))
		{
		}
	}
}