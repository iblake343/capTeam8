using Godot;
using System;

public partial class AppPlayer : Node, Player {
	public override async Task<(Vector2I loc, TileOrientation orient)> PlaceTile() {
		
	}
	Task<Vector2I> PlaceInitialStack();
	Task<(Vector2I loc, MoveDirection dir, byte count)> MoveTokens();
}
