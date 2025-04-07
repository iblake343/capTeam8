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
            var option = options[i]; // Access the TileArrangement (assuming it has 'origin' and 'orientation')
            Vector2I loc = option.origin;   // Use 'origin' as the location
            Direction dir = option.orientation;  // Use 'orientation' as the direction

            // Create a copy of the board to simulate on (deep copy)
            Board simBoard = board.Clone();
            simBoard.PlaceTile(loc, dir); // Place the tile at 'loc' in direction 'dir'

            // Simulate the initial stack placement
            PlaceInitialStackSimulation(simBoard);

            // Run the simulation from this point onward
            scores[i] = RunSimulation(simBoard);
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
        var loc = options[Random.Shared.Next(options.Count)];
        
        var dest_options = simBoard.LegalDestLocations(loc);
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

            // Run the simulation from this point onward
            scores[i] = RunSimulation2(simBoard);
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
                
                // Run the simulation from this point onward
                scores[i] = RunSimulation2(simBoard);
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