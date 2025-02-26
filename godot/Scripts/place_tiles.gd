extends Node2D

@onready var tilemaplayer: TileMapLayer = $ChipLayer  # Ensure this exists in your scene
@onready var tilemapcount: TileMapLayer = $ChipLayer2

func _ready():
	#tilemaplayer.set_cell(Vector2i(0,0), 0, Vector2i(0,0))
	pass
func _input(event):
	if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
		var mouse_pos = tilemaplayer.get_local_mouse_position()
		var tile_pos = tilemaplayer.local_to_map(mouse_pos)
		#Will need to be told what user to place
		tilemaplayer.set_cell(tile_pos, 0, Vector2i(0, 0))  # Set permanent change
		#tilemapcount.set_cell(tile_pos, 1, Vector2i(0, 0))
		
