extends CanvasLayer
var background_scene = null
# Called when the node enters the scene tree for the first time.
func _ready():
	if background_scene == null:
		background_scene = load("res://Scenes/PersistentBackground.tscn").instantiate()
		call_deferred("add_background_scene")

func add_background_scene():
	get_tree().root.add_child(background_scene)

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass


func _on_start_btn_pressed() -> void:
	get_tree().change_scene_to_file("res://Scenes/select_net_ai.tscn")
