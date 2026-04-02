using Xunit;
using Yggdrasil.Db.MySql.SimpleCommands;

namespace Yggdrasil.Test.Db.MySql
{
	public class InsertCommandTests
	{
		[Fact]
		public void Build()
		{
			using var cmd = new InsertCommand("INSERT INTO `table` {parameters}", null, null);

			cmd.Set("id", 5);
			cmd.Set("name", "foobar");

			var query = cmd.Build();

			Assert.Equal("INSERT INTO `table` (`id`, `name`) VALUES (@id, @name)", query);
		}
	}
}
