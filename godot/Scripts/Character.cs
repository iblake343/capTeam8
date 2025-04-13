using Godot;
using System;

public partial class Character : Node2D
{
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
		// Runs every frame
	}

	private void OnButtonPressed()
	{
		Globals.players[0] = 0;
		GetTree().ChangeSceneToFile("res://Scenes/Game.tscn");
	}

	private void OnButton2Pressed()
	{
		Globals.players[0] = 1;
		GetTree().ChangeSceneToFile("res://Scenes/Game.tscn");
	}

	private void OnButton3Pressed()
	{
		Globals.players[0] = 2;
		GetTree().ChangeSceneToFile("res://Scenes/Game.tscn");
	}

	private void OnButton4Pressed()
	{
		Globals.players[0] = 3;
		GetTree().ChangeSceneToFile("res://Scenes/Game.tscn");
	}
}
