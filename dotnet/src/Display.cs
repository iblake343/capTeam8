using System.Runtime.InteropServices;

public interface Display {
	void DrawBoard(Board board);
	void DeclareWinner(string name);
	void DeclareTie();
}

public class TuiDisplay : Display {
	[DllImport("core.dll", CallingConvention = CallingConvention.Cdecl)]
	private extern static void xDrawBoard(byte[] data);
	
	public void DrawBoard(Board board) {
	    xDrawBoard(board.data);
	}
	public void DeclareWinner(string name) {
		Console.WriteLine($"The winner is {name}");
	}
	public void DeclareTie() {
		Console.WriteLine("The game is tied");
	}
}
