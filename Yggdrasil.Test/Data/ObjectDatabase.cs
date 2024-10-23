using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using Xunit;
using Yggdrasil.Data;
using Yggdrasil.Data.JSON;
using Yggdrasil.Data.JSON.ObjectOriented;

namespace Yggdrasil.Test.Data
{
	public class ObjectDatabaseTests
	{
		[Fact]
		public void Load()
		{
			using var stream = new MemoryStream(Encoding.UTF8.GetBytes(TestFile1));

			var db = new ItemDb();
			db.Load("stream", stream);

			Assert.Equal(3, db.Objects.Count);

			Assert.Equal("Sword", db.Objects.Get(1).Name);
			Assert.Equal(1.5f, db.Objects.Get(1).Weight);

			Assert.Equal("Shield", db.Objects.Get(2).Name);
			Assert.Equal(2.5f, db.Objects.Get(2).Weight);

			Assert.Equal("Potion", db.Objects.Get(3).Name);
			Assert.Equal(0.5f, db.Objects.Get(3).Weight);
		}

		[Fact]
		public void LoadMultiple()
		{
			using var stream1 = new MemoryStream(Encoding.UTF8.GetBytes(TestFile1));
			using var stream2 = new MemoryStream(Encoding.UTF8.GetBytes(TestFile2));

			var db = new ItemDb();
			db.Load("stream1", stream1);
			db.Load("stream2", stream2);

			Assert.Equal(5, db.Objects.Count);

			Assert.Equal("Sword", db.Objects.Get(1).Name);
			Assert.Equal(1.5f, db.Objects.Get(1).Weight);

			Assert.Equal("Shield", db.Objects.Get(2).Name);
			Assert.Equal(2.5f, db.Objects.Get(2).Weight);

			Assert.Equal("Potion", db.Objects.Get(3).Name);
			Assert.Equal(0.5f, db.Objects.Get(3).Weight);

			Assert.Equal("Axe", db.Objects.Get(4).Name);
			Assert.Equal(2.0f, db.Objects.Get(4).Weight);

			Assert.Equal("Bow", db.Objects.Get(5).Name);
			Assert.Equal(1.0f, db.Objects.Get(5).Weight);
		}

		[Fact]
		public void Override()
		{
			using var stream1 = new MemoryStream(Encoding.UTF8.GetBytes(TestFile1));
			using var stream2 = new MemoryStream(Encoding.UTF8.GetBytes(TestFile3));

			var db = new ItemDb();
			db.Load("stream1", stream1);

			Assert.Equal(3, db.Objects.Count);

			Assert.Equal("Sword", db.Objects.Get(1).Name);
			Assert.Equal(1.5f, db.Objects.Get(1).Weight);

			Assert.Equal("Shield", db.Objects.Get(2).Name);
			Assert.Equal(2.5f, db.Objects.Get(2).Weight);

			Assert.Equal("Potion", db.Objects.Get(3).Name);
			Assert.Equal(0.5f, db.Objects.Get(3).Weight);

			db.Load("stream2", stream2);

			Assert.Equal(3, db.Objects.Count);

			Assert.Equal("Sword", db.Objects.Get(1).Name);
			Assert.Equal(1.5f, db.Objects.Get(1).Weight);

			Assert.Equal("Shield", db.Objects.Get(2).Name);
			Assert.Equal(20f, db.Objects.Get(2).Weight);

			Assert.Equal("Potion", db.Objects.Get(3).Name);
			Assert.Equal(0.1f, db.Objects.Get(3).Weight);
		}

		[Fact]
		public void MandatoryFields()
		{
			using var stream1 = new MemoryStream(Encoding.UTF8.GetBytes(TestFile1));
			using var stream2 = new MemoryStream(Encoding.UTF8.GetBytes(TestFile4));

			var db1 = new ItemDb();
			db1.Load("stream", new MemoryStream(Encoding.UTF8.GetBytes(TestFile1)));

			Assert.Equal(3, db1.Objects.Count);
			Assert.Equal(0, db1.Warnings.Count);

			Assert.Equal("Sword", db1.Objects.Get(1).Name);
			Assert.Equal(1.5f, db1.Objects.Get(1).Weight);

			Assert.Equal("Shield", db1.Objects.Get(2).Name);
			Assert.Equal(2.5f, db1.Objects.Get(2).Weight);

			Assert.Equal("Potion", db1.Objects.Get(3).Name);
			Assert.Equal(0.5f, db1.Objects.Get(3).Weight);

			var db2 = new ItemDb();
			db2.Load("stream", new MemoryStream(Encoding.UTF8.GetBytes(TestFile4)));

			Assert.Equal(2, db2.Objects.Count);
			Assert.Equal(1, db2.Warnings.Count);

			Assert.Equal("Sword", db2.Objects.Get(1).Name);
			Assert.Equal(1.5f, db2.Objects.Get(1).Weight);

			Assert.Equal("Potion", db2.Objects.Get(3).Name);
			Assert.Equal(0.5f, db2.Objects.Get(3).Weight);

			Assert.Equal(typeof(MandatoryValueException), db2.Warnings[0].GetType());
		}

