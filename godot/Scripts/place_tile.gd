extends Node

@onready var highlights: TileMapLayer = $"../HoverHighlightLayer"
@onready var game = $".."
var is_right_click_held = false
var dir: int = 0  # Default is the original pattern

func _process(_delta):
	highlights.clear()
	# Only update hover if right-click is not held
	if is_right_click_held:
		return  # Skip hover effect if right-click is held

	var mouse_pos = highlights.get_local_mouse_position()
	var tile_pos = highlights.local_to_map(mouse_pos)

	var pattern_locs = game.board.GetLocsFromOriginAndDir(tile_pos, dir)
	if game.board.IsLegalTilePlacement(tile_pos, dir):
		highlights.modulate = Color(0, 1, 0, 0.33)
	else:
		highlights.modulate = Color(1, 0, 0, 0.33)
	
	# Apply the hover effect in the selected pattern
	for target_tile in pattern_locs:
		highlights.set_cell(target_tile, 4, Vector2i(0, 0))  # hover tile

func _input(event):
	if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
		var mouse_pos = highlights.get_local_mouse_position()
		var tile_pos = highlights.local_to_map(mouse_pos)
		game.board.PlaceTile(tile_pos, dir)
		 # Will need to decide how to keep up with how many tiles have been placed and by who
		# kill self
		queue_free()

	# Check if the "R" key is pressed to switch the pattern
	if event is InputEventKey and event.pressed and event.keycode == Key.KEY_R:
		cycle_pattern()
	
func _exit_tree():
	highlights.clear()

# Function to cycle through the pattern states
func cycle_pattern() -> void:
	dir = (dir + 1) % 6  # Cycle through 0, 1, 2
	#tracker = true
