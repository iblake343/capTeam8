using Godot;
using System;
using System.Reflection;

public partial class PlayerSelector : BoxContainer {
	static string ps1 = "/root/LocalSetup/Center/Main/Sel1";
	static string ps2 = "/root/LocalSetup/Center/Main/Sel2";
	static string psk1 = "/root/LocalSetup/Center/Main/P1/Kind";
	static string psk2 = "/root/LocalSetup/Center/Main/P2/Kind";
	static string debug_switch_path = "/root/LocalSetup/Center/Main/Center/DebugSwitch";
	[Export]
	public string global_property_name;
	public override void _Ready() {
		var children = GetChildren();
		for (int ix = 0; ix < children.Count; ++ix) {
			int v = ix;
			Button button = (Button) children[ix];
			Button debug_switch = GetNode<Button>(debug_switch_path);
			button.Connect(Button.SignalName.Pressed, Callable.From(() => {
				if (global_property_name == "p1token") {
					Globals.players[0] = v;
					if (!debug_switch.IsPressed())
					if (Globals.players[1] == v) {
						Globals.players[1] = (v + 1) % 4;
						PlayerSelector next = GetNode<PlayerSelector>(ps2);
						next.UpdateDraw();
					}
				} else if (global_property_name == "p1kind") {
					Globals.player_kinds[0] = (PlayerKind)v;
					if (!debug_switch.IsPressed())
					if (v != 0 && Globals.player_kinds[1] != (PlayerKind) 0) {
						Globals.player_kinds[1] = 0;
						PlayerSelector next = GetNode<PlayerSelector>(psk2);
						next.UpdateDraw();
					}
				} else if (global_property_name == "p2token") {
					Globals.players[1] = v;
					if (!debug_switch.IsPressed())
					if (Globals.players[0] == v) {
						Globals.players[0] = (v + 1) % 4;
						PlayerSelector next = GetNode<PlayerSelector>(ps1);
						next.UpdateDraw();
					}
				} else if (global_property_name == "p2kind") {
					Globals.player_kinds[1] = (PlayerKind)v;
					if (!debug_switch.IsPressed())
					if (v != 0 && Globals.player_kinds[0] != (PlayerKind) 0) {
						Globals.player_kinds[0] = 0;
						PlayerSelector next = GetNode<PlayerSelector>(psk1);
						next.UpdateDraw();
					}
				}
				GD.Print($"set {global_property_name} to {GetSelIx()}");
				UpdateDraw();
			}));
		}
		UpdateDraw();
	}
	
	public int GetSelIx() {
		if (global_property_name == "p1token") {
			return Globals.players[0];
		} else if (global_property_name == "p1kind") {
			return (int)Globals.player_kinds[0];
		} else if (global_property_name == "p2token") {
			return Globals.players[1];
		} else if (global_property_name == "p2kind") {
			return (int)Globals.player_kinds[1];
		}
		GD.Print("error in PlayerSelector: use of unconfigured global_property_name");
		return -1;
	}
	
	public void UpdateDraw() {
		var children = GetChildren();
		for (int ix = 0; ix < children.Count; ++ix) {
			int v = ix;
			Button button = (Button) children[ix];
			int target = GetSelIx();
			button.SetPressedNoSignal(target == ix);
		}
	}
}
