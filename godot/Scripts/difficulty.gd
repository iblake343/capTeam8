extends Node2D

func _on_start_game_btn_pressed() -> void:
	get_tree().change_scene_to_file("res://Scenes/character.tscn")


func _on_start_game_btn_2_pressed() -> void:
	get_tree().change_scene_to_file("res://Scenes/character.tscn")


func _on_start_game_btn_3_pressed() -> void:
	get_tree().change_scene_to_file("res://Scenes/character.tscn")


func _on_endGame_pressed() -> void:
	get_tree().change_scene_to_file("res://Scenes/menu.tscn")
