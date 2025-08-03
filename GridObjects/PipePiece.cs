using Godot;
using System;

public partial class PipePiece : Node2D, Pipe {
    [Export]
    public int MaxCapacity { get; set; }
    [Export]
    public int Capacity { get; set; }
    [Export]
    public int Cost { get; set; }
    public bool HasPushedWater { get; set; } = false;
    public bool IsPureWater { get; set; } = true;

    public override void _Process(double delta) {
        var texture = GetNode<TextureRect>("TextureRect");
        if (!IsPureWater) {
            texture.Modulate = new Color(0.75f, 0.75f, 0.75f);
        }
        else {
            texture.Modulate = new Color(1f, 1f, 1f);
        }
    }
}
