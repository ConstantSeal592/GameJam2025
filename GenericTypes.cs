public interface Structure {
    public int[,] childTiles { get; set; }
    public bool IsOccupingCoords(int x, int y);
}