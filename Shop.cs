using Godot;
using System;

public partial class Shop : CanvasLayer
{
	// Called when the node enters the scene tree for the first time.
	[Export]
	int[] WTP_speed { get; set; } = new int[4]; 
	public int speed_lv = 0;
	public bool speed_max = false;

	[Export]
	int[] WTP_efficiency { get; set; } = new int[4]; 
	public int efficiency_lv = 0;
	public bool efficiency_max = false;

	[Export]
	int[] WTP_capacity { get; set; } = new int[4];
	public int capacity_lv = 0;
	public bool capacity_max = false;

	[Export]
	int[] Pipe_upgrade { get; set; } = new int[4];
	public int pipe_lv = 0;
	public bool pipe_max = false;

	


	private void _on_close_pressed() {


		GetNode<Panel>("Panel").Hide();

	}


	private void  _on_pipe_upg_pressed()
	{
		var Person = GetNode<Person>("/root/Main/game_scene/Person");
		if (pipe_max == false && Person.Money >= Pipe_upgrade[pipe_lv]) {
			Person.Money -= Pipe_upgrade[pipe_lv];
			pipe_lv ++;
		}

		if (pipe_lv == Pipe_upgrade.Length) {
			pipe_max = true;
		}
		
	}

	private void _on_capacity_upg_pressed() {
		var Person = GetNode<Person>("/root/Main/game_scene/Person");
		if (capacity_max == false && Person.Money >= WTP_capacity[capacity_lv]) {
			Person.Money -= WTP_capacity[capacity_lv];
			capacity_lv++;
		}

		if (capacity_lv == WTP_capacity.Length) {
			capacity_max = true;
		}

	}


	private void _on_speed_upg_pressed() {
		var Person = GetNode<Person>("/root/Main/game_scene/Person");
		if (speed_max == false && Person.Money >= WTP_speed[speed_lv]) {
			Person.Money -= WTP_speed[speed_lv];
			speed_lv++;
		}

		if (speed_lv == WTP_speed.Length) {
			speed_max = true;
		}




	}

	private void _on_efficiency_upg_pressed() {
		var Person = GetNode<Person>("/root/Main/game_scene/Person");
		if (efficiency_max == false && Person.Money >= WTP_efficiency[efficiency_lv]) {
			Person.Money -= WTP_efficiency[efficiency_lv];
			efficiency_lv++;
		}

		if (efficiency_lv == WTP_efficiency.Length) {
			efficiency_max = true;
		}
		
	}

	private void  _on_top_up_pressed()
	{
		//will work diffenernty
		var Person = GetNode<Person>("/root/Main/game_scene/Person");
		int cost = (int)(Person.WaterCapacity - Person.Water);
		if (Person.Water < Person.WaterCapacity && Person.Money>cost) {
			Person.Money -= cost;
			Person.Water = Person.WaterCapacity;
		}
		
	}
	public override void _Ready() {

	}



	public void UpdateCostsAndStats() {
		if (speed_max) {
			GetNode<Button>("Panel/speed_upg").Text = " upgrade WTP  speed\nMAX";
		}
		else {
			GetNode<Button>("Panel/speed_upg").Text = " upgrade WTP  speed\n£ " + WTP_speed[speed_lv];
		}

		if (efficiency_max) {
			GetNode<Button>("Panel/efficiency_upg").Text = " upgrade WTP efficiency\nMAX";
		}
		else {
			GetNode<Button>("Panel/efficiency_upg").Text = " upgrade WTP efficiency\n£ " + WTP_efficiency[efficiency_lv];
		}

		if (pipe_max) {
			GetNode<Button>("Panel/pipe_upg").Text = " upgrade pipe\nMAX";
		}
		else {
			GetNode<Button>("Panel/pipe_upg").Text = " upgrade pipe\n£ " + Pipe_upgrade[pipe_lv];
		}

		if (capacity_max) {
			GetNode<Button>("Panel/capacity_upg").Text = " upgrade WTP capacity\nMAX";
		}
		else {
			GetNode<Button>("Panel/capacity_upg").Text = " upgrade WTP capacity\n£ " + WTP_capacity[capacity_lv];
		}
		var Person = GetNode<Person>("/root/Main/game_scene/Person");
		GetNode<Button>("Panel/top_up").Text = "top up water £ " + (int)(Person.WaterCapacity - Person.Water);


		var slider = GetNode<VSlider>("/root/Main/game_scene/GUI/HUD/border/level_slider");
		 

		GetNode<Label>("Panel/revenue_label").Text = "Gross revenue: £" + Person.Revenue;
		GetNode<Label>("Panel/speed_label").Text = "WTP speed: " + "None" + "L/h";
		GetNode<Label>("Panel/capacity_label").Text = "WTP capacity: " + Person.WaterCapacity + "L";
		GetNode<Label>("Panel/efficiency_label").Text = "Water efficiency: " + Person.WaterEfficiency * 100 + "%";
		GetNode<Label>("Panel/plv_label").Text = "Pipe LV: " + slider.Value;
		GetNode<Label>("Panel/pspeed_label").Text = "Pipe speed: " + "None" + "L/h";
		GetNode<Label>("Panel/pcapacity_label").Text = "Pipe capacity: " + "None" + "L";
		GetNode<Label>("Panel/circulation_label").Text = "Water circulating: " +Mathf.Round(Person.Water)+ "L" ;
		

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public double time = 0d;
	public override void _Process(double delta) {
		UpdateCostsAndStats();
		time += delta;
		GetNode<Label>("Panel/elapsed_label").Text = "Time elapsed: " + (int) time + "s";
	}
}
