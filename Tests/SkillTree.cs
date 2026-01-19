using E33Randomizer;

namespace Tests;

public class SkillTree
{
    public List<SkillNode> SkillNodes;
    public List<Tuple<int, int>> Edges;
    public List<SkillData> Skills;
    public string Name;

    public SkillTree(SkillGraph graph)
    {
        Skills = graph.Nodes.Select(n => n.SkillData).ToList();
        SkillNodes = new List<SkillNode>(graph.Nodes);
        Edges = new List<Tuple<int, int>>(graph.Edges);
        Name = graph.CharacterName;
    }
}