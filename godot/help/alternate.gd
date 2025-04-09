extends RichTextLabel

func _ready():
	set_child(0)
	
func set_child(idx):
	text = get_child(idx).text
