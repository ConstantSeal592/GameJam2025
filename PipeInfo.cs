using Godot;
using System;

public partial class PipeInfo : Node2D {
    public void SetText(int Capacity, int MaxCapacity, bool IsPureWater) {
        GetNode<Label>("Capacities").Text = Capacity.ToString() + "/" + MaxCapacity.ToString();
        GetNode<Label>("Type").Text = (IsPureWater) ? "Pure" : "Waste";
    }
}
