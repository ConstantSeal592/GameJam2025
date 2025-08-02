using Godot;
using System;

public partial class Hud : CanvasLayer
{
	


	private void _on_shop_pressed()
	{	
		var panel = GetNode<Panel>("/root/Main/game_scene/GUI/Shop/Panel");
		panel.Visible = !panel.Visible;
	}

	public void UpdateMoneyAndSlider() {
		var Person = GetNode<Person>("/root/Main/game_scene/Person");
		GetNode<Label>("border/MoneyLabel").Text = "£" + Person.Money.ToString();
		var level_label = GetNode<Label>("/root/Main/game_scene/GUI/HUD/border/level_label");
		var level_slider = GetNode<VSlider>("/root/Main/game_scene/GUI/HUD/border/level_slider");
		var Shop = GetNode<Shop>("/root/Main/game_scene/GUI/Shop");
		level_slider.MaxValue = Shop.pipe_lv;
		level_label.Text = " pipe lv: " + level_slider.Value.ToString();
		

	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		UpdateMoneyAndSlider();
	}
}
