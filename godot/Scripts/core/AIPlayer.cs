using System;
using Godot;
using System.Threading.Tasks;
using System.Linq;

public class AIPlayer : Player {
	public async Task<int> PlaceTile(Board board) {
		// Assuming board.LegalTileArrangements() returns a list or array of TileArrangement objects
		var options = board.LegalTileArrangements();

		// Simulate placing each tile in each arrangement
		int numSimulations = 100; // Number of simulations to run for each action
		float[] scores = new float[options.Count];

		for (int i = 0; i < options.Count; i++) {
			if (options.Count == 0) return 0;  // No options, so return early to avoid errors
			var option = options[i]; // Access the TileArrangement

			Vector2I loc = option.origin;   // Use 'origin' as the location
			Direction dir = option.orientation;  // Use 'orientation' as the direction

			// Create a copy of the board to simulate on
			Board simBoard = board.Clone();
			simBoard.PlaceTile(loc, dir); // Place the tile at 'loc' in direction 'dir'

			// Simulate the initial stack placement
			PlaceInitialStackSimulation(simBoard);

			// Run the simulation for this option, accumulate the scores
			float totalScore = 0f;
			for (int j = 0; j < numSimulations; j++)
			{
				float score = RunSimulation(simBoard);
				totalScore += score;
				GD.Print($"Simulation {j + 1}: Score = {score} for move at ({loc.X}, {loc.Y}) in direction {dir.Name()}");
			}
			
			// Store the average score for this move
			scores[i] = totalScore / numSimulations;
			GD.Print($"Average score for move at ({loc.X}, {loc.Y}) in direction {dir.Name()} = {scores[i]}");
		}

		// Find the best action based on the simulation results
		float maxScore = scores.Max();
		int bestIndex = Array.IndexOf(scores, maxScore);

		// Get the best tile placement option
		var bestOption = options[bestIndex]; // Get the best option based on the simulation results
		Vector2I bestLoc = bestOption.origin;
		Direction bestDir = bestOption.orientation;

		// Execute the best move
		board.PlaceTile(bestLoc, bestDir);
		GD.Print($"Placed tile at ({bestLoc.X}, {bestLoc.Y}) in direction {bestDir.Name()}");

		return 0; // Return 0 as the placeholder return value
	}


	public void PlaceInitialStackSimulation(Board simBoard)
	{
		var options = simBoard.LegalInitialStackLocations();
		if (options.Count == 0) return;  // Guard against empty options list
		Vector2I bestLoc = options[Random.Shared.Next(options.Count)];
		simBoard.PlaceInitialStack(bestLoc); // Place the initial stack
	}

	public float RunSimulation(Board simBoard)
	{
		int maxSimSteps = 100;  // Limit to prevent infinite games
		int currentPlayer = 1; // Assume 1 is AI, 0 is opponent
		
		// Simulate until the game ends or max steps are reached
		for (int i = 0; i < maxSimSteps; i++) {
			if (currentPlayer == 1) {
				// AI's turn: Random move or Monte Carlo policy for AI
				MoveTokensRandomly(simBoard);
			} else {
				// Opponent's turn: Random move for opponent
				MoveTokensRandomly(simBoard);
			}
			
			// Check for game-ending conditions (e.g., win, loss, or draw)
			int winner = simBoard.Winner();
			if (winner != 0) {
				return (winner == 1) ? 1.0f : 0.0f;  // 1 if AI wins, 0 if opponent wins
			}
			
			// Switch turn
			currentPlayer = 1 - currentPlayer; 
		}
		
		// If game ends after max steps, return a neutral score (e.g., 0.5f for draw)
		return 0.5f;
	}

	public void MoveTokensRandomly(Board simBoard)
	{
		var options = simBoard.LegalStartStacks();
		if (options.Count == 0) return;  // Guard against empty options list
		var loc = options[Random.Shared.Next(options.Count)];
		
		var dest_options = simBoard.LegalDestLocations(loc);
		if (dest_options.Count == 0) return;  // Guard against empty dest options list
		var dest = dest_options[Random.Shared.Next(dest_options.Count)];
		
		var stack_size = simBoard.At(loc).count;
		if (stack_size < 2) return; // No move possible
		
		var amt = Random.Shared.Next(1, stack_size);
		simBoard.MoveTokens(loc, dest, amt);
	}

