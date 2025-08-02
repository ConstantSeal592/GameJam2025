public interface Structure {
    public int MaxCapacity { get; set; }
    public int Capacity { get; set; }

    public int[,] childTiles { get; set; }
    public bool IsOccupingCoords(int x, int y);
}

public interface Pipe {
    public int MaxCapacity { get; set; }
    public int Capacity { get; set; }
}