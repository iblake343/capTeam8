using System.Threading.Tasks;
using Godot;

public partial class MultiplayerChoice : CanvasLayer, Player
{
	[Export]
	private int port = 9999;
	[Export]
	private string address = "127.0.0.1";
	private ENetMultiplayerPeer peer;
	Label statusLbl;

	private Board previousState;
	public override void _Ready()
	{
		Multiplayer.PeerConnected += PeerConnected;
		Multiplayer.PeerDisconnected += PeerDisconnected;
		Multiplayer.ConnectedToServer += ConnectedToServer;
		Multiplayer.ConnectionFailed += ConnectionFailed;
		Multiplayer.ServerDisconnected += ServerDisconnected;

		statusLbl = GetNode<Label>("Control/status");
		statusLbl.Text = "Choose to host or join a game";
	
	}

    private void ServerDisconnected()
    {
		if (peer != null)
		{
			peer.Close();
			peer = null;
		}
        GD.Print("SERVER DISCONNECTED");
		Multiplayer.MultiplayerPeer = null;
    }

    private void ConnectionFailed()
	{
		GD.Print("CONNECTION FAILED");
		//Multiplayer.MultiplayerPeer = null;	
	}

	private void ConnectedToServer()
	{
		GD.Print("Connected to server.");
	}

	private void PeerDisconnected(long id)
	{
		GD.Print("Player disconnected: " + id.ToString());
		Multiplayer.MultiplayerPeer = null;
	}

	private void PeerConnected(long id)
	{
		GD.Print("Player Connected: " + id.ToString());
		statusLbl.Text = "Player  Connected";
		Rpc("startGame");
	}
	
	private void _on_back_btn_pressed()
	{
		if (peer != null)
		{
			peer.Close();
			peer = null;
		}
		Multiplayer.MultiplayerPeer = null;
		Callable.From(() => { GetTree().ChangeSceneToFile("res://Scenes/menu.tscn"); }).CallDeferred();
	}

	public void _on_host_pressed() {

		if (peer != null)
		{
			peer.Close();
			peer = null;
		}
		Multiplayer.MultiplayerPeer = null;

		peer = new ENetMultiplayerPeer();
		var error = peer.CreateServer(port, 2); 

		if (error != Error.Ok) {
			GD.Print("error cannot host! : " + error.ToString());
			peer = null;
			return;
		}

		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);


		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Waiting For Players");
		statusLbl.Text = "Waiting  for  players";
	}
	
	public void _on_join_pressed() { 
		peer = new ENetMultiplayerPeer();
		peer.CreateClient(address, port, 0, 0, 2);

		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);

		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Joining Game");
		statusLbl.Text = "Joining  Game";

	}
	public void _on_ai_btn_pressed(){
		GetTree().ChangeSceneToFile("res://Scenes/aivai.tscn");
	}
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void startGame() { 

		int peerId = Multiplayer.GetUniqueId();
		if (peerId == 1) {
		Globals.player_kinds = new PlayerKind[] {PlayerKind.Human, PlayerKind.Network};
		}
		else { 
		Globals.player_kinds = new PlayerKind[] {PlayerKind.Network, PlayerKind.Human};
		}
		Globals.network_instance = this;

		Callable.From(() => {GetTree().ChangeSceneToFile("res://Scenes/Game.tscn");}).CallDeferred();
		
	}

	[Rpc(MultiplayerApi.RpcMode.Authority)]
    public void SubmitMove(string diff, int playerId)
    {
       GD.Print($"[Server] Received move from player {playerId}: {diff}");

		// Relay the move to the other peer
		int sender = playerId;
		int receiver = GetOtherPeerId(sender);

		RpcId(receiver, nameof(ReceiveMove), diff, sender);
    }

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void ReceiveMove(string diff, int playerId)
    {
        GD.Print($"[Client] Receiving move from player {playerId}: {diff}");
		response = diff;
    }

	private string response = null;
    public async Task<int> DoAction(Board board)
    {
		// Don't send a move to the peer if it's the first move
		if (board.Frame().min != board.Frame().max) {
			string diff = Board.DiffAsString(previousState, board);
			// submit move
			int myId = Multiplayer.GetUniqueId();
			SubmitMove(diff, myId);
		}

		//response = receive move *(done in rpc recieve function)*
		string turn = response;
		board.ParseAndDoTurn(turn); // not implemented yet
		previousState = board.Clone();
		return 0;
	}

    public async Task<int> PlaceTile(Board board)
    {
		return await DoAction(board);
	}
    public async Task<int> PlaceInitialStack(Board board)
    {
		return await DoAction(board);
    }

    public async Task<int> MoveTokens(Board board)
    {
		return await DoAction(board);
    }
}
