
var board = new Board();
var player1 = new RandPlayer();
var player2 = new RandPlayer();
var display = new TuiDisplay();
var game = new Game(
    new Player[] {player1, player2},
	new string[] {"player 1", "player 2"},
	board, display);
game.StartGame();
