extends Node2D

@onready var tilemaplayer: TileMapLayer = $TileMapLayer
@onready var clownDisk = $"CanvasLayer/HBoxContainer/ClownfishDisk1(1)"
@onready var octopusDisk = $"CanvasLayer/HBoxContainer/OctopusDisk0(1)"
@onready var crabDisk = $"CanvasLayer/HBoxContainer/CrabDisk0(1)"
@onready var sharkDisk = $"CanvasLayer/HBoxContainer/SharkDisk1(1)"
var tracker = false
var is_right_click_held = false  # Track if the right-click is held down
var hovered_tile: Vector2i = Vector2i(-1, -1)  # Track previously hovered tile
var original_tiles: Dictionary = {}  # Stores original tile states
var wantsToPlaceTile = false
var count = 0 #will delete when andrew implement when to go to next state
var placed_positions: Array = []
var turn = true
var player1
var player2

# Hex grid offsets for even and odd column parities (existing offsets)
var pattern_offsets_even_original: Array = [
	Vector2i(0, 0),  # Center
	Vector2i(0, 1),  # Right
	Vector2i(1, 0),  # Left
	Vector2i(1, 1)   # Top-Left
]

var pattern_offsets_odd_original: Array = [
	Vector2i(0, 0),
	Vector2i(0, 1),  # Center
	Vector2i(1, 1),   # Bottom-Left
	Vector2i(1, 2)
]

var pattern_offsets_even_sideways: Array = [
	Vector2i(0, 0),  # Center
	Vector2i(1, 0),  # Right
	Vector2i(-1, 0),  # Left
	Vector2i(0, 1)  # Bottom-Left
]

var pattern_offsets_odd_sideways: Array = [
	Vector2i(0, 0),  # Center
	Vector2i(1, 1),  # Right
	Vector2i(-1, 1),  # Left
	Vector2i(0, 1)  # Bottom-Left
]

var pattern_offsets_even_flipped: Array = [
	Vector2i(0, 0),  # Center
	Vector2i(0, 1),  # Right
	Vector2i(-1, 0),  # Left
	Vector2i(-1, 1)   # Top-Left
]

var pattern_offsets_odd_flipped: Array = [
	Vector2i(0, 0),  # Center
	Vector2i(0, 1),  # Right
	Vector2i(-1, 1),  # Left
	Vector2i(-1, 2)   # Top-Left
]

var pattern_state: int = 0  # Default is the original pattern

func _ready() -> void:
	turn = false
	clownDisk.hide()
	octopusDisk.hide()
	crabDisk.hide()
	sharkDisk.hide()
	if GameBoard.player1 == 0:
		player1 = clownDisk
	elif GameBoard.player1 == 1:
		player1 = crabDisk
	elif GameBoard.player1 == 2:
		player1 = octopusDisk
	elif GameBoard.player1 == 3:
		player1 = sharkDisk
	if GameBoard.player2 == 0:
		player2 = clownDisk
	elif GameBoard.player2 == 1:
		player2 = crabDisk
	elif GameBoard.player2 == 2:
		player2 = octopusDisk
	elif GameBoard.player2 == 3:
		player2 = sharkDisk
	player1.show()
	$"CanvasLayer/HBoxContainer/Label2".text = str("TILES REMAINING: 8")
func _process(delta):
	# Only update hover if right-click is not held
	if is_right_click_held or !wantsToPlaceTile:
		return  # Skip hover effect if right-click is held

	var mouse_pos = tilemaplayer.get_local_mouse_position()
	var tile_pos = tilemaplayer.local_to_map(mouse_pos)

	if tile_pos != hovered_tile or tracker == true:
		tracker = false
		# Restore previous tiles
		for prev_tile in original_tiles.keys():
			tilemaplayer.set_cell(prev_tile, 4, original_tiles[prev_tile])
		original_tiles.clear()

		# Select the correct offset pattern based on column parity and current state
		var pattern_offsets = get_pattern_offsets(tile_pos)

		# Apply the hover effect in the selected pattern
		for offset in pattern_offsets:
			var target_tile = tile_pos + offset
			if not original_tiles.has(target_tile):
				original_tiles[target_tile] = tilemaplayer.get_cell_atlas_coords(target_tile)
				tilemaplayer.set_cell(target_tile, 4, Vector2i(0, 0))  # hover tile

		hovered_tile = tile_pos

func _input(event):
	if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT and wantsToPlaceTile == true:
		wantsToPlaceTile = false
		var mouse_pos = tilemaplayer.get_local_mouse_position()
		var tile_pos = tilemaplayer.local_to_map(mouse_pos)
		

		# Select the correct offset pattern based on column parity and current state
		var pattern_offsets = get_pattern_offsets(tile_pos)
		

		# Make the clicked pattern stay changed
		for offset in pattern_offsets:
			var target_tile = tile_pos + offset
			original_tiles.erase(target_tile)  # Remove from revert list
			# Will pass to game core to validate placement
			tilemaplayer.set_cell(target_tile, 4, Vector2i(0, 0))  # Set permanent change
			placed_positions.append(target_tile)
			GameBoard.add_tile_position(placed_positions)
		$"CanvasLayer/HBoxContainer/Label2".text = str("TILES REMAINING: ")
		$"CanvasLayer/HBoxContainer/Label2".text += str(7-count)
		if turn == true:
			turn = false
			player2.hide()
			player1.show()
		else:
			turn = true
			player1.hide()
			player2.show()
		print("Pattern clicked!")
		count += 1
		if count == 8:
			place_initial_stack()

	# Check if the "R" key is pressed to switch the pattern
	if event is InputEventKey and event.pressed and event.keycode == Key.KEY_R:
		cycle_pattern()
	
	# Right-click logic to hide hover when pressed, restore it when released
	if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_RIGHT:
		if event.pressed:
			# Right-click held, hide hover and clear current hovered tiles
			is_right_click_held = true
			hovered_tile = Vector2i(-1, -1)  # Reset hovered tile
			for prev_tile in original_tiles.keys():
				tilemaplayer.set_cell(prev_tile, 1, original_tiles[prev_tile])
			original_tiles.clear()
		else:
			# Right-click released, restore hover functionality
			is_right_click_held = false

# Function to get the current pattern offsets based on the pattern state and column parity
func get_pattern_offsets(tile_pos: Vector2i) -> Array:
	var pattern_offsets: Array
	if tile_pos.x % 2 == 0:  # Even column
		match pattern_state:
			0: pattern_offsets = pattern_offsets_even_original
			1: pattern_offsets = pattern_offsets_even_sideways
			2: pattern_offsets = pattern_offsets_even_flipped
	else:  # Odd column
		match pattern_state:
			0: pattern_offsets = pattern_offsets_odd_original
			1: pattern_offsets = pattern_offsets_odd_sideways
			2: pattern_offsets = pattern_offsets_odd_flipped
	return pattern_offsets

# Function to cycle through the pattern states
func cycle_pattern() -> void:
	pattern_state = (pattern_state + 1) % 3  # Cycle through 0, 1, 2
	print("Pattern switched to state: ", pattern_state)
	tracker = true

func place_tile() -> void:
	wantsToPlaceTile = true

#calling myself at the moment, will have Andrew pass me when to go
func place_initial_stack():
	get_tree().change_scene_to_file("res://Scenes/place_tiles.tscn")
	
