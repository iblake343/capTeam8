using System;
using System.Threading.Tasks;
using Godot;

public partial class Game {
	private Player[] players;
	private string[] names;
	private Board board;
	private Display display;

	public Game(Player[] ps, string[] ns, Board b, Display disp) {
		players = ps;
		names = ns;
		board = b;
		display = disp;
	}
	
	public async void StartGame() {
		while (true) {
			display.DrawBoard(board);
			int k = board.ExpectedMoveKind();

			if (k == -1) {
				int ix_winner = board.Winner();
				if (ix_winner == -1) {
					display.DeclareTie();
				} else {
					display.DeclareWinner(names[ix_winner]);
				}
				return;
			}
			int ix_player = board.CurrentPlayer();
			Player player = players[ix_player];

			Console.WriteLine($"{names[ix_player]}'s turn");
			MoveKind j = (MoveKind) k;
			if (j == MoveKind.PlaceTile) {
				if (await Task.Run(() => player.PlaceTile(board)) == 1) break;
			} else if (j == MoveKind.PlaceInitialStack) {
				if (await Task.Run(() => player.PlaceInitialStack(board)) == 1) break;
			} else if (j == MoveKind.MoveTokens) {
				if (await Task.Run(() => player.MoveTokens(board)) == 1) break;
			}
		}
		GD.Print("Game Ended Premeaekjlewrly");
		display.ExitPremature();
	}
}

public enum MoveKind : ushort {
	PlaceTile,
	PlaceInitialStack,
	MoveTokens,
}
