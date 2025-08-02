using Godot;
using System;

public partial class MainPump : Node2D, Structure {
    [Export]
    public PackedScene ground { get; set; }

    [Export]
    public int MaxCapacity { get; set; }
    [Export]
    public int Capacity { get; set; }

    public int[,] childTiles { get; set; } = new int[3 * 4 + 2, 2];

    public int CellSize = 50;

    public MainPump() {
        GD.Print("Called");

        childTiles[0, 0] = -1;
        childTiles[0, 1] = 1;

        childTiles[1, 0] = 4;
        childTiles[1, 1] = 1;

        int count = 2;
        for (int x = 0; x < 4; x++) {
            for (int y = 0; y < 3; y++) {
                childTiles[count, 0] = x;
                childTiles[count, 1] = y;
                count++;
            }
        }
    }

    public override void _Ready() {
        for (int i = 0; i < childTiles.GetLength(0); i++) {
            var tile = ground.Instantiate<Node2D>();

            tile.Position = new Vector2(childTiles[i, 0] * CellSize + 0.5f * CellSize, childTiles[i, 1] * CellSize + 0.5f * CellSize);

            tile.AddToGroup("Cell");

            AddChild(tile);
        }
    }

    public bool IsOccupingCoords(int x, int y) {
        int thisX = (int)this.Position.X / CellSize;
        int thisY = (int)this.Position.Y / CellSize;

        for (int i = 0; i < childTiles.GetLength(0); i++) {
            if (childTiles[i, 0] + thisX == x && childTiles[i, 1] + thisY == y) {
                return true;
            }
        }
        return false;
    }
} 

