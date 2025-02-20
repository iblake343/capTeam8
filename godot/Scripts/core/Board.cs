using Godot;
using System;
using System.Runtime.InteropServices;

public partial class Board : Node {
	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static int BoardSize();

	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static void InitBoard(byte[] data);

	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static int xAt(byte[] data, int x, int y);

	private byte[] data;
	public override void _Ready() {
		GD.Print($"Size of a board (in bytes) is {BoardSize()}");
		var size = BoardSize();
		data = new byte[size];
		GD.Print("Allocated byte array");
		InitBoard(data);
		GD.Print("Initialized Board struct as byte array");
		GD.Print($"Board[3, 4] = {xAt(data, 3, 4)}");
	}
}
