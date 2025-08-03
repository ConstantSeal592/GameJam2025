using Godot;
using System;

public partial class MenuScene : Node
{
	[Export]
	public PackedScene game_scene { get; set; }
	public Node2D newgame;
	public void _on_play_pressed() {
		var hud = GetNode<Hud>("/root/Main/game_scene/GUI/HUD");
		hud.Show();
		var game = GetNode<Node2D>("/root/Main/game_scene");
		game.Show();
		var menu = GetNode<Node2D>("/root/Main/menu_scene");
		menu.Hide();

	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
