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

	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static int xCountLegalTileLocations(byte[] data);
	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static void xGetLegalTileLocations(byte[] board, int[] coords);
	public Vector2I[] LegalTileLocations() {
		var size = xCountLegalTileLocations(data);
		var coords = new int[size * 2];
		var vectors = new Vector2I[size];
		xGetLegalTileLocations(data, coords);
		for (int i = 0; i < size; ++i) {
			vectors[i] = new(coords[i*2], coords[i*2+1]);
		}
		return vectors;
	}


	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static bool xPlaceTile(byte[] data, int x, int y, int dir);
    // returns `true` on successful placement
	public bool PlaceTile(Vector2I loc, Direction dir) {
		return xPlaceTile(data, loc.x, loc.y, (int) dir);
	}
	
	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static int xCountLegalInitialStackLocations(byte[] data);
	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static void xGetLegalInitialStackLocations(byte[] board, int[] coords);
	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static bool xPlaceInitialStack(byte[] data, int x, int y);
	public Vector2I[] LegalInitialStackLocations() {
		var size = xCountLegalInitialStackLocations(data);
		var coords = new int[size * 2];
		var vectors = new Vector2I[size];
		xGetLegalInitialStackLocations(data, coords);
		for (int i = 0; i < size; ++i) {
			vectors[i] = new(coords[i*2], coords[i*2+1]);
		}
		return vectors;
	}

	public bool PlaceInitialStack(Vector2I loc) {
		return xPlaceInitialStack(data, loc.x, loc.y);
	}

	
	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static int xCountLegalStartStacks(byte[] data);
	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static void xGetLegalStartStacks(byte[] board, int[] coords);

	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static int xCountLegalDestLocations(byte[] data, int x, int y);
	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static void xGetLegalDestLocations(byte[] board, int x, int y, int[] coords);

	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static bool xMoveTokens(byte[] data, int x1, int y1, int x2, int y2, int amt);

	public Vector2I[] LegalStartStacks() {
		var size = xCountLegalStartStacks(data);
		var coords = new int[size * 2];
		var vectors = new Vector2I[size];
		xGetLegalStartStacks(data, coords);
		for (int i = 0; i < size; ++i) {
			vectors[i] = new(coords[i*2], coords[i*2+1]);
		}
		return vectors;
	}
	public Vector2I[] LegalDestLocations(Vector2I src) {
		var size = xCountLegalDestLocations(data, src.x, src.y);
		var coords = new int[size * 2];
		var vectors = new Vector2I[size];
		xGetLegalDestLocations(data, src.x, src.y, coords);
		for (int i = 0; i < size; ++i) {
			vectors[i] = new(coords[i*2], coords[i*2+1]);
		}
		return vectors;
	}

	public bool MoveTokens(Vector2I src, Vector2I dest, int amt) {
		return xMoveTokens(data, src.x, src.y, dest.x, dest.y, amt);
	}

	public Cell At(Vector2I loc) {
	    var cell = xAt(data, loc.x, loc.y);
        if (cell == 0) return new(Color.Blue, 0);
		if (cell == 1) return new(Color.Blue, -1);
		return new((Color) ((cell / 100) - 1), cell % 100);
	}
	
	public (Vector2I min, Vector2I max) Frame() {
		return (new(0, 0), new(0, 0));
	}
}

public class Cell {
	public Color color;
	public int count;
	public Cell (Color color, int count) {
		this.color = color;
		this.count = count;
	}
}
