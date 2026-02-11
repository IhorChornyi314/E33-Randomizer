namespace E33Randomizer;

public class ObjectPool<T>(List<T> startingPool, List<T> excludedPool)
{
    public List<T> StartingPool = startingPool;
    public List<T> ExcludedPool = excludedPool;
    private Queue<T> _currentPool = [];
    private bool _poolEmpty;

    private void Reshuffle()
    {
        var newPool = StartingPool.ToArray();
        newPool = newPool.Where(o => !ExcludedPool.Contains(o)).ToArray();
        _poolEmpty = newPool.Length == 0;
        if (_poolEmpty) return;
        RandomizerLogic.rand.Shuffle(newPool);
        
        _currentPool = new Queue<T>(newPool);
    }

    public T GetObject()
    {
        if (_poolEmpty) return default;
        
        if (_currentPool.Count == 0)
        {
            Reshuffle();
        }

        return _poolEmpty ? default : _currentPool.Dequeue();
    }
    
}