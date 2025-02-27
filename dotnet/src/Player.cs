using System;
using System.Threading.Tasks;

public interface Player {
    void PlaceTile(Board board);
    void PlaceInitialStack(Board board);
    void MoveTokens(Board board);
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
