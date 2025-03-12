using Godot;
using System;
using System.Threading.Tasks;

public partial class GameScene : Node, Display, Player {
	public Board board;
	public Board GetBoard() {return board;}
	public override void _Ready() {
		board = new Board();

		Player player2 = new RandPlayer();
		

		var game = new Game(
			new Player[] {this, player2},
			new string[] {"Human", "Computer"},
			board, this);
		game.StartGame();
	}
	
	public void DrawBoard(Board board) {
		//GetNode<Node>("Drawer").Call("draw");
		GD.Print("TODO DrawBoard");
	}
	public void DeclareWinner(string name) { }
	public void DeclareTie() { }
	
	public async Task<int> PlaceTile(Board board) {
		
		Node node = new();
		ulong nodeId = node.GetInstanceId();
		node.SetScript(GD.Load<Script>("res://Scripts/tilemap.gd"));
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
}
