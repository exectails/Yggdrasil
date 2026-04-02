using Xunit;
using Yggdrasil.Db.MySql.SimpleCommands;

namespace Yggdrasil.Test.Db.MySql
{
	public class BatchedInsertCommandTests
	{
		[Fact]
		public void BuildSingle()
		{
			using var cmd = new BatchedInsertCommand("table", null, null);

			cmd.Set("id", 5);
			cmd.Set("name", "foobar");

			var query = cmd.Build();

			Assert.Equal("INSERT INTO `table` (`id`, `name`) VALUES (@id_0, @name_0)", query);
		}

		[Fact]
		public void BuildMultiple()
		{
			using var cmd = new BatchedInsertCommand("table", null, null);

			for (var i = 0; i < 5; ++i)
			{
				cmd.AddRow();

				cmd.Set("id", 5 + i);
				cmd.Set("name", "foobar" + i);
			}

			var query = cmd.Build();

			Assert.Equal("INSERT INTO `table` (`id`, `name`) VALUES (@id_0, @name_0), (@id_1, @name_1), (@id_2, @name_2), (@id_3, @name_3), (@id_4, @name_4)", query);
		}

		[Fact]
		public void BuildMultiple_BottomAdd()
		{
			using var cmd = new BatchedInsertCommand("table", null, null);

			for (var i = 0; i < 5; ++i)
			{
				cmd.Set("id", 5 + i);
				cmd.Set("name", "foobar" + i);

				cmd.AddRow();
			}

			var query = cmd.Build();

			Assert.Equal("INSERT INTO `table` (`id`, `name`) VALUES (@id_0, @name_0), (@id_1, @name_1), (@id_2, @name_2), (@id_3, @name_3), (@id_4, @name_4)", query);
		}

		[Fact]
		public void BuildMultiple_Clear()
		{
			using var cmd = new BatchedInsertCommand("table", null, null);

			for (var i = 0; i < 30; ++i)
			{
				cmd.Set("id", 5 + i);
				cmd.Set("name", "foobar" + i);

				cmd.AddRow();

				// Check query at different points. First after 4 rows,
				// then after 5, and then every 5 rows after 10.

				if ((i + 1) == 4)
				{
					var query = cmd.Build();
					Assert.Equal("INSERT INTO `table` (`id`, `name`) VALUES (@id_0, @name_0), (@id_1, @name_1), (@id_2, @name_2), (@id_3, @name_3)", query);

					cmd.Clear();
				}
				else if ((i + 1) == 5)
				{
					var query = cmd.Build();
					Assert.Equal("INSERT INTO `table` (`id`, `name`) VALUES (@id_0, @name_0)", query);

					cmd.Clear();
				}
				else if ((i + 1) == 10)
				{
					cmd.Clear();
				}
				else if ((i + 1) > 10 && ((i + 1) % 5) == 0)
				{
					var query = cmd.Build();
					Assert.Equal("INSERT INTO `table` (`id`, `name`) VALUES (@id_0, @name_0), (@id_1, @name_1), (@id_2, @name_2), (@id_3, @name_3), (@id_4, @name_4)", query);

					cmd.Clear();
				}
			}
		}
	}
}
