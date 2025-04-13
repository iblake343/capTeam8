using System;
using Godot;
using System.Threading.Tasks;


public class RandPlayer : Player {
	public async Task<int> PlaceTile(Board board) {
		var options = board.LegalTileArrangements();
		var ix = Random.Shared.Next(options.Count);
		var	option = options[ix];
		Vector2I loc = option.origin;
		Direction dir = option.orientation;
		board.PlaceTile(loc, dir);
		GD.Print($"Placed tile at ({loc.X}, {loc.Y}) in the {dir.NameNorthMajorAskew()} direction");
		return 0;
	}
	
	public async Task<int> PlaceInitialStack(Board board) {
		var options = board.LegalInitialStackLocations();
		var ix = Random.Shared.Next(options.Count);
		var loc = options[ix];
		board.PlaceInitialStack(loc);
		GD.Print($"Placed initial stack at ({loc.X}, {loc.Y})");
		return 0;
	}
	
	public async Task<int> MoveTokens(Board board) {
		
	}

}
