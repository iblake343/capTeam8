using Godot;
using System;

public partial class CharacterPlayer1 : Node2D
{
	public override void _Ready()
	{
		Globals.player_kinds[0] = PlayerKind.Human;
		Globals.player_kinds[1] = PlayerKind.Human;
	}

	public override void _Process(double delta)
	{
		// Runs every frame
	}

	private void OnButtonPressed()
	{
		Globals.players[0] = 0;
		GetTree().ChangeSceneToFile("res://Scenes/characterPlayer2.tscn");
	}

	private void OnButton2Pressed()
	{
		Globals.players[0] = 1;
		GetTree().ChangeSceneToFile("res://Scenes/characterPlayer2.tscn");
	}

	private void OnButton3Pressed()
	{
		Globals.players[0] = 2;
		GetTree().ChangeSceneToFile("res://Scenes/characterPlayer2.tscn");
	}

	private void OnButton4Pressed()
	{
		Globals.players[0] = 3;
		GetTree().ChangeSceneToFile("res://Scenes/characterPlayer2.tscn");
	}
	
	private void on_quit_btn_pressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/menu.tscn");
	}
}
