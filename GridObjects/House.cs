using Godot;
using System;

public partial class House : Node2D, Structure {
	//Max water is handles by MaximumWater pls handle the water pump in and out and New water is NewWater :)
	
	[Export]
	public int MaxCapacity { get; set; }
	public int WasteCapacity { get; set; }
	public int PureCapacity { get; set; }
	public int Capacity { get; set; }
	
	[Export]
	public float BillsMultiplier { get; set; }

	[Export]
	public int WaterUsedPerUpdate { get; set; }

	public int[,] childTiles { get; set; } = { { -1, 0 }, { 0, 0 }, { 1, 0 } };

	public int CellSize = 50;

	public void WaterToMoney(int NewWater) {
		var Person = GetNode<Person>("/root/Main/game_scene/Person");
		Person.Money += (int)(NewWater * BillsMultiplier);
		Person.Revenue += (int)(NewWater * BillsMultiplier);
	}

	public void Update() {
		var In = GetNode<PipePiece>("In");
		var Out = GetNode<PipePiece>("Out");

		int outPut = (int)MathF.Min(Out.MaxCapacity - Out.Capacity, WasteCapacity);
		Out.Capacity += outPut;
		WasteCapacity -= outPut;

		Capacity = WasteCapacity + PureCapacity;

		int inTake = (int)MathF.Min(In.Capacity, MaxCapacity - Capacity);
		In.Capacity -= inTake;
		PureCapacity += inTake;

		Capacity = WasteCapacity + PureCapacity;

		WaterToMoney(inTake);

		int convert = (int)MathF.Min(PureCapacity, WaterUsedPerUpdate);
		PureCapacity -= convert;
		WasteCapacity += convert;
	}

	public void CheckWaterNotSupplied() {
		if (PureCapacity == 0) {
			//GD.Print("SAHRA MOMENT WAYYYYYYYYYYYYYYYYYYYYY!!!!!!!!!!!");
		}
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		GetNode<Label>("Label").Text = WaterUsedPerUpdate.ToString();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		CheckWaterNotSupplied();
	}
}




