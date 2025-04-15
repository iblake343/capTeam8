using Godot;
using System;

public partial class Test : Node2D {
	public override void _Ready() {
		string old_board_src= "h";
		Board old_board = Board.Parse(old_board_src.ToCharArray());
		var options = old_board.LegalTileArrangements();
		for (int ix = 0; ix < options.Count; ++ix) {
			var option = options[ix];
			Vector2I loc = option.origin;
			Direction dir = option.orientation;
			Board new_board = old_board.Clone();
			new_board.PlaceTile(loc, dir);
			string diff = Board.DiffAsString(old_board, new_board);
			GD.Print($"diff is [{diff}]");
			GD.Print($"length = {diff.Length}");
		}
	}
}