	public async Task<int> PlaceInitialStack(Board board)
	{
		// Get all legal initial stack locations
		var options = board.LegalInitialStackLocations();

		// Number of simulations to run for each stack placement
		int numSimulations = 100;
		float[] scores = new float[options.Count];

		// Simulate placing the initial stack at each possible location
		for (int i = 0; i < options.Count; i++) {
			Vector2I loc = options[i];

			// Create a copy of the board to simulate on (deep copy)
			Board simBoard = board.Clone();

			// Place the initial stack at this location
			simBoard.PlaceInitialStack(loc);

			// Run multiple simulations and accumulate the results
			float totalScore = 0f;
			for (int j = 0; j < numSimulations; j++)
			{
				totalScore += RunSimulation2(simBoard);  // Run simulation and accumulate score
			}

			// Store the average score for this move
			scores[i] = totalScore / numSimulations;
		}

		// Find the best initial stack location based on simulation results
		float maxScore = scores.Max();
		int bestIndex = Array.IndexOf(scores, maxScore);

		Vector2I bestLoc = options[bestIndex];
		board.PlaceInitialStack(bestLoc);

		GD.Print($"Placed initial stack at ({bestLoc.X}, {bestLoc.Y})");

		return 0;
	}
	public float RunSimulation2(Board simBoard)
	{
		int maxSimSteps = 100;  // Limit to prevent infinite games
		int currentPlayer = 1;  // Assume 1 is AI, 0 is opponent

		// Simulate until the game ends or max steps are reached
		for (int i = 0; i < maxSimSteps; i++) {
			if (currentPlayer == 1) {
				// AI's turn: Random move or Monte Carlo policy for AI
				MoveTokensRandomly(simBoard);
			} else {
				// Opponent's turn: Random move for opponent
				MoveTokensRandomly(simBoard);
			}

			// Check for game-ending conditions (e.g., win, loss, or draw)
			int winner = simBoard.Winner();
			if (winner != 0) {
				return (winner == 1) ? 1.0f : 0.0f;  // 1 if AI wins, 0 if opponent wins
			}

			// Switch turn
			currentPlayer = 1 - currentPlayer;
		}

		// If game ends after max steps, return a neutral score (e.g., 0.5f for draw)
		return 0.5f;
	}

	public async Task<int> MoveTokens(Board board)
	{
		// Get all legal starting locations for tokens
		var options = board.LegalStartStacks();
		if (options.Count == 0) return 0; // No legal options to move tokens

		int numSimulations = 100; // Number of simulations to run for each move
		float[] scores = new float[options.Count];
		
		// Simulate each possible token move
		for (int i = 0; i < options.Count; i++) {
			Vector2I loc = options[i];
			
			// Get all legal destination locations for tokens from this location
			var dest_options = board.LegalDestLocations(loc);
			
			// Try moving tokens to each possible destination
			foreach (var dest in dest_options) {
				// Create a copy of the board to simulate on (deep copy)
				Board simBoard = board.Clone();
				
				// Get the stack size at the location
				var simStackSize = simBoard.At(loc).count;
				if (simStackSize < 2) continue; // Can't move if there's less than 2 tokens in the stack
				
				// Move a random number of tokens (between 1 and the stack size)
				var amtSim = Random.Shared.Next(1, simStackSize);
				
				// Perform the move
				simBoard.MoveTokens(loc, dest, amtSim);
				
				// Run multiple simulations and accumulate the results
				float totalScore = 0f;
				for (int j = 0; j < numSimulations; j++)
				{
					totalScore += RunSimulation2(simBoard);  // Run simulation and accumulate score
				}

				// Store the average score for this move
				scores[i] = totalScore / numSimulations;
			}
		}
		
		// Find the best move based on the simulation results
		float maxScore = scores.Max();
		int bestIndex = Array.IndexOf(scores, maxScore);
		
		// Get the best location and destination for the token move
		Vector2I bestLoc = options[bestIndex];
		var bestDestOptions = board.LegalDestLocations(bestLoc);
		Vector2I bestDest = bestDestOptions[Random.Shared.Next(bestDestOptions.Count)];
		
		// Move the tokens to the best destination
		var stack_size = board.At(bestLoc).count;
		var amt = Random.Shared.Next(1, stack_size);
		board.MoveTokens(bestLoc, bestDest, amt);
		
		GD.Print($"Moved {amt} tokens from ({bestLoc.X}, {bestLoc.Y}) to ({bestDest.X}, {bestDest.Y})");

		return 0;
	}
}
