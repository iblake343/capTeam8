using Godot;
using System;

public partial class ArbitraryPlayer : Player {
	public (Vector2I a, Vector2I b, Vector2I c, Vector2I d) PlaceTile(Board _board) {
		return (new(0, 0), new(0, 1), new(1, 0), new(1, 1));
	}
	public Vector2I PlaceInitialStack(Board _board) {
		return new(0, 0);
	}
	public (Vector2I src, Vector2I dest, ushort count) MoveTokens(Board _board) {
		return (new(0, 0), new(0, 0), 0);
	}
}
