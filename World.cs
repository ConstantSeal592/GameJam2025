using Godot;
using System;

public partial class World : Node2D {
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		if (Input.IsActionPressed("pan right")) {
			Position = new Vector2(Position.X + 1, Position.Y);
		}
		if (Input.IsActionPressed("pan up")) {
			Position = new Vector2(Position.X, Position.Y + 1);
		}
		if (Input.IsActionPressed("pan left")) {
			Position = new Vector2(Position.X - 1, Position.Y);
		}
		if (Input.IsActionPressed("pan down")) {
			Position = new Vector2(Position.X, Position.Y - 1);
		}
		if (Input.IsActionPressed("zoom in")) {
			Scale = new Vector2(Scale.X * 1.01f, Scale.Y * 1.01f);
		}
		if (Input.IsActionPressed("zoom out")) {
			Scale = new Vector2(Scale.X * 0.99f, Scale.Y * 0.99f);
		}
	}
}
