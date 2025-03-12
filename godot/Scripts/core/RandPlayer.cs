using System;
using Godot;
using System.Threading.Tasks;

public class RandPlayer : Player {
	public async Task<int> PlaceTile(Board board) {
		var options = board.LegalTileArrangements();
		var ix = Random.Shared.Next(options.Length);
		var	option = options[ix];
		Vector2I loc = option.origin;
		Direction dir = option.orientation;
		board.PlaceTile(loc, dir);
		GD.Print($"Placed tile at ({loc.X}, {loc.Y}) in the {dir.NameNorthMajorAskew()} direction");
		return 0;
	}
	
	public async Task<int> PlaceInitialStack(Board board) {
		var options = board.LegalInitialStackLocations();
		var ix = Random.Shared.Next(options.Length);
		var loc = options[ix];
		board.PlaceInitialStack(loc);
		GD.Print($"Placed initial stack at ({loc.X}, {loc.Y})");
		return 0;
	}
	
	public async Task<int> MoveTokens(Board board) {
		var options = board.LegalStartStacks();
		var loc = options[Random.Shared.Next(options.Length)];
		
		var dest_options = board.LegalDestLocations(loc);
		var dest = dest_options[Random.Shared.Next(dest_options.Length)];

		var stack_size = board.At(loc).count;
		if (stack_size < 2) GD.Print($"stack_size = {stack_size}");
		var amt = Random.Shared.Next(1,stack_size);

		board.MoveTokens(loc, dest, amt);
		GD.Print($"Moved {amt} tokens from ({loc.X}, {loc.Y}) to ({dest.X}, {dest.Y})");
		return 0;
	}
}
