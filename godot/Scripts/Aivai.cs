using Godot;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public partial class Aivai : Control
{
	private static string BASE_URL = "https://softserve.harding.edu/aivai/";
	private static int TEST_IT = 5;
	private static System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
	private const string PLAYER_NAME = "atat";
	private const string PLAYER_TOKEN = "ufSfW8lxPhirMGdwABK9ubvIaXW2KSBfAjk78ZgQmR8";
	private const string EVENT = "mirror";
	private const bool INF_LOOP = true;
	// Called when the node enters the scene tree for the first time.
	public async override void _Ready()
	{
		string stateUrl = BASE_URL + "play-state";

		for (int i = 0; i < TEST_IT; i++)
		//while (INF_LOOP)
		{
			var getStateObj = new
			{
				@event = EVENT,
				player = PLAYER_NAME,
				token = PLAYER_TOKEN
			};

			// Get the state
			GD.Print("Getting state...");
			HttpResponseMessage stateReponse = await ApiPostAsync(stateUrl, getStateObj);

			if ((int)stateReponse.StatusCode == 204)
			{
				GD.Print("204: Sleep 2 seconds and querey again...\n");
				await Task.Delay(2000);
			}
			else if (!stateReponse.IsSuccessStatusCode)
			{
				GD.Print($"Request failed: {(int)stateReponse.StatusCode} {stateReponse.ReasonPhrase}"); 
			}
			else
			{
				string stateData = await stateReponse.Content.ReadAsStringAsync();
				GD.Print($"- State response: {stateData}");

				var doc = JsonDocument.Parse(stateData);
				var root = doc.RootElement;

				int action_id = root.GetProperty("action_id").GetInt32();
				string state = root.GetProperty("state").GetString();

				char[] cState = state.ToCharArray();

				Board board = Board.Parse(cState);
				

				var board_copy = board.Clone();
				var emk = board.ExpectedMoveKind();
				Player player = new AIPlayer();

				if (emk == 0) {
					await player.PlaceTile(board_copy);
				} else if (emk == 1) {
					await player.PlaceInitialStack(board_copy);
				} else if (emk == 2) {
					await player.MoveTokens(board_copy);
				}

				string action = Board.DiffAsString(board, board_copy);

				var actionObj = new
				{
					player = PLAYER_NAME,
					token = PLAYER_TOKEN,
					action_id = action_id,
					action = action
				};

				// submit the move action
				GD.Print($"Sending \"{action}\"");
				string actionUrl = BASE_URL + "submit-action";
				HttpResponseMessage actionResponse = await ApiPostAsync(actionUrl, actionObj);

				string actionData = await actionResponse.Content.ReadAsStringAsync();
				GD.Print($"- Action response: {actionData}");

				GD.Print("Sleeping 1 second...\n");
				await Task.Delay(1000);
			}
		}
	}

	private static async Task<HttpResponseMessage> ApiPostAsync(string url, object payload)
	{
		var json = JsonSerializer.Serialize(payload);
		var content = new StringContent(json, Encoding.UTF8, "application/json");

		Uri uri = new Uri(url);
		HttpResponseMessage response = await client.PostAsync(uri, content);
		return response;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void _on_back_pressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/menu.tscn");
	}
}
