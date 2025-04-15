using Godot;
using System;

public partial class ButtonHandler : Node {
	public void _on_single_player_pressed() {
		Callable.From(() => {
			Globals.player_kinds = new PlayerKind[] {PlayerKind.Human, PlayerKind.AI};
			GetTree().ChangeSceneToFile("res://Scenes/LocalSetup.tscn");
		}).CallDeferred();
	}
}
