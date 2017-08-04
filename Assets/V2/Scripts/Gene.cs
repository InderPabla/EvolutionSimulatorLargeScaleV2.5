public class Gene
{
    public int inNode;
    public int outNode;
    public float weight;
    public bool active;
    public int inno;

    public Gene(int inno, int inNode, int outNode, float weight, bool active)
    {
        this.inNode = inNode;
        this.outNode = outNode;
        this.weight = weight;
        this.active = active;
        this.inno = inno;
    }

    public Gene(Gene copy)
    {
        this.inNode = copy.inNode;
        this.outNode = copy.outNode;
        this.weight = copy.weight;
        this.active = copy.active;
        this.inno = copy.inno;
    }
}