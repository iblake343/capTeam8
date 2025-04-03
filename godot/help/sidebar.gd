extends VBoxContainer

@onready var main_text = $"../../../Text"

func _ready() -> void:
	var idx = 0
	print(main_text.get_child_count())
	for child in main_text.get_children():
		var node = LinkButton.new()
		node.text = child.name
		node.underline = LinkButton.UNDERLINE_MODE_ON_HOVER
		node.pressed.connect(main_text.set_child.bind(idx))
		add_child(node)
		idx += 1
