using System.Diagnostics;

namespace PegasusEngine.Core;

public class ScrollingBuffer
{
    private readonly double[] _data;
    private readonly int _capacity;
    private int _writeIdx;
    private int _size;
    
    public int Capacity => _capacity;
    public int Size => _size;
    public bool Empty => _size == 0;

    public ScrollingBuffer(int capacity)
    {
        _capacity = capacity;
        _data = new double[2 * capacity];
    }

    private static int Mod(int a, int m) => ((a % m) + m) % m;

    public void PushBack(double val)
    {
        // Throw error?
        if (_capacity == 0) return;
        
        _data[_writeIdx] = val;
        _data[_writeIdx + _capacity] = val;

        _writeIdx = (_writeIdx + 1) % _capacity;
        
        if (_size < _capacity) _size++;
    }

    public ReadOnlySpan<double> GetData(int span = 100)
    {
        if (_capacity == 0) return ReadOnlySpan<double>.Empty;

        int rSize = SpanSize(span);
        if (_size < _capacity)
            return new ReadOnlySpan<double>(_data, _size - rSize, rSize);
        
        return new ReadOnlySpan<double>(_data, _writeIdx + (_size - rSize), rSize);
    }

    public void Clear()
    {
        _size = 0;
        _writeIdx = 0;
    }
    
    public int GetSize(int span = 100) => SpanSize(span);

    public double Average(int span = 100)
    {
        int rSize = SpanSize(span);
        if (rSize == 0) return 0;

        double sum = 0;
        int startingIdx = Mod(_writeIdx - rSize, _capacity);
        for (int i = 0; i < rSize; i++)
            sum += _data[(startingIdx + i) % _capacity];
        return sum / rSize;
    }

    private int SpanSize(int span)
    {
        span = Math.Clamp(span, 1, 100);
        return (int) Math.Ceiling(_size * (span / 100.0));
    }
}

public class Profiler
{
    public sealed class ScopeTimer : IDisposable
    {
        private readonly Profiler _profiler;
        private readonly string _label;
        private readonly long _startTimespamp;

        public ScopeTimer(Profiler profiler, string label)
        {
            _profiler = profiler;
            _label = label;
            _profiler.CreateTimerEntry(label);
            _startTimespamp = Stopwatch.GetTimestamp();
        }

        public void Dispose()
        {
            var elapsed = Stopwatch.GetElapsedTime(_startTimespamp);
            _profiler.AddTimerValue(_label, elapsed.TotalMilliseconds);
        }
    }
    
    private readonly Dictionary<string, ScrollingBuffer> _data = new();
    private readonly int _capacity;
    private string _globalLabel = "GLOBAL";
    
    public bool GlobalTimerSet { get; private set; }
    public bool IsPaused { get; set; }
    public string GlobalLabel => _globalLabel;
    public IReadOnlyDictionary<string, ScrollingBuffer> Data => _data;

    public Profiler(int capacityPerTimer)
    {
        _capacity = capacityPerTimer;
    }

    public ScopeTimer CreateGlobalTimer(string globalLabel)
    {
        _globalLabel = globalLabel;
        GlobalTimerSet = true;
        return new ScopeTimer(this, _globalLabel);
    }

    public ScopeTimer CreateTimer(string label)
    {
        Debug.Assert(label != _globalLabel, "Use CreateGlobalTimer for the global scope!");
        return new ScopeTimer(this, label);
    }

    public void CreateTimerEntry(string label)
    {
        if (!_data.ContainsKey(label))
            _data.Add(label, new ScrollingBuffer(_capacity));
    }

    public void AddTimerValue(string label, double elapsedMs)
    {
        if (IsPaused) return;
        
        if (_data.TryGetValue(label, out var buffer))
            buffer.PushBack(elapsedMs);
    }

    public ScrollingBuffer GetGlobalBuffer()
    {
        Debug.Assert(GlobalTimerSet, "Global timer must be set before accessing the global buffer!");
        return _data[_globalLabel];
    }

    public void Clear()
    {
        foreach (var buffer in _data.Values) buffer.Clear();
    }
}