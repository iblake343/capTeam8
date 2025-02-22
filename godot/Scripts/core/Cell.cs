using Godot;
using System;

public partial class Cell : Node {

	private byte data;
	public static Cell fromByte(byte b) {
		return new Cell{ data = b };
	}

	/// returns 0 for no player or a positive integer for the other players
	public byte player() {
		return (byte)(data / (byte)100);
	}

	public byte countTokens() {
		return (byte)(data % (byte)100);
	}

	public bool isOcean() {
		return data == 1;
	}

	public string description() {
		if (isOcean()) return "Ocean";
		if (player() == 0) return "Empty";
		return $"{countTokens()} Player {player()}";
	}
}
