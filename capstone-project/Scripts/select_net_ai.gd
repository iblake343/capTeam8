extends CanvasLayer

# Called when the node enters the scene tree for the first time.
func _ready():
	pass

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass
	
func _on_back_btn_pressed() -> void:
	get_tree().change_scene_to_file("res://Scenes/menu.tscn")


func _on_network_btn_pressed() -> void:
	get_tree().change_scene_to_file("res://Scenes/PlaceTiles.tscn")


func _on_ai_btn_pressed() -> void:
	get_tree().change_scene_to_file("res://Scenes/PlaceTiles.tscn")
