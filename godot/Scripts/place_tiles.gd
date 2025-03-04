extends Node2D

@onready var tilemaplayer: TileMapLayer = $ChipLayer  # Ensure this exists in your scene
@onready var tilemapcount: TileMapLayer = $ChipLayer2
@onready var PlaceTile = true
@onready var PickTile = false
@onready var first_click_position = null

func _ready():
	pass
	
func _input(event):
	if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT and PlaceTile:
		var mouse_pos = tilemaplayer.get_local_mouse_position()
		var tile_pos = tilemaplayer.local_to_map(mouse_pos)
		#Will need to be told what user to place
		tilemaplayer.set_cell(tile_pos, randi_range(0,1), Vector2i(0, 0))  # Set permanent change
		PlaceTile = false
		#need to be told when to stop placing tiles.
		PickTile = true
		#tilemapcount.set_cell(tile_pos, 1, Vector2i(0, 0))
	elif event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT and PickTile:
		#set cell at tile location. Will have to set the number from the game board.
		if first_click_position == null:
			first_click_position = event.position
			var mouse_pos = tilemaplayer.get_local_mouse_position()
			var tile_pos = tilemaplayer.local_to_map(mouse_pos)
		else:
			var tileNumber = 0
			var tilePlayer = 0
			move_tokens(tileNumber, tilePlayer)
			first_click_position = null
			

		
func move_tokens(tileNumber, tilePlayer):
		var mouse_pos = tilemaplayer.get_local_mouse_position()
		var tile_pos = tilemaplayer.local_to_map(mouse_pos)
		tilemaplayer.set_cell(tile_pos, tileNumber, Vector2i(0,0))
		tilemapcount.set_cell(tile_pos, tilePlayer, Vector2i(0,0))
		
func declare_winner():
	pass # TODO implement
