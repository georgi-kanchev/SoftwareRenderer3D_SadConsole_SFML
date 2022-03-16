using SadConsole;
using SFML.System;

namespace ConsoleGame
{
	public class Unit
	{
		public Vector2i PositionPrevious { get; private set; }
		public Vector2i Position { get; private set; }
		public int GraphicBottom { get; set; }
		public int GraphicTop { get; set; }

		public Unit(Vector2i position, int graphicBottom, int graphicTop = 0)
		{
			PositionPrevious = position;
			Position = position;
			GraphicBottom = graphicBottom;
			GraphicTop = graphicTop;
		}
		public void Move(Vector2i direction)
		{
			PositionPrevious = Position;
			Position += direction;

			TryPreventOutOfBounds();

			void TryPreventOutOfBounds()
			{
				if (Position.X < 0 || Position.X >= Settings.CELLS_WIDTH - Game.UI_WIDTH ||
					Position.Y < 0 || Position.Y >= Settings.CELLS_HEIGHT)
					Position = PositionPrevious;
			}
		}
		public void Draw()
		{
			if (GraphicBottom != default)
				Settings.Units.SetGlyph(Position.X + Game.UI_WIDTH, Position.Y, GraphicBottom);
			if (GraphicTop != default)
				Settings.Units.SetGlyph(Position.X + Game.UI_WIDTH, Position.Y - 1, GraphicTop);
		}
	}
}
