extends Node

@onready var highlights: TileMapLayer = $"../HoverHighlightLayer"
@onready var static_lights: TileMapLayer = $"../HighlightLayer"
@onready var numbers: TileMapLayer = $"../NumberLayer"
@onready var game = $".."
var legal_locations
var is_right_click_held = false
var state = 0
var start_loc
var end_loc
var amount = 0
var max_amount = 0

func _ready():
	legal_locations = game.board.LegalStartStacks()
	for loc in legal_locations:
		static_lights.set_cell(loc, 4, Vector2i(0, 0))

func _process(_delta):
	highlights.clear()
	# Only update hover if right-click is not held
	if is_right_click_held:
		return  # Skip hover effect if right-click is held

	var mouse_pos = highlights.get_local_mouse_position()
	var tile_pos = highlights.local_to_map(mouse_pos)

	if legal_locations.has(tile_pos):
		highlights.modulate = Color(0, 1, 0, 0.33)
	else:
		highlights.modulate = Color(1, 0, 0, 0.33)
	
	highlights.set_cell(tile_pos, 4, Vector2i(0, 0))  # hover tile
	
	if state == 2:
		numbers.set_cell(end_loc, amount - 1, Vector2i(0, 0))

func _input(event):
	if event is InputEventMouseButton:
		if not event.pressed: return
		if event.button_index == MOUSE_BUTTON_LEFT:
			var mouse_pos = highlights.get_local_mouse_position()
			var tile_pos = highlights.local_to_map(mouse_pos)
			
			if !legal_locations.has(tile_pos): return
			if state == 0: # pick start location
				state = 1
				start_loc = tile_pos
				legal_locations = game.board.LegalDestLocations(start_loc)
				static_lights.clear()
				for loc in legal_locations:
					static_lights.set_cell(loc, 4, Vector2i(0, 0))
				return
			if state == 1: # pick end location
				state = 2
				end_loc = tile_pos
				max_amount = game.board.At(start_loc).count - 1
				amount = (max_amount + 1) / 2
				legal_locations = [end_loc]
				return
			if state == 2: # complete movement
				game.board.MoveTokens(start_loc, end_loc, amount)
				queue_free()
		if state == 2 and event.button_index == MOUSE_BUTTON_WHEEL_UP:
			amount = min(amount + 1, max_amount)
		if state == 2 and event.button_index == MOUSE_BUTTON_WHEEL_DOWN:
			amount = max(amount - 1, 1)
	if event is InputEventKey and event.pressed:
		if event.keycode in [KEY_W, KEY_UP]:
			amount = min(amount + 1, max_amount)
		if event.keycode in [KEY_S, KEY_DOWN]:
			amount = max(amount - 1, 1)

func _exit_tree():
	highlights.clear()
	static_lights.clear()
