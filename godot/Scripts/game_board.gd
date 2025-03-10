extends Node

var placed_tiles: Array = []
var player1 = 0
var player2 = 1
func add_tile_position(tile_positions: Array):
	for pos in tile_positions:
		placed_tiles.append(pos)

func get_placed_tiles() -> Array:
	return placed_tiles

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass
