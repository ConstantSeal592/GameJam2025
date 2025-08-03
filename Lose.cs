using Godot;
using System;

public partial class Lose : Node2D
{
	public void _on_leave_pressed() {
		GetTree().Quit();
		
	}

	public void _on_try_again_pressed() {
		GetTree().ChangeSceneToFile("res://Main.tscn");
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
