using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public partial class Grid : Node2D {
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
    public PackedScene junc_pipe { get; set; }

    [Export]
    public PackedScene main_pump { get; set; }

    [Export]
    public PackedScene house { get; set; }

    [Export]
    public double WaterUpdateIncrement { get; set; }    //Seconds

    public string CurrentTool { get; set; }
    public int CurrentLevel { get; set; }
    public int CurrentRotation { get; set; } = 0;

    public int CellSize = 50;

    public Vector2 GetGridRelPos(Node2D marker) {
        Node2D parent = marker;
        Vector2 pos = marker.Position;
        while (parent != this) {
            parent = parent.GetParent<Node2D>();

            Transform2D matrix = new Transform2D(parent.Rotation, Vector2.Zero);
            pos = matrix * pos + parent.Position;
        }
        return pos;
    }

    public Node2D GetCellAtCoords(int x, int y) {
        foreach (Node2D cell in GetChildren()) {
            if (cell.IsInGroup("Structure")) { //Loop through children to see if it does occupy the space
                foreach (Node child in cell.GetChildren()) {
                    if (child is Node2D) {
                        var cellChild = child as Node2D;
                        if (GetGridRelPos(cellChild).X == x * CellSize + 0.5f * CellSize && GetGridRelPos(cellChild).Y == y * CellSize + 0.5f * CellSize) {
                            if (child.IsInGroup("Cell")) {
                                return cellChild;
                            }
                        }
                    }
                }
            }
            else if (cell.IsInGroup("Cell")) {
                if (cell.Position.X == x * CellSize + 0.5f * CellSize && cell.Position.Y == y * CellSize + 0.5f * CellSize) {
                    return cell;
                }
            }
        }
        return null;
    }
    public Node2D GetCellAtPosition(int x, int y) {
        int newX = (int)MathF.Round((float)x / (float)CellSize - 0.5f);
        int newY = (int)MathF.Round((float)y / (float)CellSize - 0.5f);
        return GetCellAtCoords(newX, newY);
    }

    public void PlaceCellAtCoords(int x, int y, int rotation, bool flip, PackedScene tileType) {
        var prev = GetCellAtCoords(x, y);
        if (prev != null) {
            if (IsPipePiece(prev)) {
                var pipe = prev as PipePiece;
                SpawnWater(pipe.Capacity);
            }
            prev.Free();
        }

        var cell = tileType.Instantiate<Node2D>();

        cell.Position = new Vector2(x * CellSize + 0.5f * CellSize, y * CellSize + 0.5f * CellSize);
        if (flip == true) {
            cell.GetNode<TextureRect>("TextureRect").FlipH = true;
            cell.GetNode<Marker2D>("Out").Position = new Vector2(50, 0);
        }
        cell.Rotation = (float)rotation / 180 * Mathf.Pi;


        AddChild(cell);
    }

    public void PlaceStructureAtCoords(int x, int y, PackedScene structType) {
        var instantiatedStructure = structType.Instantiate();

        var struc = instantiatedStructure as Structure;
        for (int i = 0; i < struc.childTiles.GetLength(0); i++) {
            GD.Print(struc.childTiles[i,0],",",struc.childTiles[i,1]);
            var prev = GetCellAtCoords(struc.childTiles[i, 0] + x, struc.childTiles[i, 1] + y);
            if (prev != null) {
                if (IsPipePiece(prev)) {
                    var pipe = prev as PipePiece;
                    SpawnWater(pipe.Capacity);
                }
                prev.Free();
            }
            else {
                GD.Print("not found");
            }
        }

        var node = instantiatedStructure as Node2D;
        node.Position = new Vector2(x * CellSize, y * CellSize);

        AddChild(instantiatedStructure);
    }

    public bool IsPipePiece(Node2D PipeSeg) {
        return PipeSeg.IsInGroup("Pipe");
    }

    public Marker2D[] GetFeederMarkers(PipePiece PipeSeg, bool In, bool Out) {
        Marker2D[] markers = new Marker2D[8];
        int index = 0;

        Regex[] regexes = new Regex[2];
        if (In) {
            regexes[0] = new Regex(@"^In\d*$");
        }
        if (Out) {
            regexes[1] = new Regex(@"^Out\d*$");
        }

        foreach (Node child in PipeSeg.GetChildren()) {
            foreach (Regex regex in regexes) {
                if (regex != null) {
                    if (regex.IsMatch(child.Name)) {
                        markers[index] = (Marker2D)child;
                        index++;
                    }
                }
            }
        }

        return markers;
    }

    public PipePiece[] GetPipePrev(PipePiece PipeSeg) {
        PipePiece[] prevs = new PipePiece[4];
        int index = 0;

        foreach (Marker2D marker in GetFeederMarkers(PipeSeg, true, false)) {
            if (marker != null) {
                var cell = GetCellAtPosition((int)GetGridRelPos(marker).X, (int)GetGridRelPos(marker).Y);
                if (cell != null) {
                    if (IsPipePiece(cell)) {
                        bool valid = false;

                        foreach (Marker2D marker1 in GetFeederMarkers((PipePiece)cell, false, true)) {
                            if (marker1 != null) {
                                if ((GetGridRelPos(marker1) - GetGridRelPos(PipeSeg)).Length() < 0.1) {
                                    valid = true;
                                }
                            }
                        }

                        if (valid) {
                            prevs[index] = (PipePiece)cell;
                            index++;
                        }
                    }
                }
            }
        }

        return prevs;
    }
    public PipePiece[] GetPipeNext(PipePiece PipeSeg) {
        PipePiece[] nexts = new PipePiece[4];
        int index = 0;

        foreach (Marker2D marker in GetFeederMarkers(PipeSeg, false, true)) {
            if (marker != null) {
                var cell = GetCellAtPosition((int)GetGridRelPos(marker).X, (int)GetGridRelPos(marker).Y);

                if (cell != null && IsPipePiece(cell)) {
                    bool valid = false;

                    foreach (Marker2D marker1 in GetFeederMarkers((PipePiece)cell, true, false)) {
                        if (marker1 != null) {
                            if ((GetGridRelPos(marker1) - GetGridRelPos(PipeSeg)).Length() < 0.1) {
                                valid = true;
                            }
                        }
                    }

                    if (valid) {
                        nexts[index] = (PipePiece)cell;
                        index++;
                    }
                }
            }
        }

        return nexts;
    }

    public void FlowWater(PipePiece PipeSeg) {
        if (PipeSeg.HasPushedWater) {
            return;
        }

        //Push water to next peice
        PipePiece[] nexts = GetPipeNext(PipeSeg);

        foreach (PipePiece next in nexts) {
            //NEEDS REWORKING TO SPLIT EVENLY AT JUNCTIONS
            if (next != null) {
                if (next.IsPureWater == PipeSeg.IsPureWater || next.Capacity == 0) {
                    int ToFlow = (int)MathF.Min(next.MaxCapacity - next.Capacity, PipeSeg.Capacity);
                    PipeSeg.Capacity -= ToFlow;
                    next.Capacity += ToFlow;

                    next.IsPureWater = PipeSeg.IsPureWater;

                    PipeSeg.HasPushedWater = true;
                }
                else {
                    GD.Print("Water type mix!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                }
            }
        }

        PipePiece[] prevs = GetPipePrev(PipeSeg);
        foreach (PipePiece prev in prevs) {
            if (prev != null) {
                GD.Print("Calling ", prev.Name);
                PipePeicesToFlow.Add(prev);
            }
        }
    }

    List<PipePiece> PipePeicesToFlow = new List<PipePiece>();

    public void UpdateWaterFlows() {
        PipePeicesToFlow = new List<PipePiece>();

        foreach (Node2D cell in GetChildren()) {
            if (cell.IsInGroup("Structure")) {
                var struc = cell as Structure;
                struc.Update();

                PipePeicesToFlow.Add(cell.GetNode<PipePiece>("In"));
                cell.GetNode<PipePiece>("In").HasPushedWater = false;
                cell.GetNode<PipePiece>("Out").HasPushedWater = false;
            }
            else if (cell.IsInGroup("Cell")) {
                if (IsPipePiece(cell)) {
                    var pipe = cell as PipePiece;

                    pipe.HasPushedWater = false;

                    PipePiece[] nexts = GetPipeNext(pipe);
                    bool isEnd = true;
                    foreach (PipePiece next in nexts) {
                        if (next != null) {
                            isEnd = false;
                        }
                    }
                    GD.Print(isEnd);
                    if (isEnd == true) {
                        PipePeicesToFlow.Add(pipe);
                    }
                }
            }
        }

        //var junc = GetNode("main_pump").GetNode<PipePiece>("In");

        // PipePiece[] prevs = GetPipePrev(junc);
        // foreach (PipePiece PipeSeg in prevs) {
        //     GD.Print("P ", PipeSeg);
        //     if (PipeSeg != null) {
        //         GD.Print("Inital call ", PipeSeg.Position);
        //         FlowWater(PipeSeg);
        //     }
        // }

        GD.Print("Starts found", PipePeicesToFlow.Count);
        while (PipePeicesToFlow.Count > 0) {
            FlowWater(PipePeicesToFlow[0]);
            PipePeicesToFlow.RemoveAt(0);
        }
    }

    public void BuildPipePath(int startX, int startY, int endX, int endY) {     //lvl needs adding
        if (MathF.Abs(startX - endX) > MathF.Abs(startY - endY)) {
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
        else {
            if (startY != endY) {
                for (int y = Mathf.Min(startY, endY); y <= Mathf.Max(startY, endY); y++) {
                    PlaceCellAtCoords(startX, y, (endY < startY) ? 90 : 270, false, straight_pipe);
                }
            }

            if (startX != endX) {
                for (int x = Mathf.Min(startX, endX); x <= MathF.Max(startX, endX); x++) {
                    PlaceCellAtCoords(x, endY, (endX < startX) ? 0 : 180, false, straight_pipe);
                }
            }

            if (startX > endX) {
                if (startY > endY) {
                    PlaceCellAtCoords(startX, endY, 0, false, bent_pipe);
                }
                else if (endY > startY) {
                    PlaceCellAtCoords(startX, endY, 180, true, bent_pipe);
                }
            }
            else if (endX > startX) {
                if (startY > endY) {
                    PlaceCellAtCoords(startX, endY, 0, true, bent_pipe);
                }
                else if (endY > startY) {
                    PlaceCellAtCoords(startX, endY, 180, false, bent_pipe);
                }
            }
        }
    }

    public void SpawnWater(int water) {
        GetNode<MainPump>("main_pump").Capacity += water;
    }

    double coolDown = 0d;

    public override void _Ready() {
        for (int x = 0; x < XGridSize; x++) {
            for (int y = 0; y < YGridSize; y++) {
                PlaceCellAtCoords(x, y, 0, false, ground);
            }
        }

        // PlaceCellAtCoords(1, 1, 0, false, bent_pipe);
        // PlaceCellAtCoords(4, 1, 90, false, bent_pipe);
        // PlaceCellAtCoords(7, 1, 270, false, bent_pipe);
        // PlaceCellAtCoords(10, 1, 360, false, bent_pipe);

        // PlaceCellAtCoords(1, 4, 0, true, bent_pipe);
        // PlaceCellAtCoords(4, 4, 90, true, bent_pipe);
        // PlaceCellAtCoords(7, 4, 270, true, bent_pipe);
        // PlaceCellAtCoords(10, 4, 360, true, bent_pipe);

        PlaceStructureAtCoords(2, 2, house);

        PlaceStructureAtCoords(5, 5, main_pump);

        coolDown = WaterUpdateIncrement;
    }

    public void PositionPipeOverlay() {
        
    }

    public override void _Process(double delta) {
        coolDown -= delta;
        if (coolDown < 0) {
            coolDown = WaterUpdateIncrement;

            GD.Print("Update");
            UpdateWaterFlows();
        }

        var info = GetNode<PipeInfo>("pipe_info");
        var cell = GetCellAtPosition((int)info.Position.X, (int)info.Position.Y);
        if (IsPipePiece(cell)) {
            var pipe = cell as PipePiece;
            info.SetText(pipe.Capacity, pipe.MaxCapacity);
        }

        if (Input.IsActionPressed("rotate")) {
            CurrentRotation = (CurrentRotation + 90) % 720;
        }
    }


    public int startDragX = 0;
    public int startDragY = 0;

    public override void _UnhandledInput(InputEvent @event) {
        if (@event is InputEventMouseButton mouseDown) {
            if (mouseDown.ButtonIndex == MouseButton.Left) {
                if (CurrentTool == "BuildTool") {
                    var clickCoords = GetViewport().GetMousePosition();

                    int x = (int)clickCoords.X / CellSize;
                    int y = (int)clickCoords.Y / CellSize;

                    GD.Print(x, y);

                    if (mouseDown.Pressed) {
                        startDragX = x;
                        startDragY = y;
                    }
                    else {
                        BuildPipePath(startDragX, startDragY, x, y);
                    }
                }
                else {
                    var clickCoords = GetViewport().GetMousePosition();

                    int x = (int)clickCoords.X / CellSize;
                    int y = (int)clickCoords.Y / CellSize;

                    GD.Print(CurrentTool);
                    
                    if (CurrentTool == "Straight") {
                        PlaceCellAtCoords(x, y, CurrentRotation, false, straight_pipe);
                    }
                    else if (CurrentTool == "Bent") {
                        PlaceCellAtCoords(x, y, CurrentRotation, (CurrentRotation >= 360) ? false : true, bent_pipe);
                    }
                    else if (CurrentTool == "Tunnel") {
                        GD.Print("NO TUNNEL");
                        //PlaceCellAtCoords(x, y, CurrentRotation, (CurrentRotation >= 360) ? false : true, straight_pipe);
                    }
                    else if (CurrentTool == "Junc") {
                        PlaceCellAtCoords(x, y, CurrentRotation, (CurrentRotation >= 360) ? false : true, junc_pipe);
                    }
                    else if (CurrentTool == "Delete") {
                        PlaceCellAtCoords(x, y, CurrentRotation, (CurrentRotation >= 360) ? false : true, ground);
                    }
                }
            }
        }

        else if (@event is InputEventMouseMotion mouseMove) {
            var info = GetNode<PipeInfo>("pipe_info");

            var cell = GetCellAtPosition((int)mouseMove.Position.X, (int)mouseMove.Position.Y);
            bool isPipe = false;

            if (cell != null) {
                if (IsPipePiece(cell)) {
                    var pipe = cell as PipePiece;
                    info.Position = GetGridRelPos(pipe);
                    info.SetText(pipe.Capacity, pipe.MaxCapacity);

                    isPipe = true;
                }
            }

            info.Hide();
            if (isPipe == true) {
                info.Show();
            }
        }
    }

}
