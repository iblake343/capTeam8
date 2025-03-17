using Godot;
using System;
using System.Threading.Tasks;

public partial class GameScene : Node, Display, Player {
	public Board board;
	public Board GetBoard() {return board;}
	public override void _Ready() {
		board = new Board();
		Player player1 = new RandPlayer();
		
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
		var frame = board.Frame();
		
		number_layer.Clear();
		base_layer.Clear();
		chip_layer.Clear();
		
		for (int y = frame.min.Y; y < frame.max.Y; ++y) {
			for (int x = frame.min.X; x < frame.max.X; ++x) {
				var cell = board.At(new(x, y));
				if (cell.count == -1) continue;
				
				base_layer.SetCell(new(x, y), 4, new(0, 0));
				if (cell.count == 0) continue;
				
				chip_layer.SetCell(new(x, y), Globals.players[(int)cell.color], new(0, 0));
				if (cell.count == 1) continue;
				
				number_layer.SetCell(new(x, y), cell.count, new(0, 0));
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
		
		await ToSignal(GetTree(), "node_removed");
		return 0;
	}

	public Task<int> PlaceInitialStack(Board board) { 
		GD.Print("TODO PlaceInitialStack");
		return Task.FromResult(0);
	}
	
	public Task<int> MoveTokens(Board board) {
		GD.Print("TODO MoveTokens");
		return Task.FromResult(0);
	}
	
	private int hashCoords(int x, int y, int max) {
		string a = (x >= 0) ? new string ('A', x) : new string ('B', -x);
		string b = (y >= 0) ? new string ('C', y) : new string ('B', -y);
		return (a + b).GetHashCode() % max;
	}
}
