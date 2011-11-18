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
		public void Test_ResourceDisposalWhenRowIsReturned()
		{
			provider.Expect(p => p.GetOpenConnection()).Return(connection);
			connection.Expect(c => c.CreateCommand()).Return(command);
			command.Expect(c => c.ExecuteReader(CommandBehavior.SequentialAccess)).Return(reader);
		
			reader.Expect(r => r.Read()).Return(true).Repeat.Once();
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
			Assert.IsTrue(filterRowsHasBeenInvoked, "filterRows was invoked");
			Assert.AreEqual(10, result.First());
			
			reader.VerifyAllExpectations();
			command.VerifyAllExpectations();
			provider.VerifyAllExpectations();
		}
		
		[Test]
		public void Test_ResourceDisposalWhenPreparationFails()
		{
			provider.Expect(p => p.GetOpenConnection()).Return(connection);
			connection.Expect(c => c.CreateCommand()).Return(command);
			
			command.Expect(c => c.Dispose());
			connection.Expect(c => c.Dispose());
			
			var filterRowsHasBeenInvoked = false;
			
			var expectedException = new Exception("preperation failed");
			
			try
			{
				provider.GetRows(
					(cmd) => { throw expectedException; }, 
					(rdr) => { filterRowsHasBeenInvoked = true; return 10; }).ToList();
				// Without ToList the enumeration is never evaluated				
			}
			catch (Exception ex)
			{
				Assert.AreEqual(expectedException, ex);	
			}
			
			Assert.IsFalse(filterRowsHasBeenInvoked, "filterRows was invoked");
			
			command.VerifyAllExpectations();
			provider.VerifyAllExpectations();
		}
		
		[Test]
		public void Test_ResourceDisposalWhenFilterFails()
		{
			provider.Expect(p => p.GetOpenConnection()).Return(connection);
			connection.Expect(c => c.CreateCommand()).Return(command);
			command.Expect(c => c.ExecuteReader(CommandBehavior.SequentialAccess)).Return(reader);
			
			reader.Expect(r => r.Read()).Return(true).Repeat.Once();
			reader.Expect(r => r.Dispose());
			command.Expect(c => c.Dispose());
			connection.Expect(c => c.Dispose());
			
			var prepareCommandHasBeenInvoked = false;
			
			var expectedException = new Exception("filter failed");
			
			try
			{
				provider.GetRows(
					(cmd) => { prepareCommandHasBeenInvoked = true; }, 
					(rdr) => { throw expectedException; return 10; }).ToList();
				// Without ToList the enumeration is never evaluateid				
			}
			catch (Exception ex)
			{
				Assert.AreEqual(expectedException, ex);	
			}
			
			Assert.IsTrue(prepareCommandHasBeenInvoked, "preperation was not invoked");
			
			command.VerifyAllExpectations();
			provider.VerifyAllExpectations();
		}
	}
}