
public interface Display {
	void drawBoard(Board board);
}

public class EmptyDisplay : Display {
	public void drawBoard(Board _board) {
		Console.WriteLine("drawing board!");
	}
}
