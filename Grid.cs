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
    public int MyLevel { get; set; }
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
            else if (cell.IsInGroup("Cell") && !cell.IsInGroup("Hologram")) {
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

    public void PlaceCellAtCoords(int x, int y, int rotation, bool flip, PackedScene tileType, bool IsHologram) {
        if (IsHologram == false) {
            var prev = GetCellAtCoords(x, y);
            if (prev != null) {
                if (IsPipePiece(prev)) {
                    var pipe = prev as PipePiece;
                    SpawnWater(pipe.Capacity);
                }
                prev.Free();
            }
        }
        GetTree().CallGroup("Hologram", Node.MethodName.Free);

        var cell = tileType.Instantiate<Node2D>();

        cell.Position = new Vector2(x * CellSize + 0.5f * CellSize, y * CellSize + 0.5f * CellSize);
        if (flip == true) {
            cell.GetNode<TextureRect>("TextureRect").FlipH = true;
            cell.GetNode<Marker2D>("Out").Position = new Vector2(50, 0);
        }
        cell.Rotation = (float)rotation / 180 * Mathf.Pi;

        if (IsHologram) {
            cell.GetNode<TextureRect>("TextureRect").Modulate = new Color(0, 0, 0, 0.5f);
            cell.GetNode<TextureRect>("Hologram").Show();
            if (flip == true) {
                cell.GetNode<TextureRect>("Hologram").FlipH = true;
            }
            cell.AddToGroup("Hologram");
        }


        AddChild(cell);
    }

    public void PlaceStructureAtCoords(int x, int y, PackedScene structType) {
        var instantiatedStructure = structType.Instantiate();

        var struc = instantiatedStructure as Structure;
        for (int i = 0; i < struc.childTiles.GetLength(0); i++) {

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

    public void SpawnHouse(int x, int y) {
        PlaceStructureAtCoords(x, y, house);
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

        while (PipePeicesToFlow.Count > 0) {
            FlowWater(PipePeicesToFlow[0]);
            PipePeicesToFlow.RemoveAt(0);
        }
    }

    public void BuildPipePath(int startX, int startY, int endX, int endY, bool IsHologram) {     //lvl needs adding
        if (MathF.Abs(startX - endX) > MathF.Abs(startY - endY)) {
            if (startX != endX) {
                for (int x = Mathf.Min(startX, endX); x <= MathF.Max(startX, endX); x++) {
                    PlaceCellAtCoords(x, startY, (endX < startX) ? 0 : 180, false, straight_pipe, IsHologram);
                }
            }

            if (startY != endY) {
                for (int y = Mathf.Min(startY, endY); y <= Mathf.Max(startY, endY); y++) {
                    PlaceCellAtCoords(endX, y, (endY < startY) ? 90 : 270, false, straight_pipe, IsHologram);
                }
            }

            if (startX > endX) {
                if (startY > endY) {
                    PlaceCellAtCoords(endX, startY, 270, true, bent_pipe, IsHologram);
                }
                else if (endY > startY) {
                    PlaceCellAtCoords(endX, startY, 270, false, bent_pipe, IsHologram);
                }
            }
            else if (endX > startX) {
                if (startY > endY) {
                    PlaceCellAtCoords(endX, startY, 90, false, bent_pipe, IsHologram);
                }
                else if (endY > startY) {
                    PlaceCellAtCoords(endX, startY, 90, true, bent_pipe, IsHologram);
                }
            }
        }
        else {
            if (startY != endY) {
                for (int y = Mathf.Min(startY, endY); y <= Mathf.Max(startY, endY); y++) {
                    PlaceCellAtCoords(startX, y, (endY < startY) ? 90 : 270, false, straight_pipe, IsHologram);
                }
            }

            if (startX != endX) {
                for (int x = Mathf.Min(startX, endX); x <= MathF.Max(startX, endX); x++) {
                    PlaceCellAtCoords(x, endY, (endX < startX) ? 0 : 180, false, straight_pipe, IsHologram);
                }
            }

            if (startX > endX) {
                if (startY > endY) {
                    PlaceCellAtCoords(startX, endY, 0, false, bent_pipe, IsHologram);
                }
                else if (endY > startY) {
                    PlaceCellAtCoords(startX, endY, 180, true, bent_pipe, IsHologram);
                }
            }
            else if (endX > startX) {
                if (startY > endY) {
                    PlaceCellAtCoords(startX, endY, 0, true, bent_pipe, IsHologram);
                }
                else if (endY > startY) {
                    PlaceCellAtCoords(startX, endY, 180, false, bent_pipe, IsHologram);
                }
            }
        }
    }


    public void BuildPipePath2(int startX, int startY, int endX, int endY, bool IsHologram) {     //lvl needs adding

        Vector2 mousePosition = GetMousePositionRelToGrid();
        mousePosition = new Vector2(
            (int)Mathf.Round(mousePosition.X / CellSize - 0.5f),
            (int)Mathf.Round(mousePosition.Y / CellSize - 0.5f)
        );
        GD.Print(string.Join(", ", allThePositions.Select(v => v.ToString())));
        PlaceCellAtCoords((int)mousePosition.X, (int)mousePosition.Y, 0, false, straight_pipe, IsHologram);
    }
    public void SpawnWater(int water) {
        GetNode<MainPump>("main_pump").Capacity += water;
    }

    public Vector2 GetMousePositionRelToGrid() {
        return GetLocalMousePosition();
    }

    double coolDown = 0d;

    public void HideAllTiles() {
        foreach (Node2D child in GetChildren()) {
            child.Hide();
        }
    }
    public void ShowTilesInRadius(int radius) {
        foreach (Node2D child in GetChildren()) {
            float dist = MathF.Sqrt(MathF.Pow(child.Position.X / CellSize - 0.5f - XGridSize / 2, 2) + MathF.Pow(child.Position.Y / CellSize - 0.5f - YGridSize / 2, 2));
            if (dist < radius) {
                child.Show();
            }
        }
    }

    public void loadLevelCells(int Level) {
        straight_pipe = GD.Load<PackedScene>("res://GridObjects/lvl" + Level.ToString() + "/straight_pipe.tscn");
        junc_pipe = GD.Load<PackedScene>("res://GridObjects/lvl" + Level.ToString() +"/junc_pipe.tscn");
        bent_pipe = GD.Load<PackedScene>("res://GridObjects/lvl" + Level.ToString() +"/bent_pipe.tscn");
    }

    public override void _Ready() {
        GetParent<World>().Position = new Vector2(XGridSize * CellSize - GetViewportRect().Size.X, YGridSize * CellSize - GetViewportRect().Size.Y) * -0.5f;

        for (int x = 0; x < XGridSize; x++) {
            for (int y = 0; y < YGridSize; y++) {
                PlaceCellAtCoords(x, y, 0, false, ground, false);
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

        PlaceStructureAtCoords(20, 20, house);
        PlaceStructureAtCoords(28, 18, house);
        PlaceStructureAtCoords(17, 26, house);
        PlaceStructureAtCoords(30, 30, house);
        PlaceStructureAtCoords(24, 32, house);

        PlaceStructureAtCoords(24, 24, main_pump);

        coolDown = WaterUpdateIncrement;

        HideAllTiles();
        ShowTilesInRadius(10);
    }

    public override void _Process(double delta) {
        coolDown -= delta;
        if (coolDown < 0) {
            coolDown = WaterUpdateIncrement;


            UpdateWaterFlows();
        }

        var info = GetNode<PipeInfo>("pipe_info");
        var cell = GetCellAtPosition((int)info.Position.X, (int)info.Position.Y);
        if (IsPipePiece(cell)) {
            var pipe = cell as PipePiece;
            info.SetText(pipe.Capacity, pipe.MaxCapacity);
        }

        if (Input.IsActionJustPressed("rotate")) {
            CurrentRotation = (CurrentRotation + 90) % 720;

        }

        var clickCoords = GetMousePositionRelToGrid();

        int x = (int)clickCoords.X / CellSize;
        int y = (int)clickCoords.Y / CellSize;

        if (CurrentTool == "BuildTool" && isMouseDown) {
            BuildPipePath(startDragX, startDragY, x, y, true);
        }
        else if (CurrentTool == "Straight") {
            PlaceCellAtCoords(x, y, CurrentRotation, false, straight_pipe, true);
        }
        else if (CurrentTool == "Bent") {
            PlaceCellAtCoords(x, y, CurrentRotation, (CurrentRotation >= 360) ? true : false, bent_pipe, true);
        }
        else if (CurrentTool == "Tunnel") {
            GD.Print("NO TUNNEL");
            //PlaceCellAtCoords(x, y, CurrentRotation, (CurrentRotation >= 360) ? false : true, straight_pipe, true);
        }
        else if (CurrentTool == "Junc") {
            PlaceCellAtCoords(x, y, 0, false, junc_pipe, true);
        }
        else if (CurrentTool == "Delete") {
            PlaceCellAtCoords(x, y, 0, false, ground, true);
        }

        if (MyLevel != CurrentLevel) {
            MyLevel = CurrentLevel;

            loadLevelCells(CurrentLevel);
        }
    }


    public int startDragX = 0;
    public int startDragY = 0;
    public bool isMouseDown = false;


    List<Vector2> allThePositions = new List<Vector2>();
    public override void _UnhandledInput(InputEvent @event) {
        var clickCoords = GetMousePositionRelToGrid();

        int x = (int)clickCoords.X / CellSize;
        int y = (int)clickCoords.Y / CellSize;

        if (@event is InputEventMouseButton mouseDown) {
            if (mouseDown.ButtonIndex == MouseButton.Left) {
                if (mouseDown.Pressed) {
                    isMouseDown = true;
                }
                else {
                    isMouseDown = false;
                }

                if (CurrentTool == "BuildTool") {
                    if (mouseDown.Pressed) {
                        x = startDragX;
                        y = startDragY;
                        // var mousePosition = GetMousePositionRelToGrid();
                        // mousePosition = new Vector2(
                        //     (int)Mathf.Round(mousePosition.X / CellSize - 0.5f),
                        //     (int)Mathf.Round(mousePosition.Y / CellSize - 0.5f)
                        // );
                        // if (!allThePositions.Contains(mousePosition)) {
                        //     allThePositions.Add(mousePosition);
                    }
                    else {
                        BuildPipePath(startDragX, startDragY, x, y, false);
                        allThePositions = new List<Vector2>();

                    }
                    }
                    else {
                        GD.Print(CurrentTool);

                        if (CurrentTool == "Straight") {
                            PlaceCellAtCoords(x, y, CurrentRotation, false, straight_pipe, false);
                        }
                        else if (CurrentTool == "Bent") {
                            PlaceCellAtCoords(x, y, CurrentRotation, (CurrentRotation >= 360) ? true : false, bent_pipe, false);
                        }
                        else if (CurrentTool == "Tunnel") {
                            GD.Print("NO TUNNEL");
                            //PlaceCellAtCoords(x, y, CurrentRotation, (CurrentRotation >= 360) ? false : true, straight_pipe, false);
                        }
                        else if (CurrentTool == "Junc") {
                            PlaceCellAtCoords(x, y, 0, false, junc_pipe, false);
                        }
                        else if (CurrentTool == "Delete") {
                            PlaceCellAtCoords(x, y, 0, false, ground, false);
                        }
                    }
                }
            }

            else if (@event is InputEventMouseMotion mouseMove) {
                var info = GetNode<PipeInfo>("pipe_info");

                var cell = GetCellAtPosition((int)GetMousePositionRelToGrid().X, (int)GetMousePositionRelToGrid().Y);
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
                if (CurrentTool == "BuildTool" && isMouseDown) {


                    // var mousePosition = GetMousePositionRelToGrid();
                    // mousePosition = new Vector2(
                    //     (int)Mathf.Round(mousePosition.X / CellSize - 0.5f),
                    //     (int)Mathf.Round(mousePosition.Y / CellSize - 0.5f)
                    // );
                    // if (!allThePositions.Contains(mousePosition)) {
                    //     allThePositions.Add(mousePosition);
                    }
                }

            }
        }

    

