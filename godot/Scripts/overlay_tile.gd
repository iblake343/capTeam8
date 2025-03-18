extends Node2D
var image = preload("res://.png").get_image()
grayscale_image(image)
var texture = ImageTexture.create_from_image(image)
$Sprite2D.texture = texture

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.

func grayscale_image(image: Image):
	image.lock() # Lock image for pixel access
	for y in range(image.get_height()):
		for x in range(image.get_width()):
			var color = image.get_pixel(x, y)
			var gray = color.r * 0.299 + color.g * 0.587 + color.b * 0.114
			image.set_pixel(x, y, Color(gray, gray, gray, color.a))
	image.unlock() # Unlock when done



# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass
