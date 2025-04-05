using Godot;
using System;

public partial class MultiplayerChoice : CanvasLayer
{

	[Export]
	private int port = 8888;
	
	[Export]
	private string address = "127.0.0.1";

	private ENetMultiplayerPeer peer;
	// Called when the node enters the scene tree for the first time.

	Label statusLbl;
	public override void _Ready()
	{
		Multiplayer.PeerConnected += PeerConnected;
		Multiplayer.PeerDisconnected += PeerDisconnected;
		Multiplayer.ConnectedToServer += ConnectedToServer;
		Multiplayer.ConnectionFailed += ConnectionFailed;

		statusLbl = GetNode<Label>("Control/status");
		statusLbl.Text = "Choose to host or join a game";

	}

	private void ConnectionFailed()
	{
		GD.Print("CONNECTION FAILED");
	}


	private void ConnectedToServer()
	{
		GD.Print("Connected to server.");
	}


	private void PeerDisconnected(long id)
	{
		GD.Print("Player disconnected: " + id.ToString());
	}

	private void PeerConnected(long id)
	{
		GD.Print("Player Connected: " + id.ToString());
		statusLbl.Text = "Player  Connected";

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	private void _on_back_btn_pressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/select_net_ai.tscn"); //fix 
	}

	public void _on_host_pressed() { 
		peer = new ENetMultiplayerPeer();
		var error = peer.CreateServer(port, 2);

		if (error != Error.Ok) { 
			GD.Print("error cannot host! : " + error.ToString());
		}

		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);

		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Waiting For Players");
		statusLbl.Text = "Waiting  for  players";


	}
	
	public void _on_join_pressed() { 
		peer = new ENetMultiplayerPeer();
		peer.CreateClient(address, port);

		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);

		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Joining Game");
		statusLbl.Text = "Joining  Game";


	}

	public void _on_start_pressed() { 
		Rpc("startGame");
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void startGame() { 
		// take them through these scenes after lobby filled 
		// coin flip
		// p1 chooses character then p2 

		var scene = ResourceLoader.Load<PackedScene>("res://Scenes/character.tscn").Instantiate<Node2D>(); 
		GetTree().Root.AddChild(scene);
		this.Hide();
	}
	
}
