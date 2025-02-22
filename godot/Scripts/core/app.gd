extends Node

@onready var game = $Game
@onready var wrapper = $Player

func _ready() -> void:
	game.registerPlayer1(wrapper)
	game.startGame()

func place_tile():
	print("placing tile...");
	await get_tree().create_timer(1.0).timeout
	return [Vector2i(1, 2), 1]
