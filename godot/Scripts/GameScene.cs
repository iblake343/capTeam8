using Godot;
using System;
using System.Threading.Tasks;

public partial class GameScene : Node, Display, Player {
	public Board board;
	public Board GetBoard() {return board;}
	public Godot.Collections.Array<Texture2D> character_tokens;
	
	private bool move_camera = false;
	private Control game_layer;
	private Hero p1;
	private Hero p2;
	
	public override void _Ready() {
		board = new Board();
		GetNode<HBoxContainer>("UI Layer/UI/Bottom UI/MarginContainer/HBoxContainer/P1 Tiles").Show();
		GetNode<HBoxContainer>("UI Layer/UI/Bottom UI/MarginContainer/HBoxContainer/P2 Tiles").Show();
		game_layer = GetNode<Control>("UI Layer/UI/GameLayer");
		
		character_tokens = new();
		character_tokens.Add(GD.Load("res://assets/CharacterTiles/Clownfish disk1 (1).png") as Texture2D);
		character_tokens.Add(GD.Load("res://assets/CharacterTiles/Crab Disk0 (1).png") as Texture2D);
		character_tokens.Add(GD.Load("res://assets/CharacterTiles/Octopus Disk0 (1).png") as Texture2D);
		character_tokens.Add(GD.Load("res://assets/CharacterTiles/Shark disk1 (1).png") as Texture2D);
		
		var names = new[] {
			"Clownfish",
			"Crabs",
			"Octopodes",
			"Sharks"
		};
		
		p1 = GetNode<Hero>("UI Layer/UI/Bottom UI/MarginContainer/HBoxContainer/P1");
		p1.Face().Texture = character_tokens[Globals.players[0]];
		p2 = GetNode<Hero>("UI Layer/UI/Bottom UI/MarginContainer/HBoxContainer/P2");
		p2.Face().Texture = character_tokens[Globals.players[1]];
		
		var player_names = new string[]{
			names[Globals.players[0]],
			names[Globals.players[1]],
		};
		
		var game = new Game(
			new Player[] {
				Globals.player_kinds[0].NewPlayer(this),
				Globals.player_kinds[1].NewPlayer(this),
			},
			player_names,
			board, this);
		game.StartGame();
	}
	
	public void DrawBoard(Board board) {
		TileMapLayer base_layer = GetNode<TileMapLayer>("Center/HexLayer");
		TileMapLayer back_layer = GetNode<TileMapLayer>("Center/BackLayer");
		TileMapLayer chip_layer = GetNode<TileMapLayer>("Center/ChipLayer");
		TileMapLayer number_layer = GetNode<TileMapLayer>("Center/NumberLayer");
		var frame = board.Frame();
		
		if (board.CurrentPlayer() == 0) {
			p1.Outline().SetVisible(true);
			p2.Outline().SetVisible(false);
		} else {
			p1.Outline().SetVisible(false);
			p2.Outline().SetVisible(true);
		}
		
		if (move_camera) {
			Camera2D camera = GetNode<Camera2D>("Center/Camera");
			Vector2 center = (base_layer.MapToLocal(frame.min) + base_layer.MapToLocal(frame.max)) * 0.5f;
			camera.Position = base_layer.ToGlobal(center);
		}
		move_camera = board.ExpectedMoveKind() == 0;
		
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
			GetNode<Label>("UI Layer/UI/Win Dialogue/PanelContainer/MarginContainer/VBoxContainer/Label").Text = name + " are the winners!";
		}
	}
	public void DeclareTie() {
		if(board.ExpectedMoveKind() < 0) {
			GetNode<Node2D>("UI Layer/UI/Win Dialogue").Show();
			GetNode<Label>("UI Layer/UI/Win Dialogue/PanelContainer/MarginContainer/VBoxContainer/Label").Text = "Both players win!";
		}
	}
	
	public async Task<int> PlaceTile(Board board) {
		Control node = new();
		ulong nodeId = node.GetInstanceId();
		node.SetScript(GD.Load<Script>("res://Scripts/place_tile.gd"));
		node = (Control)InstanceFromId(nodeId);
		game_layer.CallDeferred("add_child", node);
		await ToSignal(GetTree(), "node_removed");
		
		
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
	
	public void QuitToMenu() {
		GetTree().ChangeSceneToFile("res://Scenes/menu.tscn");
	}
}
