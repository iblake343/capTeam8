extends CanvasLayer

func _on_quit_btn_pressed() -> void:
	Music.playButtonPress()
	get_tree().change_scene_to_file("res://Scenes/menu.tscn")


func _on_settings_btn_pressed() -> void:
	Music.playButtonPress()
	get_tree().change_scene_to_file("res://Scenes/credits.tscn")


func _on_start_game_btn_pressed() -> void:
	Music.playButtonPress()
	get_tree().change_scene_to_file("res://Scenes/tutorial.tscn")
