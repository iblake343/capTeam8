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
		//GD.Print($"Placed tile at ({loc.X}, {loc.Y}) in the {dir.NameNorthMajorAskew()} direction");
		return 0;
	}
	
	public async Task<int> PlaceInitialStack(Board board) {
		var options = board.LegalInitialStackLocations();
		Vector2I center = board.Center(); // midpoint of min/max frame

		int bestScore = int.MinValue;
		Vector2I bestLoc = options[0];

		int aiPlayer = board.CurrentPlayer();
		bool aiIsSecond = aiPlayer == 1;

		foreach (var loc in options) {
			// Clone and simulate placing the AI's initial stack at this location
			var simBoard = board.Clone();
			simBoard.PlaceInitialStack(loc);

			int reachability = CountReachableEmptyTiles(simBoard, loc, 3);
			int distFromCenter = Math.Abs(loc.X - center.X) + Math.Abs(loc.Y - center.Y);
			int neighborBonus = CountImmediateEmptyNeighbors(simBoard, loc);
			

			int score =
				(reachability * 10) +
				(neighborBonus * 5) -
				(distFromCenter * 2);
				

			if (aiIsSecond) {
				int worstOpponentScore = int.MinValue;

				foreach (var oppLoc in simBoard.LegalInitialStackLocations()) {
					var oppBoard = simBoard.Clone();
					oppBoard.PlaceInitialStack(oppLoc);

					int opponentTiles = CountTilesControlled(oppBoard, 1 - aiPlayer);
					int centerBonus = CenterControlBonus(oppBoard, 1 - aiPlayer);

					int oppScore = (opponentTiles * 12) + (centerBonus * 2);
					if (oppScore > worstOpponentScore)
						worstOpponentScore = oppScore;
				}

				score -= worstOpponentScore;
				//GD.Print($"⚠️ Going second. Simulated worst-case opponent score: -{worstOpponentScore}");
			}


			//GD.Print($"📍 {loc} — reach: {reachability}, neighbors: {neighborBonus}, dist: {distFromCenter} => score {score}");

			if (score > bestScore) {
				bestScore = score;
				bestLoc = loc;
			}
		}

		board.PlaceInitialStack(bestLoc);
		//GD.Print($"✅ Chose initial stack at {bestLoc} with score {bestScore}");
		return 0;
	}
	private int CountTilesControlled(Board board, int player) {
		int count = 0;
		var frame = board.Frame();

		for (int x = frame.min.X; x <= frame.max.X; x++) {
			for (int y = frame.min.Y; y <= frame.max.Y; y++) {
				var cell = board.At(new Vector2I(x, y));
				if ((int)cell.color == player && cell.count > 0)
					count++;
			}
		}
		return count;
	}

	private int CenterControlBonus(Board board, int player) {
		var frame = board.Frame();
		Vector2I center = new((frame.min.X + frame.max.X) / 2, (frame.min.Y + frame.max.Y) / 2);
		int bonus = 0;

		for (int x = frame.min.X; x <= frame.max.X; x++) {
			for (int y = frame.min.Y; y <= frame.max.Y; y++) {
				var loc = new Vector2I(x, y);
				var cell = board.At(loc);
				if ((int)cell.color != player || cell.count == 0) continue;

				int dist = Math.Abs(loc.X - center.X) + Math.Abs(loc.Y - center.Y);
				bonus += Math.Max(0, 5 - dist);
			}
		}
		return bonus;
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

		int currentPlayer = board.CurrentPlayer();
		int bestScore = int.MinValue;

		Vector2I bestSrc = new();
		Vector2I bestDest = new();
		int bestAmt = 1;
		int bestBias = int.MaxValue;

		int moveCounter = 0;

		foreach (var loc in options) {
			var destOptions = board.LegalDestLocations(loc);
			int stackSize = board.At(loc).count;

			foreach (var dest in destOptions) {
				int[] splitOptions = { 1, stackSize / 2, stackSize - 1 };

				foreach (int amt in splitOptions.Distinct()) {
					moveCounter++;

					var simBoard = board.Clone();
					simBoard.MoveTokens(loc, dest, amt);

					int score = AlphaBeta(simBoard, 2, int.MinValue, int.MaxValue, false, currentPlayer);
					int splitBias = Math.Abs((stackSize / 2) - amt);

					//GD.Print($"#{moveCounter}: {amt} tokens from ({loc.X},{loc.Y}) → ({dest.X},{dest.Y}) | Score: {score}");

					// Prefer higher score, then more balanced splits
					if (simBoard.LegalDestLocations(dest).Count == 0 && (stackSize - amt) > 1)
						score -= (stackSize - amt) * 5;

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

		//GD.Print($"✅ BEST MOVE: {bestAmt} tokens from ({bestSrc.X},{bestSrc.Y}) → ({bestDest.X},{bestDest.Y}) | Final Score: {bestScore}");
		board.MoveTokens(bestSrc, bestDest, bestAmt);
		return 0;
	}


	private int AlphaBeta(Board board, int depth, int alpha, int beta, bool maximizing, int aiPlayer) {
		if (depth == 0 || board.ExpectedMoveKind() == -1 || 
			board.CountStacks(aiPlayer) == 0 || board.CountStacks(1 - aiPlayer) == 0) {
			return Evaluate(board, aiPlayer);
		}
		//GD.Print($"Depth: {depth}");
		var options = board.LegalStartStacks();
		if (options.Count == 0) {
			return Evaluate(board, aiPlayer);
		}

		if (maximizing) {
			int maxEval = int.MinValue;
			bool anyMoveMade = false;

			foreach (var loc in options.Where(loc => (int)board.At(loc).color == aiPlayer)) {
				var destOptions = board.LegalDestLocations(loc);
				int stackSize = board.At(loc).count;

				foreach (var dest in destOptions) {
					bool pruned = false;

					int[] splitOptions = { 1, stackSize / 2, stackSize - 1 };

					foreach (int amt in splitOptions.Distinct()) {
						var simBoard = board.Clone();
						simBoard.MoveTokens(loc, dest, amt);

						int eval = AlphaBeta(simBoard, depth - 1, alpha, beta, false, aiPlayer);

						maxEval = Math.Max(maxEval, eval);
						alpha = Math.Max(alpha, eval);
						anyMoveMade = true;

						if (beta <= alpha) {
							pruned = true;
							break;
						}
					}
					if (pruned) break;
				}
				if (beta <= alpha) break;
			}

			return anyMoveMade ? maxEval : Evaluate(board, aiPlayer);
		}
		else {
			int minEval = int.MaxValue;
			bool anyMoveMade = false;

			foreach (var loc in options.Where(loc => (int)board.At(loc).color == 1 - aiPlayer)) {
				var destOptions = board.LegalDestLocations(loc);
				int stackSize = board.At(loc).count;

				foreach (var dest in destOptions) {
					bool pruned = false;

					int[] splitOptions = { 1, stackSize / 2, stackSize - 1 };

					foreach (int amt in splitOptions.Distinct()) {
						var simBoard = board.Clone();
						simBoard.MoveTokens(loc, dest, amt);

						int eval = AlphaBeta(simBoard, depth - 1, alpha, beta, true, aiPlayer);

						minEval = Math.Min(minEval, eval);
						beta = Math.Min(beta, eval);
						anyMoveMade = true;

						if (beta <= alpha) {
							pruned = true;
							break;
						}
					}
					if (pruned) break;
				}
				if (beta <= alpha) break;
			}

			return anyMoveMade ? minEval : Evaluate(board, aiPlayer);
		}

	}

	private int Evaluate(Board board, int aiPlayer) {
		int opponent = 1 - aiPlayer;
		var frame = board.Frame();
		Vector2I center = new((frame.min.X + frame.max.X) / 2, (frame.min.Y + frame.max.Y) / 2);

		int aiTilesControlled = 0;
		int opponentTilesControlled = 0;
		int aiStacks = 0;
		int opponentStacks = 0;
		int aiCenterBonus = 0;
		int opponentCenterBonus = 0;
		int aiMobility = 0;
		int opponentMobility = 0;
		int aiStranded = 0;
		int opponentStranded = 0;
		int aiReachable = 0;
		int opponentReachable = 0;

		var allLocs = new List<Vector2I>();

		for (int x = frame.min.X; x <= frame.max.X; x++) {
			for (int y = frame.min.Y; y <= frame.max.Y; y++) {
				var loc = new Vector2I(x, y);
				allLocs.Add(loc);

				var cell = board.At(loc);
				if (cell.count <= 0) continue;

				int dist = Math.Abs(loc.X - center.X) + Math.Abs(loc.Y - center.Y);
				int centerBonus = Math.Max(0, 5 - dist); // 0–5 based on distance

				// Only count reachability for stacks
				if ((int)cell.color == aiPlayer && cell.count > 0) {
					aiTilesControlled++;
					if (cell.count > 1) aiStacks++;
					aiCenterBonus += centerBonus;

					if (board.LegalDestLocations(loc).Count == 0 && cell.count > 1)
						aiStranded += (cell.count - 1);

					if (cell.count > 1) {
						aiReachable += CountReachableTilesViaStraightLines(board, loc);
					}
				} else if (cell.count > 0) {
					opponentTilesControlled++;
					if (cell.count > 1) opponentStacks++;
					opponentCenterBonus += centerBonus;

					if (board.LegalDestLocations(loc).Count == 0 && cell.count > 1)
						opponentStranded += (cell.count - 1);

					if (cell.count > 1) {
						opponentReachable += CountReachableTilesViaStraightLines(board, loc);
					}
				}
			}
		}

		aiMobility = board.LegalStartStacks().Count(s => (int)board.At(s).color == aiPlayer);
		opponentMobility = board.LegalStartStacks().Count(s => (int)board.At(s).color == opponent);

		int aiContiguous = board.CountContiguousStacks(aiPlayer);
		int opponentContiguous = board.CountContiguousStacks(opponent);

		// --- Scoring components ---
		int score = 0;

		score += 10 * (aiTilesControlled - opponentTilesControlled);
		score += 2  * (aiStacks - opponentStacks);          // Number of active stacks
		score += 5  * (aiMobility - opponentMobility);      // Move flexibility
		score += 3  * (aiContiguous - opponentContiguous);  // Region unity
		score += 6  * (aiCenterBonus - opponentCenterBonus); // Positioning
		score += 4  * (aiReachable - opponentReachable);    // Reach-based potential

		int isLateGame = (aiTilesControlled + opponentTilesControlled) > 16 ? 1 : 0;
		score -= (isLateGame == 1 ? 10 : 3) * aiStranded;

		return score;
	}

	private int CountReachableTilesViaStraightLines(Board board, Vector2I start) {
		int count = 0;
		var visited = new HashSet<Vector2I>();

		for (int i = 0; i < 6; i++) {
			Direction dir = (Direction)i;
			Vector2I probe = start + dir.Vector();

			while (true) {
				var cell = board.At(probe);
				if (cell.count != 0) break;
				if (!visited.Add(probe)) break;

				count++;
				probe += dir.Vector();
			}
		}

		return count;
	}
}
