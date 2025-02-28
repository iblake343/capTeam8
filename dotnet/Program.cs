
var board = new Board();
var player1 = new TuiPlayer();
var player2 = new RandPlayer();
var display = new TuiDisplay();
var game = new Game(
    new Player[] {player1, player2},
	new string[] {"Red", "Blue"},
	board, display);
game.StartGame();
