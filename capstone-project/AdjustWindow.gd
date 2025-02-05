extends Node2D

func _ready():
	var window_center = get_viewport().size / 2
	position = window_center  # Move (0,0) to the center of the window


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass
