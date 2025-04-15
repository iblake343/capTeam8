extends CanvasLayer

func _on_quit_btn_pressed() -> void:
	Music.playButtonPress()
	get_tree().change_scene_to_file("res://Scenes/settings.tscn")
