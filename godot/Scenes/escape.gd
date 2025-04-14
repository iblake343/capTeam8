extends Button



func _on_pressed() -> void:
	$"../../../../..".fail = true;
	var x = $"../../../GameLayer".get_child_count()
	if x > 0: $"../../../GameLayer".get_child(0).queue_free()
	else:
		get_tree().change_scene_to_file.bind("res://Scenes/menu.tscn").call_deferred()
		

func to_menu():
	get_tree().change_scene_to_file.bind("res://Scenes/menu.tscn").call_deferred()
