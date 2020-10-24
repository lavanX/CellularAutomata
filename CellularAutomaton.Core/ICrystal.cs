namespace CellularAutomaton.Core
{
    public interface ICrystal
    {
        void UpdateMatrixState();
        void UpdateMatrixState(int iterations);
        int Rule { get; }
        int Size { get; }
        void SetState(int x, int y, PixelState state);
        PixelState GetState(int x, int y);
    }
}
