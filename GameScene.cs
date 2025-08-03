using Godot;
using System;

public partial class GameScene : Node2D
{
	// Called when the node enters the scene tree for the first time.
	[Export]
	int[] Quotas { get; set; } = new int[5];

	public int CurrentQuota;
	public int QuotasAchieved = 0;
	public void NewQuota() {
		CurrentQuota = Quotas[QuotasAchieved];
		QuotasAchieved += 1;
		var grid = GetNode<Grid>("/root/Main/game_scene/world/Grid");
		grid.ShowTilesInRadius(5*(QuotasAchieved+1));
	}



	public void Lose() {
		GD.Print("LOST!!");
	}
	public void Win(){}
	public override void _Ready() {
		CurrentQuota = Quotas[QuotasAchieved];
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		var Person = GetNode<Person>("/root/Main/game_scene/Person");



		if (Person.Revenue > CurrentQuota && QuotasAchieved < Quotas.Length - 1) {
			NewQuota();
		}

		if (QuotasAchieved < Quotas.Length - 1) {
			Win();
		}

		if (Person.Water / Person.WaterCapacity < 0.01) {
			Lose();
		}

		foreach (Node node in GetTree().GetNodesInGroup("House")) {
			if (!node.IsConnected("HouseGameOver", new Callable(this, nameof(Lose)))) {
				node.Connect("HouseGameOver", new Callable(this, nameof(Lose)));
			}
		}
	}
}
