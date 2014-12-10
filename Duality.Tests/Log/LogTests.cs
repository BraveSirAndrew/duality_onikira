using System.Linq;
using NUnit.Framework;

namespace Duality.Tests.Log
{
	[TestFixture]
	public class LogTests
	{
		[Test]
		public void When_writeError_and_there_is_no_params_Then_it_does_not_throw()
		{
			Assert.DoesNotThrow(() => Duality.Log.Game.WriteError("This is a test {0}"));
		}

		[Test]
		public void When_writeWarning_and_there_is_no_params_Then_it_does_not_throw()
		{
			Assert.DoesNotThrow(() => Duality.Log.Game.WriteWarning("This is a test {0}"));
		}

		[Test]
		public void When_write_and_there_is_no_params_Then_it_does_not_throw()
		{
			Assert.DoesNotThrow(() => Duality.Log.Game.Write("This is a test {0}"));
		}

		[Test]
		public void When_params_contains_a_game_object_Then_sets_log_entry_game_object_reference()
		{
			var dataLogOutput = Duality.Log.Game.Outputs.FirstOrDefault(l => l is DataLogOutput) as DataLogOutput;
			var logEntryCount = dataLogOutput.Data.Count();
			var gameObject = new GameObject();
			
			Duality.Log.Game.Write("Test", gameObject);
			
			Assert.AreEqual(logEntryCount + 1, dataLogOutput.Data.Count());
			Assert.AreSame(gameObject, dataLogOutput.Data.Last().GameObject);
		}
	}
}
