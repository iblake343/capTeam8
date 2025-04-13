using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using Godot;

//using Location = Vector2I;
//using Cell = (Color color, byte count);
//using TileLocation = (Location a, Location b, Location c, Location d);
//using Move = (Location src, Location dest);

public partial class Board : GodotObject {
	public byte[] data;
	public Board() {
		var size = BoardSize();
		data = new byte[size];
		InitBoard(data);
		}
	public Board(Board b) {
		this.data = new List<byte>(b.data).ToArray();
		}
	public Board Clone() {
		return new(this);
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
	public int CountTilesPlaced() {
		return xCountTilesPlaced(data);
		}
	public Cell At(Vector2I loc) {
		var cell = xAt(data, loc.X, loc.Y);
		if (cell == 0) return new(Color.Blue, 0);
		if (cell == 1) return new(Color.Blue, -1);
		return new((Color) ((cell / 100) - 1), cell % 100);
		}
	public bool IsCoast(Vector2I loc) {
		for (int i = 0; i < 6; ++i) {
			Vector2I probe = loc + ((Direction)i).Vector();
			if (xAt(data, probe.X, probe.Y) != 1) return true;
		}
		return false;
		}
	public (Vector2I min, Vector2I max) Frame() {
		var coords = new int[4];
		xGetFrame(data, coords);
		return (new(coords[0], coords[1]), new(coords[2], coords[3]));
		}
	
	public Godot.Collections.Array<Vector2I> LegalTileLocations() {
		var size = xCountLegalTileLocations(data);
		var coords = new int[size * 2];
		var vectors = new Vector2I[size];
		xGetLegalTileLocations(data, coords);
		for (int i = 0; i < size; ++i) {
			vectors[i] = new(coords[i*2], coords[i*2+1]);
		}
		return new(vectors);
		}
	public Godot.Collections.Array<TileArrangement> LegalTileArrangements() {
		var size = xCountLegalTileArrangements(data);
		var coords = new int[size * 3];
		var tiles = new TileArrangement[size];
		xGetLegalTileArrangements(data, coords);
		for (int i = 0; i < size; ++i) {
			tiles[i] = new TileArrangement( new Vector2I(coords[i*3], coords[i*3+1]), (Direction) coords[i*3+2]);
		}
		return new(tiles);
		}
	public Godot.Collections.Array<Vector2I> GetLocsFromOriginAndDir(Vector2I orig, int diri) {
		Direction dir = (Direction) diri;
		Vector2I v = dir.Vector();
		Vector2I w = dir.Right().Vector();
		return new(new[]{
			orig, orig + v, orig + w, orig + w + v,
		});
		}
	public bool IsLegalTilePlacement(Vector2I loc, Direction dir) {
		return xIsLegalTilePlacement(data, loc.X, loc.Y, (int) dir);
		}
	public bool PlaceTile(Vector2I loc, Direction dir) {
		return xPlaceTile(data, loc.X, loc.Y, (int) dir);
		}
	
	public Godot.Collections.Array<Vector2I> LegalInitialStackLocations() {
		var size = xCountLegalInitialStackLocations(data);
		var coords = new int[size * 2];
		var vectors = new Vector2I[size];
		xGetLegalInitialStackLocations(data, coords);
		for (int i = 0; i < size; ++i) {
			vectors[i] = new(coords[i*2], coords[i*2+1]);
		}
		return new(vectors);
		}
	public bool PlaceInitialStack(Vector2I loc) {
		return xPlaceInitialStack(data, loc.X, loc.Y);
		}
	
	public Godot.Collections.Array<Vector2I> LegalStartStacks() {
		var size = xCountLegalStartStacks(data);
		var coords = new int[size * 2];
		var vectors = new Vector2I[size];
		xGetLegalStartStacks(data, coords);
		for (int i = 0; i < size; ++i) {
			vectors[i] = new(coords[i*2], coords[i*2+1]);
		}
		return new(vectors);
		}
	public Godot.Collections.Array<Vector2I> LegalDestLocations(Vector2I src) {
		var size = xCountLegalDestLocations(data, src.X, src.Y);
		var coords = new int[size * 2];
		xGetLegalDestLocations(data, src.X, src.Y, coords);
		var vectors = new Vector2I[size];
		for (int i = 0; i < size; ++i) {
			vectors[i] = new(coords[i*2], coords[i*2+1]);
		}
		return new(vectors);
		}
	public bool MoveTokens(Vector2I src, Vector2I dest, int amt) {
		return xMoveTokens(data, src.X, src.Y, dest.X, dest.Y, amt);
		}
	
	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static int BoardSize();
	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static void InitBoard(byte[] data);
	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static int xAt(byte[] data, int x, int y);
	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static int xCurrentPlayer(byte[] data);
	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static int xCountTilesPlaced(byte[] data);
	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static int xWinner(byte[] data);
	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static int xExpectedMoveKind(byte[] data);
	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static int xCountLegalTileLocations(byte[] data);
	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static void xGetLegalTileLocations(byte[] board, int[] coords);
	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static int xCountLegalTileArrangements(byte[] data);
	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static void xGetLegalTileArrangements(byte[] data, int[] options);
	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static bool xPlaceTile(byte[] data, int x, int y, int dir);
	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static bool xIsLegalTilePlacement(byte[] data, int x, int y, int dir);
	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static int xCountLegalInitialStackLocations(byte[] data);
	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static void xGetLegalInitialStackLocations(byte[] board, int[] coords);
	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static bool xPlaceInitialStack(byte[] data, int x, int y);
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
	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static bool xGetFrame(byte[] data, int[] coords);
}

public partial class Cell : GodotObject {
	public Color color;
	public int count;
	public Cell (Color color, int count) {
		this.color = color;
		this.count = count;
	}
}

public partial class TileArrangement : GodotObject {
	public Vector2I origin;
	public Direction orientation;
	public TileArrangement (Vector2I origin, Direction orientation) {
		this.origin = origin;
		this.orientation = orientation;
	}
}
