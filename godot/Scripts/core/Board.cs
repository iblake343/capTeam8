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

	private byte[] data;
	public override void _Ready() {
		var size = BoardSize();
		data = new byte[size];
		InitBoard(data);
	}
	
	public (Vector2I a, Vector2I b, Vector2I c, Vector2I d)[] GetLegalTilePlacements() {
		var placements = new ArrayList();
		return placements.ToArray();
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
	
	public (Vector2I src, Vector2I dest)[] GetLegalMoveStarts() {
		var count = xCountMoveStarts(data);
		var moves = new Vector2I[count];
		xGetMoveStarts(data, moves);
		return moves;
	}
	
	public (Color color, byte count) At(Vector2I _loc) {
		return (0, 0);
	}
	
	public (Vector2I min, Vector2I max) Frame() {
		return (new(0, 0), new(0, 0));
	}
}
