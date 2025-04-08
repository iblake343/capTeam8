using Godot;
using System;
using System.Threading.Tasks;

public partial class GameScene : Node, Display, Player {
	public Board board;
	public Board GetBoard() {return board;}
	public override void _Ready() {
		board = new Board();
		Player player1 = new AIPlayer();
		
		var game = new Game(
			new Player[] {player1, this},
			new string[] {"Human", "Computer"},
			board, this);
		game.StartGame();
	}
	
	public void DrawBoard(Board board) {
		TileMapLayer base_layer = GetNode<TileMapLayer>("HexLayer");
		TileMapLayer chip_layer = GetNode<TileMapLayer>("ChipLayer");
		TileMapLayer number_layer = GetNode<TileMapLayer>("NumberLayer");
		RichTextLabel player1Score = GetNode<RichTextLabel>("PlaceChipOverlay/Player1Score");
		RichTextLabel player2Score = GetNode<RichTextLabel>("PlaceChipOverlay/Player2Score");
		RichTextLabel player1ConnectionScore = GetNode<RichTextLabel>("PlaceChipOverlay/Player1ConnectionScore");
		RichTextLabel player2ConnectionScore = GetNode<RichTextLabel>("PlaceChipOverlay/Player2ConnectionScore");
		var frame = board.Frame();
		
		number_layer.Clear();
		base_layer.Clear();
		chip_layer.Clear();
		//Will need score by player
		player1Score.Text = "5";
		player2Score.Text = "1";
		player1ConnectionScore.Text = "15";
		player2ConnectionScore.Text = "15";
		//Can make this a function if want to clean up
		if (board.CountTilesPlaced() == 1) {
			Control node = GetNode<Control>("PlaceTileOverlay/Control1");	
			node.Modulate = new Godot.Color(0.6f, 0.6f, 0.6f);
		}
		else {
			int tilesPlaced = board.CountTilesPlaced();
			for (int i = 1; i <= tilesPlaced; i++) {
				string nodePath = $"PlaceTileOverlay/Control{i}";
				Control node = GetNode<Control>(nodePath); // Use Control directly

				if (node != null) {
					node.Modulate = new Godot.Color(0.6f, 0.6f, 0.6f); // Corrected constructor
				}
				else {
					GD.Print($"Node {nodePath} not found!");
				}
			}
		}

		
		for (int y = frame.min.Y; y < frame.max.Y; ++y) {
			for (int x = frame.min.X; x < frame.max.X; ++x) {
				var cell = board.At(new(x, y));
				if (cell.count == -1) continue;
				
				base_layer.SetCell(new(x, y), 4, new(0, 0));
				if (cell.count == 0) continue;
				
				chip_layer.SetCell(new(x, y), Globals.players[(int)cell.color], new(0, 0));
				if (cell.count == 1) continue;
				
				number_layer.SetCell(new(x, y), cell.count - 1, new(0, 0));
			}
		}
	}
	public void DeclareWinner(string name) { }
	public void DeclareTie() { }
	
	public async Task<int> PlaceTile(Board board) {
		Node node = new();
		ulong nodeId = node.GetInstanceId();
		node.SetScript(GD.Load<Script>("res://Scripts/place_tile.gd"));
		node = (Node)InstanceFromId(nodeId);
		CallDeferred("add_child", node);
		GetNode<CanvasLayer>("PlaceTileOverlay").Show();
		await ToSignal(GetTree(), "node_removed");
		return 0;
	}

	public async Task<int> PlaceInitialStack(Board board) { 
		Node node = new();
		ulong nodeId = node.GetInstanceId();
		node.SetScript(GD.Load<Script>("res://Scripts/place_initial_stack.gd"));
		node = (Node)InstanceFromId(nodeId);
		CallDeferred("add_child", node);
		GetNode<CanvasLayer>("PlaceTileOverlay").Hide();
		GetNode<CanvasLayer>("PlaceChipOverlay").Show();
		
		await ToSignal(GetTree(), "node_removed");
		return 0;
	}
	
	public async Task<int> MoveTokens(Board board) {
		Node node = new();
		ulong nodeId = node.GetInstanceId();
		node.SetScript(GD.Load<Script>("res://Scripts/select_move_start.gd"));
		node = (Node)InstanceFromId(nodeId);
		CallDeferred("add_child", node);
		
		await ToSignal(GetTree(), "node_removed");
		return 0;
	}
	
	private int hashCoords(int x, int y, int max) {
		string a = (x >= 0) ? new string ('A', x) : new string ('B', -x);
		string b = (y >= 0) ? new string ('C', y) : new string ('B', -y);
		return (a + b).GetHashCode() % max;
	}
}
