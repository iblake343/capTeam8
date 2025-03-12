using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Godot;

public interface Player {
	Task<int> PlaceTile(Board board);
	Task<int> PlaceInitialStack(Board board);
	Task<int> MoveTokens(Board board);
}

public class TuiPlayer : Player {
	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static void xTuiDoTurn(byte[] data);

	public async Task<int> PlaceTile(Board board) {
		xTuiDoTurn(board.data);
		return 0;
	}
	public async Task<int> PlaceInitialStack(Board board) {
		xTuiDoTurn(board.data);
		return 0;
	}
	public async Task<int> MoveTokens(Board board) {
		xTuiDoTurn(board.data);
		return 0;
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
	public static string NameNorthMajorAskew(this Direction dir) {
		if (dir == Direction.NW) return "west";
		if (dir == Direction.NE) return "northwest";
		if (dir == Direction.E) return "northeast";
		if (dir == Direction.SE) return "east";
		if (dir == Direction.SW) return "southeast";
		if (dir == Direction.W) return "southwest";
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
	public static Direction Left(this Direction dir) {
		if (dir == Direction.NW) return Direction.W;
		if (dir == Direction.NE) return Direction.NW;
		if (dir == Direction.E) return Direction.NE;
		if (dir == Direction.SE) return Direction.E;
		if (dir == Direction.SW) return Direction.SE;
		if (dir == Direction.W) return Direction.SW;
		Console.WriteLine("error in Direction.Right: out of bounds");
		return Direction.E;
	}
	public static Direction Right(this Direction dir) {
		if (dir == Direction.NW) return Direction.NE;
		if (dir == Direction.NE) return Direction.E;
		if (dir == Direction.E) return Direction.SE;
		if (dir == Direction.SE) return Direction.SW;
		if (dir == Direction.SW) return Direction.W;
		if (dir == Direction.W) return Direction.NW;
		Console.WriteLine("error in Direction.Left: out of bounds");
		return Direction.E;
	}
	public static Direction Back(this Direction dir) {
		if (dir == Direction.NW) return Direction.SE;
		if (dir == Direction.NE) return Direction.SW;
		if (dir == Direction.E) return Direction.W;
		if (dir == Direction.SE) return Direction.NW;
		if (dir == Direction.SW) return Direction.NE;
		if (dir == Direction.W) return Direction.E;
		Console.WriteLine("error in Direction.Back: out of bounds");
		return Direction.E;
	}
}
