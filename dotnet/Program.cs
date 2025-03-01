
var board = new Board();

Console.WriteLine("Is player 1 (Red) a computer (c) (default) or human (h)?");
string p1opt = Console.ReadLine();
Console.WriteLine("Is player 2 (Blue) a computer (c) (default) or human (h)?");
string p2opt = Console.ReadLine();

Player player1 =
    (p1opt == "h" || p1opt == "human") ? new TuiPlayer() : new RandPlayer();
Player player2 =
    (p2opt == "h" || p2opt == "human") ? new TuiPlayer() : new RandPlayer();

var display = new TuiDisplay();
var game = new Game(
    new Player[] {player1, player2},
	new string[] {"Red", "Blue"},
	board, display);
game.StartGame();
