using Godot;
using System;

public partial class PipeInfo : Node2D {
    public void SetText(int Capacity, int MaxCapacity) {
        GetNode<Label>("Label").Text = Capacity.ToString() + "/" + MaxCapacity.ToString();
    }
}
