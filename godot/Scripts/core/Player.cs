using Godot;
using System;
using System.Threading.Tasks;

public interface Player {
	(Vector2I a, Vector2I b, Vector2I c, Vector2I d) PlaceTile(Board board);
	Vector2I PlaceInitialStack(Board board);
	(Vector2I src, Vector2I dest, ushort count) MoveTokens(Board board);
}

public enum TileOrientation : ushort {
	Flat,
	DownRight,
	DownLeft,
}

public enum Color : ushort {
	Red,
	Blue,
}

public enum MoveDirection : ushort {
	NW,
	NE,
	E,
	SE,
	SW,
	W,
}
