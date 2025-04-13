using Godot;
using System;

public partial class CharacterPlayer2 : Node2D
{
	private Button _button;
	private Button _button2;
	private Button _button3;
	private Button _button4;

	public override void _Ready()
	{
		_button = GetNode<Button>("Button");
		_button2 = GetNode<Button>("Button2");
		_button3 = GetNode<Button>("Button3");
		_button4 = GetNode<Button>("Button4");

		// Gray out the button depending on selected player
		GrayOutSelectedPlayer(Globals.players[0]);
	}

	private void GrayOutSelectedPlayer(int selectedIndex)
	{
		Button target = selectedIndex switch
		{
			0 => _button,
			1 => _button2,
			2 => _button3,
			3 => _button4,
			_ => null
		};

		if (target != null)
		{
			target.Disabled = true;
			target.Modulate = new Godot.Color(0.6f, 0.6f, 0.6f);
		}
	}

	public override void _Process(double delta)
	{
		// Runs every frame
	}

	private void OnButtonPressed()
	{
		Globals.players[1] = 0;
		GetTree().ChangeSceneToFile("res://Scenes/Game.tscn");
	}

	private void OnButton2Pressed()
	{
		Globals.players[1] = 1;
		GetTree().ChangeSceneToFile("res://Scenes/Game.tscn");
	}

	private void OnButton3Pressed()
	{
		Globals.players[1] = 2;
		GetTree().ChangeSceneToFile("res://Scenes/Game.tscn");
	}

	private void OnButton4Pressed()
	{
		Globals.players[1] = 3;
		GetTree().ChangeSceneToFile("res://Scenes/Game.tscn");
	}
}
