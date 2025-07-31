using Godot;
using System;

public partial class Hud : CanvasLayer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}
	private void _on_shop_pressed()
	{
		GetNode<Panel>("/root/Main/game_scene/GUI/Shop/Panel").Show();
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}
}
