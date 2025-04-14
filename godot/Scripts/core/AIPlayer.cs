using System;
using Godot;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

public class AIPlayer : Player {
	public async Task<int> PlaceTile(Board board) {
		var options = board.LegalTileArrangements();
		var ix = Random.Shared.Next(options.Count);
		var	option = options[ix];
		Vector2I loc = option.origin;
		Direction dir = option.orientation;
		board.PlaceTile(loc, dir);
		GD.Print($"Placed tile at ({loc.X}, {loc.Y}) in the {dir.NameNorthMajorAskew()} direction");
		return 0;
	}
	
	public async Task<int> PlaceInitialStack(Board board) {
		var options = board.LegalInitialStackLocations();
		var ix = Random.Shared.Next(options.Count);
		var loc = options[ix];
		board.PlaceInitialStack(loc);
		GD.Print($"Placed initial stack at ({loc.X}, {loc.Y})");
		return 0;
	}

public async Task<int> MoveTokens(Board board) {
	var options = board.LegalStartStacks();
	if (options.Count == 0) return 0;

	int bestScore = int.MinValue;
	Vector2I bestSrc = new();
	Vector2I bestDest = new();
	int bestAmt = 1;
	int bestBias = int.MaxValue;

	int currentPlayer = board.CurrentPlayer();

	foreach (var loc in options) {
		var destOptions = board.LegalDestLocations(loc);
		int stackSize = board.At(loc).count;

		foreach (var dest in destOptions) {
			for (int amt = 1; amt < stackSize; amt++) {
				var simBoard = board.Clone();
				simBoard.MoveTokens(loc, dest, amt);

				int score = AlphaBeta(simBoard, 7, int.MinValue, int.MaxValue, false, currentPlayer);

				int splitBias = Math.Abs(stackSize / 2 - amt); // closer to half = better
				if (score > bestScore || (score == bestScore && splitBias < bestBias)) {
					bestScore = score;
					bestSrc = loc;
					bestDest = dest;
					bestAmt = amt;
					bestBias = splitBias;
				}
			}
		}
	}

	board.MoveTokens(bestSrc, bestDest, bestAmt);
	GD.Print($"Moved {bestAmt} tokens from ({bestSrc.X}, {bestSrc.Y}) to ({bestDest.X}, {bestDest.Y}) [score: {bestScore}]");
	return 0;
}

private int AlphaBeta(Board board, int depth, int alpha, int beta, bool maximizing, int aiPlayer) {
	if (depth == 0 || board.CountStacks(aiPlayer) == 0 || board.CountStacks(1 - aiPlayer) == 0 || board.Winner() != -1) {
		return Evaluate(board, aiPlayer);
	}

	var options = board.LegalStartStacks();
	if (options.Count == 0) {
		return Evaluate(board, aiPlayer); // No more moves
	}

	if (maximizing) {
		int maxEval = int.MinValue;
		foreach (var loc in options) {
			var destOptions = board.LegalDestLocations(loc);
			int stackSize = board.At(loc).count;

			foreach (var dest in destOptions) {
				for (int amt = 1; amt < stackSize; amt++) {
					var simBoard = board.Clone();
					simBoard.MoveTokens(loc, dest, amt);
					int eval = AlphaBeta(simBoard, depth - 1, alpha, beta, false, aiPlayer);
					maxEval = Math.Max(maxEval, eval);
					alpha = Math.Max(alpha, eval);
					if (beta <= alpha) break;
				}
			}
		}
		return maxEval;
	} else {
		int minEval = int.MaxValue;
		foreach (var loc in options) {
			var destOptions = board.LegalDestLocations(loc);
			int stackSize = board.At(loc).count;

			foreach (var dest in destOptions) {
				for (int amt = 1; amt < stackSize; amt++) {
					var simBoard = board.Clone();
					simBoard.MoveTokens(loc, dest, amt);
					int eval = AlphaBeta(simBoard, depth - 1, alpha, beta, true, aiPlayer);
					minEval = Math.Min(minEval, eval);
					beta = Math.Min(beta, eval);
					if (beta <= alpha) break;
				}
			}
		}
		return minEval;
	}
}
private int Evaluate(Board board, int aiPlayer) {
	int opponent = 1 - aiPlayer;

	// Basic pasture control
	int aiScore = 0;
	int opponentScore = 0;

	var frame = board.Frame();
	for (int x = frame.min.X; x <= frame.max.X; x++) {
		for (int y = frame.min.Y; y <= frame.max.Y; y++) {
			var cell = board.At(new Vector2I(x, y));
			if (cell.count <= 0) continue;

			if ((int)cell.color == aiPlayer) {
				aiScore++;
			} else {
				opponentScore++;
			}
		}
	}

	// New strategic metrics
	int aiStacks = board.CountStacks(aiPlayer);
	int opponentStacks = board.CountStacks(opponent);

	int aiContiguous = board.CountContiguousStacks(aiPlayer);
	int opponentContiguous = board.CountContiguousStacks(opponent);

	// Weighted score components
	int score = 0;
	score += 15 * (aiScore - opponentScore); // pasture control
	score += 10 * (aiStacks - opponentStacks); // # of movable stacks
	score += 3 * (aiContiguous - opponentContiguous); // connected regions
	
	// Bonus for owning central tiles
int centerBonus = 0;
Vector2I center = new(
	(frame.min.X + frame.max.X) / 2,
	(frame.min.Y + frame.max.Y) / 2
);

for (int x = frame.min.X; x <= frame.max.X; x++) {
	for (int y = frame.min.Y; y <= frame.max.Y; y++) {
		var loc = new Vector2I(x, y);
		var cell = board.At(loc);
		if (cell.count <= 0) continue;

		int dist = Math.Abs(center.X - x) + Math.Abs(center.Y - y);
		int bonus = Math.Max(0, 5 - dist); // closer to center = better

		if ((int)cell.color == aiPlayer) {
			centerBonus += bonus;
		} else {
			centerBonus -= bonus;
		}
	}
}
score += centerBonus;
// Encourage more legal moves
int mobility = board.LegalStartStacks().Count;
score += mobility;

// Penalize stranded stacks with multiple tokens
int strandedPenalty = 0;

for (int x = frame.min.X; x <= frame.max.X; x++) {
	for (int y = frame.min.Y; y <= frame.max.Y; y++) {
		var loc = new Vector2I(x, y);
		var cell = board.At(loc);

		if ((int)cell.color != aiPlayer || cell.count <= 1)
			continue; // Skip non-AI or singleton stacks

		var destOptions = board.LegalDestLocations(loc);
		if (destOptions.Count == 0) {
			// Apply penalty for each additional stranded token
			strandedPenalty += cell.count - 1;
		}
	}
}

score -= 10 * strandedPenalty; // Tweak multiplier as needed


	return score;
}



}
