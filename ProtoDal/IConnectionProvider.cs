using System;
using System.Data;

namespace ProtoDal
{
	public interface IConnectionProvider
	{
		IDbConnection GetOpenConnection();	
	}
}