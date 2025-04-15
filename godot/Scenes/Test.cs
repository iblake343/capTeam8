using Godot;
using System;

public partial class Test : Node2D {
	public override void _Ready() {
		string old_board_src= "0,0|0,1|1,0|1,1|t";
		string new_board_src= "0,0|0,1|1,0|1,1|2,0|potato2,1|3,0|3,1|h";
		Board old_board = Board.Parse(old_board_src.ToCharArray());
		Board new_board = Board.Parse(new_board_src.ToCharArray());
		string diff = Board.DiffAsString(old_board, new_board);
		
		GD.Print($"diff is [{diff}]");
		GD.Print($"length = {diff.Length}");
	}
}
