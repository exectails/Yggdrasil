using System.Collections.Generic;
using Xunit;
using Yggdrasil.Collections;
using Yggdrasil.Geometry;

namespace Yggdrasil.Test.Collections
{
	public class InOutTrackerTests
	{
		[Fact]
		public void AddAndRemoveItems()
		{
			var character1 = new Character("Character1", new Vector2F(50, 50));
			var character2 = new Character("Character2", new Vector2F(40, 40));
			var character3 = new Character("Character3", new Vector2F(100, 100));

			var map = new Map();
			map.AddCharacter(character1);
			map.AddCharacter(character2);
			map.AddCharacter(character3);

			character1.Update(out var added, out var removed);

			Assert.Equal(["Character2"], added);
			Assert.Equal([], removed);

			character2.Position = new Vector2F(0, 0);
			character1.Update(out added, out removed);

			Assert.Equal([], added);
			Assert.Equal(["Character2"], removed);

			character1.Update(out added, out removed);

			Assert.Equal([], added);
			Assert.Equal([], removed);

			character2.Position = new Vector2F(45, 45);
			character3.Position = new Vector2F(55, 55);
			character1.Update(out added, out removed);

			Assert.Equal(["Character2", "Character3"], added);
			Assert.Equal([], removed);

			character1.Update(out added, out removed);

			Assert.Equal([], added);
			Assert.Equal([], removed);
		}

		private class Character(string name, Vector2F pos)
		{
			public string Name { get; } = name;
			public Map Map { get; set; }
			public Vector2F Position { get; set; } = pos;
			public InOutTracker<Character> VisibleCharacters = new();

			public void Update(out List<string> added, out List<string> removed)
			{
				added = [];
				removed = [];

				this.VisibleCharacters.Begin();

				this.Map.GetVisibleCharacters(this, 20, this.VisibleCharacters.UpdateList);
				this.VisibleCharacters.Update();

				if (!this.VisibleCharacters.Empty)
				{
					foreach (var character in this.VisibleCharacters.Added)
						added.Add(character.Name);

					foreach (var character in this.VisibleCharacters.Removed)
						removed.Add(character.Name);
				}

				this.VisibleCharacters.End();
			}
		}

		private class Map
		{
			public List<Character> Characters = [];

			public void AddCharacter(Character character)
			{
				character.Map = this;
				this.Characters.Add(character);
			}

			public void RemoveCharacter(Character character)
			{
				this.Characters.Remove(character);
				character.Map = null;
			}

			public void GetVisibleCharacters(Character character, float radius, IList<Character> result)
			{
				foreach (var other in Characters)
				{
					if (character == other)
						continue;

					if (character.Position.InRange(other.Position, radius))
						result.Add(other);
				}
			}
		}
	}
}
