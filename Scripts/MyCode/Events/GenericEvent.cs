using System.Collections.Generic;

public class GenericEvent<T> where T : class, new()
{
    private Dictionary<string, T> _stringMap = new Dictionary<string, T>();
    private Dictionary<AdState, T> _adStateMap = new Dictionary<AdState, T>();

    public T Get(string stringChannel = "")
    {
        _stringMap.TryAdd(stringChannel, new T());
        return _stringMap[stringChannel];
    }

    public T Get(AdState adStateChannel)
    {
        _adStateMap.TryAdd(adStateChannel, new T());
        return _adStateMap[adStateChannel];
    }
}
