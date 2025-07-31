using Godot;
using System;

public partial class Shop : CanvasLayer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		NodePath fullPath = GetPath();
			GD.Print(fullPath);
	}

	private void _on_close_pressed()
	{

		GetNode<Panel>("Panel").Hide();
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
