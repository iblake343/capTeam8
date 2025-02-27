using System;
using System.Threading.Tasks;

public partial class Game {
	private Player player1;
	private Player player2;
	private Board board;
	
	public void registerPlayer1(Player p1) {
		this.player1 = p1;
	}
	public void registerPlayer2(Player p2) {
		this.player2 = p2;
	}
	
	public async void startGame()
	{
		if (player1 == null) {
			Console.WriteLine("Error: No player 1");
			return;
		}
		
		var locations = await player1.PlaceTile();
		Console.WriteLine($"got a tile placed at {loc} with orientation {orient}");
	}
}
