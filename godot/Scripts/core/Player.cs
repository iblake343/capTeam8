using Godot;
using System;
using System.Threading.Tasks;

public interface Player {
	Task<(Vector2I loc, TileOrientation orient)> PlaceTile();
	Task<Vector2I> PlaceInitialStack();
	Task<(Vector2I loc, MoveDirection dir, byte count)> MoveTokens();
}

public enum TileOrientation : ushort {
	Flat,
	DownRight,
	DownLeft,
}

public enum MoveDirection : ushort {
	NW,
	NE,
	E,
	SE,
	SW,
	W,
}
