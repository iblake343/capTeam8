using Godot;
using System;

public partial class Game : Node
{
	private Node player1;
	private Node player2;
	
	public void registerPlayer1(Node p1) {
		this.player1 = p1;
	}
	public void registerPlayer2(Node p2) {
		this.player2 = p2;
	}
	
	public void startGame()
	{
		if (player1 != null) {
			GD.Print("Found player1");
			TilePlaced += () => {
				GD.Print("tile placed. Asking again...");
				player1.Call("place_tile");
			};
			player1.Call("place_tile");
		}
	}
	
	[Signal]
	public delegate void TilePlacedEventHandler();

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
