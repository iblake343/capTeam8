extends Node

@onready var highlights: TileMapLayer = $"../HoverHighlightLayer"
@onready var static_lights: TileMapLayer = $"../HighlightLayer"
@onready var game = $".."
var legal_locations
var is_right_click_held = false

func _ready():
	legal_locations = game.board.LegalInitialStackLocationsGd()
	for loc in legal_locations:
		static_lights.set_cell(loc, 4)
	$"../PlaceChipOverlay/Player1Score".text = str(1)

func _process(_delta):
	highlights.clear()
	# Only update hover if right-click is not held
	if is_right_click_held:
		return  # Skip hover effect if right-click is held

	var mouse_pos = highlights.get_local_mouse_position()
	var tile_pos = highlights.local_to_map(mouse_pos)

	# Select the correct offset pattern based on column parity and current state
	if legal_locations.has(tile_pos):
		highlights.modulate = Color(0, 1, 0, 0.33)
	else:
		highlights.modulate = Color(1, 0, 0, 0.33)
	
	highlights.set_cell(tile_pos, 4, Vector2i(0, 0))  # hover tile

func _input(event):
	if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
		var mouse_pos = highlights.get_local_mouse_position()
		var tile_pos = highlights.local_to_map(mouse_pos)
		
		game.board.PlaceInitialStack(tile_pos)

		# kill self
		queue_free()
	
func _exit_tree():
	highlights.clear()
	static_lights.clear()
