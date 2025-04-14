
using System.Runtime.InteropServices;
using Godot;

public interface Display {
	void DrawBoard(Board board);
	void DeclareWinner(string name);
	void DeclareTie();
	void ExitPremature();
}

public class TuiDisplay : Display {
	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static void xDrawBoard(byte[] data);
	
	public void DrawBoard(Board board) {
		xDrawBoard(board.data);
	}
	public void DeclareWinner(string name) {
		GD.Print($"The winner is {name}");
	}
	public void DeclareTie() {
		GD.Print("The game is tied");
	}
	public void ExitPremature() {}
}
