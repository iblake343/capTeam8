extends Node2D


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass


func _on_button_pressed() -> void:
	GameBoard.player1 = 0
	GameBoard.player2 = 3
	get_tree().change_scene_to_file("res://Scenes/TileMapLayerGameGrid.tscn")


func _on_button_2_pressed() -> void:
	GameBoard.player1 = 1
	GameBoard.player2 = 2
	get_tree().change_scene_to_file("res://Scenes/TileMapLayerGameGrid.tscn")


func _on_button_3_pressed() -> void:
	GameBoard.player1 = 2
	GameBoard.player2 = 0
	get_tree().change_scene_to_file("res://Scenes/TileMapLayerGameGrid.tscn")

func _on_button_4_pressed() -> void:
	GameBoard.player1 = 3
	GameBoard.player2 = 1
	get_tree().change_scene_to_file("res://Scenes/TileMapLayerGameGrid.tscn")
