using Godot;
using System;

public partial class Globals : Node {
	public static PlayerKind[] player_kinds = new[]{PlayerKind.Human, PlayerKind.AI};
	public static int[] players = new[]{1, 2};
	public static Player network_instance;
}

public enum PlayerKind {
	Human,
	AI,
	Random,
	Network,
}

public static class PlayerKindExtensions {
	public static Player NewPlayer(this PlayerKind kind, GameScene gs) {
		if (kind == PlayerKind.Human) return gs;
		if (kind == PlayerKind.AI) return new AIPlayer();
		if (kind == PlayerKind.Random) return new RandPlayer();
		if (kind == PlayerKind.Network) return Globals.network_instance;
		Console.WriteLine("error in PlayerKind.NewPlayer: out of bounds");
		return gs;
	}
}
