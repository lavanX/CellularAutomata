using System;
using System.Threading.Tasks;

namespace CellularAutomaton.Core
{
    class Crystal : ICrystal
    {
        private Matrix _matrix;
        public Crystal(int size, int rule)
        {
            _matrix = new Matrix(size, rule);
        }

        public void UpdateMatrixState()
        {
            Matrix result = new Matrix(Size, Rule);
            Parallel.For(0, _matrix.Size, (
                 i) =>
            {
                Parallel.For(0, _matrix.Size, (j) =>
                {
                    PixelState state = _matrix.GetState(i, j);
                    int neighborsCount = _matrix.GetNeighborCount(i, j);
                    PixelState newState = _matrix.GetNewState(state, neighborsCount);
                    result.SetState(i, j, newState);
                });
            });
            
            _matrix = result;
        }

        public void UpdateMatrixState(int iterations)
        {
            for (int i = 0; i < iterations; i++)
            {
                UpdateMatrixState();
            }
        }


        public int Rule => _matrix.Rule;

        public int Size => _matrix.Size;

        public void SetState(int x, int y, PixelState state)
        {
            _matrix.SetState(x,y,state);
        }

        public PixelState GetState(int x, int y)
        {
            if(x < 0 || x >= Size || y < 0 || y >= Size)
                throw new ArgumentException();
            return this._matrix.Pixels[x, y];
        }
    }

    public class CrystalFactory
    {
        public static ICrystal CreateCrystal(int size, int rule)
        {
            return new Crystal(size, rule);
        }
    }
}