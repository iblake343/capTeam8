using Godot;
using System;
using System.Threading.Tasks;

public partial class GameScene : Node, Display, Player {
	public Board board;
	public Board GetBoard() {return board;}
	private bool move_camera = false;
	private Control game_layer;
	public override void _Ready() {
		board = new Board();
 		Player player1 = new AIPlayer();
		GetNode<HBoxContainer>("UI Layer/UI/Bottom UI/MarginContainer/HBoxContainer/P1 Tiles").Show();
		GetNode<HBoxContainer>("UI Layer/UI/Bottom UI/MarginContainer/HBoxContainer/P2 Tiles").Show();
		game_layer = GetNode<Control>("UI Layer/UI/GameLayer");
		var game = new Game(
			new Player[] {player1, this},
			new string[] {"Human", "Computer"},
			board, this);
		game.StartGame();
	}
	
	public void DrawBoard(Board board) {
		TileMapLayer base_layer = GetNode<TileMapLayer>("Center/HexLayer");
		TileMapLayer back_layer = GetNode<TileMapLayer>("Center/BackLayer");
		TileMapLayer chip_layer = GetNode<TileMapLayer>("Center/ChipLayer");
		TileMapLayer number_layer = GetNode<TileMapLayer>("Center/NumberLayer");
		var frame = board.Frame();
		
		GD.Print($"expected move kind is {board.ExpectedMoveKind()}");
		
		
		number_layer.Clear();
		base_layer.Clear();
		chip_layer.Clear();
		//Will need score by player
		//Can make this a function if want to clean up
		if (board.CountTilesPlaced() == 1) {
			Control node = GetNode<PanelContainer>("UI Layer/UI/Bottom UI/MarginContainer/HBoxContainer/P1 Tiles/Control1");	
			node.Modulate = new Godot.Color(0.6f, 0.6f, 0.6f);
		}
		else {
			int tilesPlaced = board.CountTilesPlaced();
			for (int i = 0; i < tilesPlaced; i++) {
				string nodePath = $"UI Layer/UI/Bottom UI/MarginContainer/HBoxContainer/P{i % 2 + 1} Tiles/Control{i / 2 + 1}";
				Control node = GetNode<Control>(nodePath); // Use Control directly

				if (node != null) {
					node.Modulate = new Godot.Color(0.6f, 0.6f, 0.6f); // Corrected constructor
				}
				else {
					GD.Print($"Node {nodePath} not found!");
				}
			}
		}

		
		for (int y = frame.min.Y - 1; y < frame.max.Y + 1; ++y) {
			for (int x = frame.min.X - 1; x < frame.max.X + 1; ++x) {
				var cell = board.At(new(x, y));
				if (cell.count == -1) {
					if (board.IsCoast(new(x, y)))
						back_layer.SetCell(new(x, y), 4, new(0, 0));
					continue;
				};
				
				base_layer.SetCell(new(x, y), fitRange(1, 5, hashCoords(x, y, 5)), new(0, 0));
				if (cell.count == 0) continue;
				
				chip_layer.SetCell(new(x, y), Globals.players[(int)cell.color], new(0, 0));
				if (cell.count == 1) continue;
				
				number_layer.SetCell(new(x, y), cell.count - 1, new(0, 0));
			}
		}
	}
	public void DeclareWinner(string name) {
		if(board.ExpectedMoveKind() < 0) {
			GetNode<Node2D>("UI Layer/UI/Win Dialogue").Show();
		}
	}
	public void DeclareTie() {
		if(board.ExpectedMoveKind() < 0) {
			GetNode<Node2D>("UI Layer/UI/Win Dialogue").Show();
		}
	}
	
	public async Task<int> PlaceTile(Board board) {
		Control node = new();
		ulong nodeId = node.GetInstanceId();
		node.SetScript(GD.Load<Script>("res://Scripts/place_tile.gd"));
		node = (Control)InstanceFromId(nodeId);
		game_layer.CallDeferred("add_child", node);
		await ToSignal(GetTree(), "node_removed");
		
		var frame = board.Frame();
		
		TileMapLayer base_layer = GetNode<TileMapLayer>("Center/HexLayer");
		Camera2D camera = GetNode<Camera2D>("Center/Camera");
		Vector2 center = (base_layer.MapToLocal(frame.min) + base_layer.MapToLocal(frame.max)) * 0.5f;
		camera.Position = base_layer.ToGlobal(center);
		
		return 0;
	}

	public async Task<int> PlaceInitialStack(Board board) { 
		Node node = new();
		ulong nodeId = node.GetInstanceId();
		node.SetScript(GD.Load<Script>("res://Scripts/place_initial_stack.gd"));
		node = (Node)InstanceFromId(nodeId);
		game_layer.CallDeferred("add_child", node);
		GetNode<HBoxContainer>("UI Layer/UI/Bottom UI/MarginContainer/HBoxContainer/P1 Tiles").Hide();
		GetNode<HBoxContainer>("UI Layer/UI/Bottom UI/MarginContainer/HBoxContainer/P2 Tiles").Hide();
		
		await ToSignal(GetTree(), "node_removed");
		return 0;
	}
	
	public async Task<int> MoveTokens(Board board) {
		Node node = new();
		ulong nodeId = node.GetInstanceId();
		node.SetScript(GD.Load<Script>("res://Scripts/select_move_start.gd"));
		node = (Node)InstanceFromId(nodeId);
		game_layer.CallDeferred("add_child", node);
		
		await ToSignal(GetTree(), "node_removed");
		return 0;
	}
	
	private int hashCoords(int x, int y, int max) {
		string a = (x >= 0) ? new string ('A', x) : new string ('B', -x);
		string b = (y >= 0) ? new string ('C', y) : new string ('B', -y);
		return Math.Abs((a + b).GetHashCode()) % max + 1;
	}
	private int fitRange(int low, int high, int val) {
		if (val <= low) return low;
		if (val >= high) return high;
		return val;
	}
}
