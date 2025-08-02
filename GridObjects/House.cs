using Godot;
using System;

public partial class House : Node2D {
    //Max water is handles by MaximumWater pls handle the water pump in and out and New water is NewWater :)

	public int WasteOwned = 0;
	public int WaterOwned = 0;
	public int MinimumWater = 15;
	public int MaximumCapacity = 60;
	public int Capacity => WasteOwned + WaterOwned;
	public int NewWater;
	[Export]
	public float WaterDebtMult = 1;

	[Export]
	public int WasteRate = 1;

	public void CreateWaste(int rate) {
		WasteOwned += rate;
		WaterOwned -= rate;

	}

	public void WaterToMoney(int NewWater) {
		var Person = GetNode<Person>("/root/Main/game_scene/Person");
		Person.Money += NewWater;
		WaterOwned += NewWater;
	}

	public void CheckWaterNotSupplied() {
		if (WaterOwned < MinimumWater) {
			var Person = GetNode<Person>("/root/Main/game_scene/Person");
			Person.Money -= (int)((MinimumWater - WaterOwned) * WaterDebtMult);
		}
	}


	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public double time = 0d;
	public override void _Process(double delta) {
		if (WaterOwned > WasteRate) {
			CreateWaste(WasteRate);
		}
		time += delta;
		if (delta % 60 == 0) {
			CheckWaterNotSupplied();
		}
		if (MaximumCapacity > NewWater + Capacity) {  //ADD the condition to as water goes in gimme money (change the water to money func accordingly)
			WaterToMoney(NewWater);
			
		}

	}
}




