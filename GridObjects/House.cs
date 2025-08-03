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

	[Export]
	public double MaxTimeWithoutWater { get; set; }

	[Signal]
	public delegate void HouseGameOverEventHandler();

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
		Out.IsPureWater = false;
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

		GetNode<Label>("Capacity").Text = Capacity.ToString();
	}

	public double TimeWithoutWater = 0d;

	public void CheckWaterNotSupplied(double delta) {
		var warning = GetNode<TextureRect>("warning");

		if (PureCapacity == 0 && Visible == true) {
			warning.Show();
			warning.Modulate = new Color(1, 1, 1);
			//GD.Print("SAHRA MOMENT WAYYYYYYYYYYYYYYYYYYYYY!!!!!!!!!!!");
			TimeWithoutWater += delta;
			if (TimeWithoutWater > 0.75d * MaxTimeWithoutWater) {
				warning.Modulate = new Color(1, 0, 1);
			}
			if (TimeWithoutWater > MaxTimeWithoutWater) {
				EmitSignal(SignalName.HouseGameOver);
			}
		}
		else {
			warning.Hide();
			TimeWithoutWater = 0;
		}
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		GetNode<Label>("PerUpdate").Text = WaterUsedPerUpdate.ToString();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		CheckWaterNotSupplied(delta);
	}
}




