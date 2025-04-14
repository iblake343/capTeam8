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
 
 public async Task<int> MoveTokens(Board board)
 {
	var options = board.LegalInitialStackLocations();
	Vector2I center = board.Center(); // midpoint of min/max frame

	int bestScore = int.MinValue;
	Vector2I bestLoc = options[0];

	foreach (var loc in options) {
		int reachability = CountReachableEmptyTiles(board, loc, 3); // in 3 moves
		int distFromCenter = Math.Abs(loc.X - center.X) + Math.Abs(loc.Y - center.Y);
		int neighborBonus = CountImmediateEmptyNeighbors(board, loc);

		int score = (reachability * 10) + (neighborBonus * 5) - (distFromCenter * 2);

		GD.Print($"📍 {loc} — reach: {reachability}, neighbors: {neighborBonus}, dist: {distFromCenter} => score {score}");

		if (score > bestScore) {
			bestScore = score;
			bestLoc = loc;
		}
	}

	board.PlaceInitialStack(bestLoc);
	GD.Print($"✅ Chose initial stack at {bestLoc} with score {bestScore}");
	return 0;
}
private int CountReachableEmptyTiles(Board board, Vector2I start, int maxMoves) {
	var visited = new HashSet<Vector2I>();
	var queue = new Queue<(Vector2I pos, int depth)>();

	visited.Add(start);
	queue.Enqueue((start, 0));
	int count = 0;

	while (queue.Count > 0) {
		var (current, depth) = queue.Dequeue();

		if (depth >= maxMoves) continue;

		for (int i = 0; i < 6; i++) {
			Vector2I neighbor = current + ((Direction)i).Vector();

			if (visited.Contains(neighbor)) continue;

			var cell = board.At(neighbor);
			if (cell.count != 0) continue; // not an empty tile

			visited.Add(neighbor);
			queue.Enqueue((neighbor, depth + 1));
			count++;
		}
	}

	return count;
}
private int CountImmediateEmptyNeighbors(Board board, Vector2I loc) {
	int count = 0;
	for (int i = 0; i < 6; i++) {
		Vector2I neighbor = loc + ((Direction)i).Vector();
		if (board.At(neighbor).count == 0)
			count++;
	}
	return count;
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

					// After the AI moves, simulate the opponent's best response
					int opponentEval = SimulateOpponentMove(simBoard, aiPlayer);

					// Combine AI score and opponent's response
					int eval = AlphaBeta(simBoard, depth - 1, alpha, beta, false, aiPlayer) - opponentEval;

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

					// After the AI moves, simulate the opponent's best response
					int opponentEval = SimulateOpponentMove(simBoard, aiPlayer);

					// Combine AI score and opponent's response
					int eval = AlphaBeta(simBoard, depth - 1, alpha, beta, true, aiPlayer) + opponentEval;

					minEval = Math.Min(minEval, eval);
					beta = Math.Min(beta, eval);
					if (beta <= alpha) break;
				}
			}
		}
		return minEval;
	}
}
private int SimulateOpponentMove(Board simBoard, int aiPlayer) {
	int opponent = 1 - aiPlayer;  // The opponent is always the opposite player
	
	// Get the best possible move for the opponent using a simplified evaluation
	int bestOpponentScore = int.MinValue;

	var opponentOptions = simBoard.LegalStartStacks();
	foreach (var loc in opponentOptions) {
		var destOptions = simBoard.LegalDestLocations(loc);
		int stackSize = simBoard.At(loc).count;

		foreach (var dest in destOptions) {
			for (int amt = 1; amt < stackSize; amt++) {
				var opponentSimBoard = simBoard.Clone();
				opponentSimBoard.MoveTokens(loc, dest, amt);

				// Evaluate the position after the opponent's move
				int opponentEval = Evaluate(opponentSimBoard, opponent);

				bestOpponentScore = Math.Max(bestOpponentScore, opponentEval);
			}
		}
	}

	// Return the opponent's best score
	return bestOpponentScore;
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
			strandedPenalty += 2 * cell.count - 1;
		}
	}
}

score -= 10 * strandedPenalty; // Tweak multiplier as needed

