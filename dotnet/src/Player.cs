using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

public interface Player {
    void PlaceTile(Board board);
    void PlaceInitialStack(Board board);
    void MoveTokens(Board board);
}

public class TuiPlayer : Player {
	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static void xTuiDoTurn(byte[] data);

	public void PlaceTile(Board board) {
		xTuiDoTurn(board.data);
	}
    public void PlaceInitialStack(Board board) {
		xTuiDoTurn(board.data);
	}
    public void MoveTokens(Board board) {
		xTuiDoTurn(board.data);
	}
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

public enum Direction : ushort {
	NW = 0,
	NE = 1,
	E = 2,
	SE = 3,
	SW = 4,
	W = 5,
}

public static class DirectionExtensions {
	public static string Name(this Direction dir) {
		if (dir == Direction.NW) return "northwest";
		if (dir == Direction.NE) return "northeast";
		if (dir == Direction.E) return "east";
		if (dir == Direction.SE) return "southeast";
		if (dir == Direction.SW) return "southwest";
		if (dir == Direction.W) return "west";
		Console.WriteLine("error in Direction.Name: out of bounds");
		return "error";
	}
	public static Vector2I Vector(this Direction dir) {
		if (dir == Direction.NW) return new(0, -1);
		if (dir == Direction.NE) return new(1, -1);
		if (dir == Direction.E) return new(1, 0);
		if (dir == Direction.SE) return new(0, 1);
		if (dir == Direction.SW) return new(-1, 1);
		if (dir == Direction.W) return new(-1, 0);
		Console.WriteLine("error in Direction.Vector: out of bounds");
		return new(42, 42);
	}
}
