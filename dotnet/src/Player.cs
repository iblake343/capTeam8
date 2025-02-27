using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

public interface Player {
    void PlaceTile(Board board);
    void PlaceInitialStack(Board board);
    void MoveTokens(Board board);
}

public class EmptyPlayer : Player {
	public void PlaceTile(Board _board) {}
    public void PlaceInitialStack(Board _board) {}
    public void MoveTokens(Board _board) {}
}

public class TuiPlayer : Player {
	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static void xTuiPlaceTile(byte[] data);

	public void PlaceTile(Board board) {
		xTuiPlaceTile(board.data);
	}
    public void PlaceInitialStack(Board _board) {}
    public void MoveTokens(Board _board) {}
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