		[Fact]
		public void Enums()
		{
			using var stream1 = new MemoryStream(Encoding.UTF8.GetBytes(TestFile1));
			using var stream2 = new MemoryStream(Encoding.UTF8.GetBytes(TestFile4));

			var db1 = new ItemDb();
			db1.Load("stream", new MemoryStream(Encoding.UTF8.GetBytes(TestFile5)));

			Assert.Equal(3, db1.Objects.Count);

			Assert.Equal("Shield", db1.Objects.Get(2).Name);
			Assert.Equal(ItemType.Equipment, db1.Objects.Get(1).Type);
		}

		[Fact]
		public void Flags()
		{
			using var stream1 = new MemoryStream(Encoding.UTF8.GetBytes(TestFile1));
			using var stream2 = new MemoryStream(Encoding.UTF8.GetBytes(TestFile4));

			var db1 = new ItemDb();
			db1.Load("stream", new MemoryStream(Encoding.UTF8.GetBytes(TestFile5)));

			Assert.Equal(3, db1.Objects.Count);

			Assert.Equal("Sword", db1.Objects.Get(1).Name);
			Assert.Equal(ItemUsableType.Knight, db1.Objects.Get(1).UsableType);

			Assert.Equal("Shield", db1.Objects.Get(2).Name);
			Assert.Equal(ItemUsableType.Knight | ItemUsableType.Archer, db1.Objects.Get(2).UsableType);

			Assert.Equal("Potion", db1.Objects.Get(3).Name);
			Assert.Equal(ItemUsableType.All, db1.Objects.Get(3).UsableType);
		}

		public enum ItemType
		{
			Undefined,
			Consumable,
			Equipment,
		}

		public enum ItemUsableType
		{
			All = 0xFFFF,
			Knight = 1,
			Wizard = 2,
			Archer = 4,
		}

		public class ItemData : StandardObjectData
		{
			public string Name { get; set; }
			public float Weight { get; set; }
			public ItemType Type { get; set; }
			public ItemUsableType UsableType { get; set; }
		}

		public class ItemDb : StandardObjectDatabase<ItemData>
		{
			protected override void CheckFields(JObject entry)
			{
				entry.AssertNotMissing("name", "weight");
			}

			protected override void ReadEntry(JObject entry, ItemData dataObj, ItemData existingObj)
			{
				dataObj.Name = entry.ReadString("name", existingObj.Name);
				dataObj.Weight = entry.ReadFloat("weight", existingObj.Weight);
				dataObj.Type = entry.ReadEnum("type", existingObj.Type);
				dataObj.UsableType = entry.ReadEnum("usableType", existingObj.UsableType);
			}
		}

		private const string TestFile1 = @"
		[
			{ id: 1, name: 'Sword', weight: 1.5 },
			{ id: 2, name: 'Shield', weight: 2.5 },
			{ id: 3, name: 'Potion', weight: 0.5 }
		]
		";

		private const string TestFile2 = @"
		[
			{ id: 4, name: 'Axe', weight: 2.0 },
			{ id: 5, name: 'Bow', weight: 1.0 }
		]
		";

		private const string TestFile3 = @"
		[
			{ id: 2, weight: 20 },
			{ id: 3, weight: 0.1 }
		]
		";

		private const string TestFile4 = @"
		[
			{ id: 1, name: 'Sword', weight: 1.5 },
			{ id: 2, name: 'Shield', waight: 2.5 }, // Typo
			{ id: 3, name: 'Potion', weight: 0.5 }
		]
		";

		private const string TestFile5 = @"
		[
			{ id: 1, name: 'Sword', weight: 1.5, type: 'Equipment', usableType: 'Knight' },
			{ id: 2, name: 'Shield', weight: 2.5, type: 'Equipment', usableType: 'Knight|Archer' },
			{ id: 3, name: 'Potion', weight: 0.5, type: 'Consumable', usableType: 'All' }
		]
		";
	}
}