//Add area control awareness
score += AreaControlBonus(board, aiPlayer);

	return score;
}
private int AreaControlBonus(Board board, int aiPlayer) {
	var visited = new HashSet<Vector2I>();
	int bonus = 0;
	var frame = board.Frame();

	for (int x = frame.min.X; x <= frame.max.X; x++) {
		for (int y = frame.min.Y; y <= frame.max.Y; y++) {
			var start = new Vector2I(x, y);
			if (visited.Contains(start)) continue;

			var cell = board.At(start);
			if (cell.count != 0) continue; // not empty

			// Start flood fill of an empty region
			var region = FloodFillEmptyRegion(board, start, visited);
			int size = region.Count;

			var (canReachAI, canReachOpponent) = TokensThatCanReach(board, region, aiPlayer);
			int tokenCount = CountTokensInRegion(board, region, aiPlayer);

			if (canReachAI && !canReachOpponent) {
				int regionBonus = tokenCount * 3;
				if (tokenCount == size) regionBonus += size >= 6 ? 20 : 10;

				GD.Print($"✅ AI-only region (size: {size}), tokens: {tokenCount}, bonus: {regionBonus}");
				bonus += regionBonus;
			}
			else if (canReachAI && canReachOpponent) {
				GD.Print($"⚖️ Shared region (size: {size}) — no bonus");
			}
			else if (!canReachAI && canReachOpponent) {
				GD.Print($"🚫 Opponent-only region (size: {size}) — skipping");
			}
			else {
				GD.Print($"🟨 Inaccessible region (size: {size}) — skipping");
			}
		}
	}

	GD.Print($"🧠 Total area control bonus: {bonus}");
	return bonus;
}

private HashSet<Vector2I> FloodFillEmptyRegion(Board board, Vector2I start, HashSet<Vector2I> visited) {
	var region = new HashSet<Vector2I>();
	var queue = new Queue<Vector2I>();

	queue.Enqueue(start);
	visited.Add(start);
	region.Add(start);

	while (queue.Count > 0) {
		var current = queue.Dequeue();

		for (int i = 0; i < 6; i++) {
			Vector2I neighbor = current + ((Direction)i).Vector();

			// Skip if already visited
			if (visited.Contains(neighbor)) continue;

			// Check if it's empty
			var cell = board.At(neighbor);
			if (cell.count != 0) continue; // not an empty tile

			// Add to region and continue search
			visited.Add(neighbor);
			region.Add(neighbor);
			queue.Enqueue(neighbor);
		}
	}

	// Debug output to check regions
	GD.Print($"Flood filled region starting from {start}, total size: {region.Count}");

	return region;
}
private (bool canReachAI, bool canReachOpponent) TokensThatCanReach(Board board, HashSet<Vector2I> region, int aiPlayer) {
	bool canReachAI = false;
	bool canReachOpponent = false;

	for (int i = 0; i < 2; i++) {
		if ((i == aiPlayer && canReachAI) || (i != aiPlayer && canReachOpponent)) {
			continue;
		}

		var stacks = board.LegalStartStacks();
		foreach (var loc in stacks) {
			var cell = board.At(loc);
			if ((int)cell.color != i || cell.count == 0) continue;

			foreach (var dest in board.LegalDestLocations(loc)) {
				if (region.Contains(dest)) {
					// Debug output to see if the AI or opponent can reach a region
					GD.Print($"🧭 Player {i} can reach region from {loc} to {dest}");

					if (i == aiPlayer) canReachAI = true;
					else canReachOpponent = true;
					break;
				}
			}

			if ((i == aiPlayer && canReachAI) || (i != aiPlayer && canReachOpponent)) {
				break;
			}
		}

		if (i == aiPlayer && !canReachAI)
			GD.Print($"❌ AI player {i} cannot reach region");
		if (i != aiPlayer && !canReachOpponent)
			GD.Print($"❌ Opponent player {i} cannot reach region");
	}

	// Print final values to verify
	GD.Print($"canReachAI: {canReachAI}, canReachOpponent: {canReachOpponent}");
	return (canReachAI, canReachOpponent);
}


private int CountTokensInRegion(Board board, HashSet<Vector2I> region, int aiPlayer) {
	int totalTokens = 0;

	foreach (var loc in region) {
		var cell = board.At(loc);
		if ((int)cell.color == aiPlayer && cell.count > 0) {
			totalTokens += cell.count;
		}
	}

	return totalTokens;
}

}
