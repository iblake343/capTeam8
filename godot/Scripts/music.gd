extends Node
var isPlaying = false

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass

func playButtonPress():
	isPlaying = true
	$Bubble.play()
	
func startSound():
	$UnderwaterB.play()

func stopSound():
	isPlaying = false
	$UnderwaterB.stop()
	
func placeLand():
	$PlaceLand.play()

func placeChip():
	$PlaceChip.play()
