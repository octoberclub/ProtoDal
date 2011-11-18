using System;
using NUnit.Framework;
using Rhino.Mocks;
using ProtoDal;
using System.Data;
using System.Linq;

namespace ProtoDalTests
{
	[TestFixture]
	public class SprocExtensionTests
	{
		[SetUp]
		public void SetUp()
		{
			provider = MockRepository.GenerateMock<IConnectionProvider>();
			connection = MockRepository.GenerateMock<IDbConnection>();
			command = MockRepository.GenerateMock<IDbCommand>();
			reader = MockRepository.GenerateMock<IDataReader>();
		}
		
		private IConnectionProvider provider;
		private IDbConnection connection;
		private IDbCommand command;
		private IDataReader reader;
		
		[Test]
		public void Test_ResourceDisposalWhenNoRowsAreReturned()
		{
			provider.Expect(p => p.GetOpenConnection()).Return(connection);
			connection.Expect(c => c.CreateCommand()).Return(command);
			command.Expect(c => c.ExecuteReader(CommandBehavior.SequentialAccess)).Return(reader);
		
			// TODO - Find some way to return true, false
			reader.Expect(r => r.Read()).Return(false);
			reader.Expect(r => r.Dispose());
			command.Expect(c => c.Dispose());
			connection.Expect(c => c.Dispose());
			
			var prepareCommandHasBeenInvoked = false;
			var filterRowsHasBeenInvoked = false;

			var result = provider.GetRows(
				(cmd) => { prepareCommandHasBeenInvoked = true; }, 
				(rdr) => { filterRowsHasBeenInvoked = true; return 10; }).ToList();
			// Without ToList the enumeration is never evaluated
			
			Assert.IsTrue(prepareCommandHasBeenInvoked, "prepareCommand was not invoked");
			Assert.IsFalse(filterRowsHasBeenInvoked, "filterRows was invoked");
			
			reader.VerifyAllExpectations();
			command.VerifyAllExpectations();
			provider.VerifyAllExpectations();
		}
		
		[Test]
		public void Test_ResourceDisposalWhenPreperationFails()
		{
			provider.Expect(p => p.GetOpenConnection()).Return(connection);
			connection.Expect(c => c.CreateCommand()).Return(command);
			command.Expect(c => c.ExecuteReader(CommandBehavior.SequentialAccess)).Return(reader);
		
			// TODO - Find some way to return true, false
			command.Expect(c => c.Dispose());
			connection.Expect(c => c.Dispose());
			
			var filterRowsHasBeenInvoked = false;
			
			// TODO - find version of nunit that supports Assert.Throw(...)

			var result = provider.GetRows(
				(cmd) => { throw new Exception("preperation failed"); }, 
				(rdr) => { filterRowsHasBeenInvoked = true; return 10; }).ToList();
			// Without ToList the enumeration is never evaluated
			
			Assert.IsFalse(filterRowsHasBeenInvoked, "filterRows was invoked");
			
			command.VerifyAllExpectations();
			provider.VerifyAllExpectations();
		}
	}
}