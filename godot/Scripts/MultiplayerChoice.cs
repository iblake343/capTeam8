using Godot;

public partial class MultiplayerChoice : CanvasLayer 
{
	[Export]
	private int port = 9999;
	[Export]
	private string address = "127.0.0.1";
	private ENetMultiplayerPeer peer;
	Label statusLbl;
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

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void startGame() { 

		var gameScene = ResourceLoader.Load<PackedScene>("res://Scenes/Game.tscn").Instantiate<Node>();
		GetTree().Root.AddChild(gameScene);


		this.Hide(); 
	}

	private void sendPlayerInfo(string move, int id){ 

		if(Multiplayer.IsServer()) { 
			Rpc("sendPlayerInfo", move, id);
		}
	}
	
}
