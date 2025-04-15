using Godot;
using System;
using System.Xml.Resolvers;

public partial class MultiplayerChoice : CanvasLayer 
{

	[Export]
	private int port = 9999;
	
	[Export]
	private string address = "127.0.0.1";

	private ENetMultiplayerPeer peer;

	//[Export]
	//public PackedScene PlayerFieldScene;
	//[Export] 
	//public PackedScene OpponentFieldScene;

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
		Multiplayer.MultiplayerPeer = null;
	}

	private void PeerConnected(long id)
	{
		GD.Print("Player Connected: " + id.ToString());
		statusLbl.Text = "Player  Connected";
		Rpc("startGame");
	}

	public override void _Process(double delta)
	{
	}
	
	private void _on_back_btn_pressed()
	{
		Callable.From(() => {GetTree().ChangeSceneToFile("res://Scenes/menu.tscn");}).CallDeferred();
		Multiplayer.MultiplayerPeer = null;
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
	public void _on_ai_btn_pressed(){
		GetTree().ChangeSceneToFile("res://Scenes/aivai.tscn");
	}
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void startGame() { 
		// sync button clicks and allow two choices
		// host chooses character then p2
		// coin flip
		// start game
		
		var gameScene = ResourceLoader.Load<PackedScene>("res://Scenes/Game.tscn").Instantiate<Node>();
		GetTree().Root.AddChild(gameScene);

		//GetTree().ChangeSceneToFile("res://Scenes/Game.tscn");

		this.Hide(); 
	}
	
}
