using System;

namespace CellularAutomaton.Core
{
    internal class Matrix
    {
        private const int DigitCapacity = 0xA;

        private readonly byte[] _currentStates = {0, 0, 0, 0, 0, 1, 1, 1, 1, 1};
        private readonly byte[] _enabledNeighbors = {0, 1, 2, 3, 4, 0, 1, 2, 3, 4};
        private readonly byte[] _resultStates = new byte[DigitCapacity];

        public Matrix(int size, int rule)
        {
            Pixels = new PixelState[size, size];
            Rule = rule;
        }

        public int Size => (int) Math.Sqrt(Pixels.Length);

        public int GetNeighborCount(int x, int y)
        {
            int count = 0;
            count += (int) GetState(x + 1, y);
            count += (int) GetState(x - 1, y);
            count += (int) GetState(x, y + 1);
            count += (int) GetState(x, y - 1);
            return count;
        }

        public PixelState GetState(int x, int y)
        {
            if (CheckCoordinate(x, y))
            {
                return Pixels[x, y];
            }

            return PixelState.Off;
        }

        public void SetState(int x, int y, PixelState state)
        {
            if (CheckCoordinate(x, y))
            {
                Pixels[x, y] = state;
            }
        }

        private bool CheckCoordinate(int x, int y)
        {
            return x >= 0 && x < Size && y >= 0 && y < Size;
        }

        private int _rule = 0;

        public int Rule
        {
            get => _rule;
            private set
            {

                if (value < 0 || value > 1023)
                    throw new ArgumentException($"{nameof(Rule)} must be between 0 and 1023.");
                _rule = value;
                var ruleTemp = _rule;
                for (int i = DigitCapacity - 1; i >= 0; i--)
                {
                    int bit = ruleTemp & 1;
                    _resultStates[i] = (byte)bit;
                    ruleTemp >>= 1;
                }
            }
        }

        private void SetRule(int rule)
        {
           
        }

        public PixelState GetNewState(PixelState currentState, int currentNeighborsCount)
        {
            for (int i = 0; i < _currentStates.Length; i++)
            {
                if (_currentStates[i] == (int) currentState && _enabledNeighbors[i] == currentNeighborsCount)
                {
                    return (PixelState) _resultStates[i];
                }
            }

            throw new Exception("");
        }

        public PixelState[,] Pixels { get; }
    }

    public enum PixelState
    {
        Off = 0,
        On = 1
    }
}

