using Xunit;
using Yggdrasil.Db.MySql.SimpleCommands;

namespace Yggdrasil.Test.Db.MySql
{
	public class UpdateCommandTests
	{
		[Fact]
		public void Build()
		{
			using var cmd = new UpdateCommand("UPDATE `table` SET {parameters}", null, null);

			cmd.Set("id", 5);
			cmd.Set("name", "foobar");

			var query = cmd.Build();

			Assert.Equal("UPDATE `table` SET `id` = @id, `name` = @name", query);
		}
	}
}
