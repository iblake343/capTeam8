extends Button


func _on_pressed() -> void:
	Music.playButtonPress()
	$"../../../BoxContainer/Help".show()
