using Godot;
using System;

public partial class Hud : CanvasLayer
{
	private int PrevSliderLevel = 0;


	private void _on_shop_pressed()
	{	
		var panel = GetNode<Panel>("/root/Main/game_scene/GUI/Shop/Panel");
		panel.Visible = !panel.Visible;
	}

	public void UpdateMoneyAndSlider() {
		var Person = GetNode<Person>("/root/Main/game_scene/Person");
		GetNode<Label>("border/MoneyLabel").Text = "£" + Person.Money.ToString();

		//You can do relative paths????
		var level_label = GetNode<Label>("/root/Main/game_scene/GUI/HUD/border/level_label");
		var level_slider = GetNode<VSlider>("/root/Main/game_scene/GUI/HUD/border/level_slider");

		var Shop = GetNode<Shop>("/root/Main/game_scene/GUI/Shop");
		level_slider.MaxValue = Shop.pipe_lv;
		level_label.Text = " pipe lv: " + level_slider.Value.ToString();

		GetNode<Grid>("/root/Main/game_scene/world/Grid").CurrentLevel = (int)level_slider.Value;

		if (PrevSliderLevel != (int)level_slider.Value) {
			PrevSliderLevel = (int)level_slider.Value;
			GetNode<Button>("border/Straight").Icon = GD.Load<Texture2D>("res://art/straight_pipe_" + PrevSliderLevel.ToString() + ".png");
			GetNode<Button>("border/Bent").Icon = GD.Load<Texture2D>("res://art/bent_pipe_" + PrevSliderLevel.ToString() + ".png");
			GetNode<Button>("border/Junc").Icon = GD.Load<Texture2D>("res://art/junc_pipe_" + PrevSliderLevel.ToString() + ".png");
			GetNode<Button>("border/BuildTool").Icon = GD.Load<Texture2D>("res://art/build_" + PrevSliderLevel.ToString() + ".png");
		}
	}

	private void _on_build_tool_pressed() {
		GetNode<Grid>("/root/Main/game_scene/world/Grid").CurrentTool = "BuildTool";
	}
	private void _on_straight_pressed() {
		GetNode<Grid>("/root/Main/game_scene/world/Grid").CurrentTool = "Straight";
	}
	private void _on_bent_pressed() {
		GetNode<Grid>("/root/Main/game_scene/world/Grid").CurrentTool = "Bent";
	}
	private void _on_tunnel_pressed() {
		GetNode<Grid>("/root/Main/game_scene/world/Grid").CurrentTool = "Tunnel";
	}
	private void _on_junc_pressed() {
		GetNode<Grid>("/root/Main/game_scene/world/Grid").CurrentTool = "Junc";
	}
	private void _on_delete_pressed() {
		GetNode<Grid>("/root/Main/game_scene/world/Grid").CurrentTool = "Delete";
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		UpdateMoneyAndSlider();

		if (Input.IsActionJustPressed("hotkey_1")) {
			_on_build_tool_pressed();
		}
		if (Input.IsActionJustPressed("hotkey_2")) {
			_on_straight_pressed();
		}
		if (Input.IsActionJustPressed("hotkey_3")) {
			_on_bent_pressed();
		}
		if (Input.IsActionJustPressed("hotkey_4")) {
			_on_junc_pressed();
		}
		if (Input.IsActionJustPressed("hotkey_6")) {
			_on_delete_pressed();
		}
		if (Input.IsActionJustPressed("level_up")) {
			GetNode<VSlider>("/root/Main/game_scene/GUI/HUD/border/level_slider").Value++;
		}
		if (Input.IsActionJustPressed("level_down")) {
			GetNode<VSlider>("/root/Main/game_scene/GUI/HUD/border/level_slider").Value--;
		}
	}
}
