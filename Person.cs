using Godot;
using System;

public partial class Person : Node
{
	

	[Export]
	public int Money { get; set; } = 10;
	[Export]
	public float Water { get; set; } = 50.0f;
	[Export]
	public float WaterCapacity { get; set; } = 75.0f;

	[Export]
	public float WaterEfficiency { get; set; } = 0.80f;
	
	public int Revenue { get; set; } = 0;
	public float WaterPercentage => Water / WaterCapacity * 100;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		if (Water > 0.0f){Water -= Water / WaterEfficiency /100;}
		var WaterBar = GetNode<ProgressBar>("/root/Main/game_scene/GUI/HUD/border/Water_bar");
		WaterBar.Value = WaterPercentage; 
	}
}
