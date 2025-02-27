using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;

//using Location = Vector2I;
//using Cell = (Color color, byte count);
//using TileLocation = (Location a, Location b, Location c, Location d);
//using Move = (Location src, Location dest);

public partial class Board {
	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static int BoardSize();

	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static void InitBoard(byte[] data);

	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static int xAt(byte[] data, int x, int y);

	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static int xCurrentPlayer(byte[] data);
	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static int xWinner(byte[] data);
	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static int xExpectedMoveKind(byte[] data);

	public byte[] data;
	public Board() {
		var size = BoardSize();
		data = new byte[size];
		InitBoard(data);
	}

	public int CurrentPlayer() {
		return xCurrentPlayer(data);
	}
	public int Winner() {
		return xWinner(data);
	}
	public int ExpectedMoveKind() {
	    return xExpectedMoveKind(data);
	}
	
	public (Color color, byte count) At(Vector2I _loc) {
		return (0, 0);
	}
	
	public (Vector2I min, Vector2I max) Frame() {
		return (new(0, 0), new(0, 0));
	}
}
