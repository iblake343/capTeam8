using Godot;
using System;
using System.Threading.Tasks;

public partial class Game : Node
{
	private Player player1;
	private Player player2;
	
	public void registerPlayer1(Node p1) {
		this.player1 = p1 as Player;
	}
	public void registerPlayer2(Node p2) {
		this.player2 = p2 as Player;
	}
	
	[Signal]
	public delegate void TilePlacedEventHandler();
	
	public async void startGame()
	{
		if (player1 == null) {
			GD.Print("Error: No player 1");
			return;
		}
		
		var locations = await player1.PlaceTile();
		GD.Print($"got a tile placed at {loc} with orientation {orient}");
	}
}
