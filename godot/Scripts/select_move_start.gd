extends Node

@onready var highlights: TileMapLayer = $"/root/Game/Center/HoverHighlightLayer"
@onready var static_lights: TileMapLayer = $"/root/Game/Center/HighlightLayer"
@onready var numbers: TileMapLayer = $"/root/Game/Center/NumberLayer"
@onready var game = $"/root/Game"
@onready var overlay = $"/root/Game/MovementOverlay"
@onready var button = $"/root/Game/MovementOverlay/FinishButton"
@onready var cancel_button = $"/root/Game/MovementOverlay/CancelButton"
@onready var plus_button = $"/root/Game/MovementOverlay/PlusButton"
@onready var minus_button = $"/root/Game/MovementOverlay/MinusButton"
var legal_locations
var is_right_click_held = false
var state = 0
var start_loc
var end_loc
var amount = 0
var max_amount = 0

func _ready():
	if button:
		button.connect("pressed", Callable(self, "_on_finish_button_pressed"))
		print("Connected finish button.")
	else:
		print("Finish button was null!")
	if cancel_button:
		cancel_button.connect("pressed", Callable(self, "_on_cancel_button_pressed"))
	if plus_button:
		plus_button.connect("pressed", Callable(self, "_on_plus_button_pressed"))
	if minus_button:
		minus_button.connect("pressed", Callable(self, "_on_minus_button_pressed"))
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
	
	if state != 2:
		highlights.set_cell(tile_pos, 4, Vector2i(0, 0))  # hover tile
	
	if state == 2:
		numbers.set_cell(end_loc, amount - 1, Vector2i(0, 0))

func _unhandled_input(event):
	if event is InputEventMouseButton:
		if not event.pressed: return

		if event.button_index == MOUSE_BUTTON_LEFT:
			var mouse_pos = highlights.get_local_mouse_position()
			var tile_pos = highlights.local_to_map(mouse_pos)
			var player = game.board.CurrentPlayer()
			var cell = game.board.At(tile_pos);
			if state == 0: # pick start location
				state = 1
				start_loc = tile_pos
				if cell.color == player:
					legal_locations = game.board.LegalDestLocations(start_loc)
					static_lights.clear()
					for loc in legal_locations:
						static_lights.set_cell(loc, 4, Vector2i(0, 0))
				return

			if state == 1: # pick end location 
				if tile_pos == start_loc:
					return
				if cell.count > 1 and cell.color == player:
					print("Restarting from a new starting tile.")
					start_loc = tile_pos
					legal_locations = game.board.LegalDestLocations(start_loc)
					static_lights.clear()
					for loc in legal_locations:
						static_lights.set_cell(loc, 4, Vector2i(0, 0))
					return
				if legal_locations.has(tile_pos):
					state = 2
					end_loc = tile_pos
					max_amount = game.board.At(start_loc).count - 1
					amount = (max_amount + 1) / 2
					legal_locations = [end_loc]
					var scale_factor = highlights.scale
					var tile_center = highlights.map_to_local(tile_pos)
					# Convert tile position to world coordinates and center it
					tile_center *= scale_factor
					overlay.position = tile_center
					overlay.show()
					return

		if state == 2 and event.button_index == MOUSE_BUTTON_WHEEL_UP:
			amount = min(amount + 1, max_amount)
		if state == 2 and event.button_index == MOUSE_BUTTON_WHEEL_DOWN:
			amount = max(amount - 1, 1)

	if event is InputEventKey and event.pressed:
		if event.keycode == KEY_ENTER:
			game.board.MoveTokens(start_loc, end_loc, amount)
			overlay.hide()
			queue_free()
		if event.keycode in [KEY_W, KEY_UP]:
			amount = min(amount + 1, max_amount)
		if event.keycode in [KEY_S, KEY_DOWN]:
			amount = max(amount - 1, 1)

func _exit_tree():
	highlights.clear()
	static_lights.clear()


#Not sure how to connect these to the buttons
func _on_finish_button_pressed():
	game.board.MoveTokens(start_loc, end_loc, amount)
	overlay.hide()
	queue_free()
	
func _on_cancel_button_pressed():
	overlay.hide()
	queue_free()
	
func _on_plus_button_pressed():
	amount = min(amount + 1, max_amount)
	
func _on_minus_button_pressed():
	amount = max(amount - 1, 1)
