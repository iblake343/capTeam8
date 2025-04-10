using Godot;
using System;

public partial class Hero : CenterContainer {
	public PanelContainer Outline() {
		return GetNode<PanelContainer>("Outline");
	}
	public TextureRect Face() {
		return GetChild(1) as TextureRect;
	}
}
