using System;
using System.Threading.Tasks;

public partial class Game {
	public Player player1;
	public Player player2;
	public Board board;
	public Display display;

    public Game(Player p1, Player p2, Board b, Display disp) {
		player1 = p1;
		player2 = p2;
		board = b;
		display = disp;
	}
	
	public void startGame() {
	    display.drawBoard(board);
        player1.PlaceTile(board);
		Console.WriteLine($"player 1 placed a tile");
	}
}
