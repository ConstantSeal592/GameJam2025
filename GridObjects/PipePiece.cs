using Godot;
using System;

public partial class PipePiece : Node2D, Pipe {
    [Export]
    public int MaxCapacity { get; set; }
    [Export]
    public int Capacity { get; set; }
    public bool HasPushedWater { get; set; } = false;
    public bool IsPureWater { get; set; } = true;
}
