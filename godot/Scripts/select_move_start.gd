extends Node2D
@onready var tileplacement: TileMapLayer = $TileMapLayer
@onready var tilemaplayer: TileMapLayer = $ChipLayer  # Ensure this exists in your scene
@onready var tilemapcount: TileMapLayer = $ChipLayer2
@onready var tilemaphighlight: TileMapLayer = $TileMapHighlight
@onready var clownDisk = $"CanvasLayer/HBoxContainer/ClownfishDisk1(1)"
@onready var octopusDisk = $"CanvasLayer/HBoxContainer/OctopusDisk0(1)"
@onready var crabDisk = $"CanvasLayer/HBoxContainer/CrabDisk0(1)"
@onready var sharkDisk = $"CanvasLayer/HBoxContainer/SharkDisk1(1)"
@onready var placing_tile = true
@onready var selecting_tile = false
@onready var first_click_position = null
@onready var selected_tile_pos = null
@onready var tileNumber = 0
var tilePlayer
@onready var playing_faze = false
var begin = false
var temp_id = null
var temp_mouse_pos = null
var temp_tile_pos = null
var player1
var player2

func _ready():
	var previous_tiles = GameBoard.get_placed_tiles()
	tilePlayer = GameBoard.player1
	for tile in previous_tiles:
		tileplacement.set_cell(tile, randi_range(1, 5), Vector2i(0, 0))  # Example of using stored positions
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
	
func _input(event):
	if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
		var mouse_pos = tilemaplayer.get_local_mouse_position()
		var tile_pos = tilemaplayer.local_to_map(mouse_pos)
		
		if placing_tile and tilePlayer == GameBoard.player1 and !playing_faze:
			#game core will have to see if original tile placement is valid
			tilemaplayer.set_cell(tile_pos, tilePlayer, Vector2i(0, 0)) 
			tilePlayer = GameBoard.player2
			player1.hide()
			player2.show()
			tilemapcount.set_cell(tile_pos, 14, Vector2i(0, 0))
		elif placing_tile and tilePlayer == GameBoard.player2 and !playing_faze:
			#game core will have to see if original tile placement is valid
			tilemaplayer.set_cell(tile_pos, tilePlayer, Vector2i(0, 0)) 
			tilemapcount.set_cell(tile_pos, 14, Vector2i(0, 0))
			tilePlayer = GameBoard.player1
			player2.hide()
			player1.show()
			placing_tile = false
			selecting_tile = true
			playing_faze = true
		elif playing_faze:
			#will have to tell game core what tile was selected and receive validation
			temp_mouse_pos = tilemaplayer.get_local_mouse_position()
			temp_tile_pos = tilemaplayer.local_to_map(temp_mouse_pos)
			temp_id = tilemapcount.get_cell_source_id(temp_tile_pos)
			if temp_id != -1:
				tilePlayer = tilemaplayer.get_cell_source_id(temp_tile_pos)
				tilemaphighlight.set_cell(temp_tile_pos, tilePlayer, Vector2i(0,0))
				tileNumber = 0
				playing_faze = false
		elif selecting_tile and temp_id != -1:
			#game core will need to check the next click to see if it is correct.
			#also will need to know the max amount of tiles that are able to be moved
			#will cap the "tileNumber" depending on starting click
			move_tile(selected_tile_pos, tile_pos)
			if selected_tile_pos == null:
				selected_tile_pos = tile_pos 
			else:
				selected_tile_pos = null

	elif event is InputEventKey and event.pressed and event.keycode == KEY_SPACE and temp_id != -1:
		playing_faze = true
		tilemaphighlight.clear()
		if !begin:
			tilemapcount.set_cell(temp_tile_pos, temp_id - tileNumber, Vector2i(0,0))
		else:
			tilemapcount.set_cell(temp_tile_pos, tileNumber, Vector2i(0,0))
		if tilePlayer == GameBoard.player2:
			tilePlayer = GameBoard.player1
			player2.hide()
			player1.show()
		else:
			tilePlayer = GameBoard.player2
			player1.hide()
			player2.show()


func move_tile(from_pos, to_pos):
	
	tilemaplayer.set_cell(to_pos, tilePlayer, Vector2i(0, 0))
	tilemapcount.set_cell(to_pos, tileNumber, Vector2i(0, 0))
	#tileNumber will be edit to have a max of how many tiles there are in starting stack
	if tileNumber < temp_id - 1:
		tileNumber += 1
		begin = false
	else:
		tileNumber = 0
		begin = true
