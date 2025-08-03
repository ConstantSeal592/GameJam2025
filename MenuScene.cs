using Godot;
using System;

public partial class MenuScene : Node2D
{
	[Export]
	public PackedScene game_scene { get; set; }
	public Node2D newgame;
	public void _on_play_pressed() {
		var hud = GetNode<Hud>("/root/Main/game_scene/GUI/HUD");
		hud.Show();
		var game = GetNode<Node2D>("/root/Main/game_scene");
		game.Show();
		game.GetNode<Grid>("world/Grid").Start();
		var menu = GetNode<Node2D>("/root/Main/menu_scene");
		Hide();
		menu.QueueFree();


	}

	public void _on_leave_pressed() {
		GetTree().Quit();
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {

		GetNode<Panel>("/root/Main/game_scene/GUI/Shop/Panel").Hide();
		// GetNode<AudioStreamPlayer>("music").Play();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
