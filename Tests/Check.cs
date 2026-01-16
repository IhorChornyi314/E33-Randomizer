using E33Randomizer.ItemSources;

namespace Tests;

public class Check(string n, List<ItemSourceParticle> i)
{
    public string Name = n;
    public List<ItemSourceParticle> Items = i;

    public int Size
    {
        get { return Items.Count; }
    }
}