using Godot;
using System;
using System.ComponentModel.DataAnnotations;

public partial class Grid : Node {
    [Export]
    public int XGridSize { get; set; }

    [Export]
    public int YGridSize { get; set; }

    [Export]
    public PackedScene ground { get; set; }

    [Export]
    public PackedScene straight_pipe { get; set; }

    [Export]
    public PackedScene bent_pipe { get; set; }

    [Export]
    public PackedScene main_pump { get; set; }

    public int CellSize = 50;

    public Node2D GetCellAtCoords(int x, int y) {
        foreach (Node2D cell in GetChildren()) {
            if (cell.Position.X == x * CellSize + 0.5f * CellSize && cell.Position.Y == y * CellSize + 0.5f * CellSize) {
                return cell;
            }
        }
        return null;
    }

    public void PlaceCellAtCoords(int x, int y, int rotation, bool flip, PackedScene tileType) {
        var prev = GetCellAtCoords(x, y);
        if (prev != null) {
            prev.QueueFree();
        }

        var cell = tileType.Instantiate<Node2D>();

        cell.Position = new Vector2(x * CellSize + 0.5f * CellSize, y * CellSize + 0.5f * CellSize);
        cell.Rotation = (float)rotation / 180 * Mathf.Pi;

        if (flip == true) {
            cell.GetNode<TextureRect>("TextureRect").FlipH = true;
        }

        AddChild(cell);
    }

    public void PlaceStructureAtCoords(int x, int y, PackedScene structType) {
        var instantiatedStructure = structType.Instantiate();

        var struc = instantiatedStructure as Structure;
        for (int i = 0; i < struc.childTiles.GetLength(0); i++) {
            GD.Print(struc.childTiles[i,0],",",struc.childTiles[i,1]);
            var prev = GetCellAtCoords(struc.childTiles[i, 0] + x, struc.childTiles[i, 1] + y);
            if (prev != null) {
                prev.QueueFree();
                GD.Print("Deleted");
            }
            else {
                GD.Print("not found");
            }
        }

        var node = instantiatedStructure as Node2D;
        node.Position = new Vector2(x * CellSize, y * CellSize);

        AddChild(instantiatedStructure);
    }

    public void BuildPipePath(int startX, int startY, int endX, int endY) {     //lvl needs adding
        if (startX != endX) {
            for (int x = Mathf.Min(startX, endX); x <= MathF.Max(startX, endX); x++) {
                PlaceCellAtCoords(x, startY, (endX < startX) ? 0 : 180, false, straight_pipe);
            }
        }

        if (startY != endY) {
            for (int y = Mathf.Min(startY, endY); y <= Mathf.Max(startY, endY); y++) {
                PlaceCellAtCoords(endX, y, (endY < startY) ? 90 : 270, false, straight_pipe);
            }
        }

        if (startX > endX) {
            if (startY > endY) {
                PlaceCellAtCoords(endX, startY, 270, true, bent_pipe);
            }
            else if (endY > startY) {
                PlaceCellAtCoords(endX, startY, 270, false, bent_pipe);
            }
        }
        else if (endX > startX) {
            if (startY > endY) {
                PlaceCellAtCoords(endX, startY, 90, false, bent_pipe);
            }
            else if (endY > startY) {
                PlaceCellAtCoords(endX, startY, 90, true, bent_pipe);
            }
        }
    }

    public override void _Ready() {
        for (int x = 0; x < XGridSize; x++) {
            for (int y = 0; y < YGridSize; y++) {
                PlaceCellAtCoords(x, y, 0, false, ground);
            }
        }

        PlaceStructureAtCoords(13, 13, main_pump);
    }

    public int startDragX = 0;
    public int startDragY = 0;

    public override void _UnhandledInput(InputEvent @event) {
        if (@event is InputEventMouseButton inputEvent) {
            var clickCoords = GetViewport().GetMousePosition();

            int x = (int)clickCoords.X / CellSize;
            int y = (int)clickCoords.Y / CellSize;

            GD.Print(x, y);

            if (inputEvent.ButtonIndex == MouseButton.Left) {
                if (inputEvent.Pressed) {
                    startDragX = x;
                    startDragY = y;
                }
                else {
                    BuildPipePath(startDragX, startDragY, x, y);
                }
            }
        }
    }

}
