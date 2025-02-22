using Godot;
using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;

//using Location = Vector2I;
//using Cell = (Color color, byte count);
//using TileLocation = (Location a, Location b, Location c, Location d);
//using Move = (Location src, Location dest);

public partial class Board : Node {
	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static int BoardSize();

	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static void InitBoard(byte[] data);

	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static int xAt(byte[] data, int x, int y);
	
	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static int xCountTilePlacements(byte[] data);
	
	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static int xGetTilePlacements(byte[] data, (Vector2I a, Vector2I b, Vector2I c, Vector2I d)[] locs);

	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static int xCountTilePlacements(byte[] data);

	private byte[] data;
	public override void _Ready() {
		GD.Print($"Size of a board (in bytes) is {BoardSize()}");
		var size = BoardSize();
		data = new byte[size];
		GD.Print("Allocated byte array");
		InitBoard(data);
		GD.Print("Initialized Board struct as byte array");
		GD.Print($"Board[3, 4] = {xAt(data, 3, 4)}");
	}
	
	public (Vector2I a, Vector2I b, Vector2I c, Vector2I d)[] GetLegalTilePlacements() {
		var count = xCountTilePlacements(data);
		var placements = new (Vector2I a, Vector2I b, Vector2I c, Vector2I d)[count];
		xGetTilePlacements(data, placements);
		return placements;
	}
	
	public Vector2I[] GetLegalHexLocations() {
		var count = xCountHexLocations(data);
		var locs = new Vector2I[count];
		xGetHexLocations(data, locs);
		return locs;
	}
	
	public Vector2I[] GetLegalInitialStackPlacements() {
		var count = xCountInitialStackPlacements(data);
		var locs = new Vector2I[count];
		xGetInitialStackPlacements(data, locs);
		return locs;
	}
	
	public (Vector2I src, Vector2I dest, ushort count)[] GetLegalMoves() {
		var count = xCountMoves(data);
		var moves = new Vector2I[count];
		xGetMoves(data, moves);
		return moves;
	}
	
	public (Color color, byte count) At(Vector2I _loc) {
		return (0, 0);
	}
	
	public (Vector2I min, Vector2I max) Frame() {
		return (new(0, 0), new(0, 0));
	}
}
