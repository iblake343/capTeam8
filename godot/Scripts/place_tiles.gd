extends Node2D

@onready var tilemaplayer: TileMapLayer = $ChipLayer  # Ensure this exists in your scene
@onready var tilemapcount: TileMapLayer = $ChipLayer2
@onready var placing_tile = true
@onready var selecting_tile = false
@onready var first_click_position = null
@onready var selected_tile_pos = null
@onready var tileNumber = 0
@onready var tilePlayer = 0
@onready var playing_faze = false

func _ready():
	pass
	
func _input(event):
	if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
		var mouse_pos = tilemaplayer.get_local_mouse_position()
		var tile_pos = tilemaplayer.local_to_map(mouse_pos)
		
		if placing_tile and tilePlayer == 0 and !playing_faze:
			#game core will have to see if original tile placement is valid
			tilemaplayer.set_cell(tile_pos, 0, Vector2i(0, 0)) 
			tilePlayer = 1
		elif placing_tile and tilePlayer == 1 and !playing_faze:
			#game core will have to see if original tile placement is valid
			tilemaplayer.set_cell(tile_pos, 1, Vector2i(0, 0)) 
			placing_tile = false
			selecting_tile = true
			playing_faze = true
		elif playing_faze:
			#will have to tell game core what tile was selected and receive validation
			playing_faze = false
		elif selecting_tile:
			#game core will need to check the next click to see if it is correct.
			#also will need to know the max amount of tiles that are able to be moved
			#will cap the "tileNumber" depending on starting click
			move_tile(selected_tile_pos, tile_pos)
			if selected_tile_pos == null:
				selected_tile_pos = tile_pos 
			else:
				selected_tile_pos = null

	elif event is InputEventKey and event.pressed and event.keycode == KEY_SPACE:
		playing_faze = true
		if tilePlayer == 1:
			tilePlayer = 0
		else:
			tilePlayer = 1

func move_tile(from_pos, to_pos):

	tilemaplayer.set_cell(to_pos, tilePlayer, Vector2i(0, 0))
	tilemapcount.set_cell(to_pos, tileNumber, Vector2i(0, 0))
	#tileNumber will be edit to have a max of how many tiles there are in starting stack
	if tileNumber == 0:
		tileNumber += 1
	else:
		tileNumber = 0
