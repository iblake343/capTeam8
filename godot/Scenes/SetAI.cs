using Godot;
using System;

public partial class SetAI : Node
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Globals.player_kinds[1] = PlayerKind.Random;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
