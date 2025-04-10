using System;

public class RandPlayer : Player {
	public void PlaceTile(Board board) {
	    Vector2I[] options = board.LegalTileLocations();
		// loop required because the random placement could fail
		while(true) {
			var	ix_option = Random.Shared.Next(options.Length);
			Vector2I loc = options[ix_option];
			Direction dir = (Direction) Random.Shared.Next(6);
			// exits on success
		    if(board.PlaceTile(loc, dir)) {
			Console.WriteLine($"Placed tile at ({loc.x}, {loc.y}) in direction {((Direction)dir).Name()}");
				return;
			}
		}
	}
    public void PlaceInitialStack(Board _board) {}
    public void MoveTokens(Board _board) {}
	    var options = board.LegalTileArrangements();
		var ix = Random.Shared.Next(options.Length);
		var	option = options[ix];
		Vector2I loc = option.origin;
		Direction dir = option.orientation;
	    board.PlaceTile(loc, dir);
		Console.WriteLine($"Placed tile at ({loc.x}, {loc.y}) in the {dir.NameNorthMajorAskew()} direction");
	}
	
    public void PlaceInitialStack(Board board) {
		var options = board.LegalInitialStackLocations();
		var ix = Random.Shared.Next(options.Length);
		var loc = options[ix];
		board.PlaceInitialStack(loc);
		Console.WriteLine($"Placed initial stack at ({loc.x}, {loc.y})");
	}
	
    public void MoveTokens(Board board) {
		var options = board.LegalStartStacks();
		var loc = options[Random.Shared.Next(options.Length)];
		
		var dest_options = board.LegalDestLocations(loc);
		var dest = dest_options[Random.Shared.Next(dest_options.Length)];

		var stack_size = board.At(loc).count;
		if (stack_size < 2) Console.WriteLine($"stack_size = {stack_size}");
		var amt = Random.Shared.Next(1,stack_size);

		board.MoveTokens(loc, dest, amt);
		Console.WriteLine($"Moved {amt} tokens from ({loc.x}, {loc.y}) to ({dest.x}, {dest.y})");
	}
}
