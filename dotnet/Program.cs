
var board = new Board();
var player1 = new TuiPlayer();
var player2 = new EmptyPlayer();
var display = new EmptyDisplay();
var game = new Game(player1, player2, board, display);
game.startGame();
