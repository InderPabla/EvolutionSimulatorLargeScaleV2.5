

using System.Collections.Generic;

public class Neuron
{
    public int id;
    public float value;
    public List<Gene> incomming = new List<Gene>();
    public Gene[] incommingArray;

    public Neuron(int id, float value)
    {
        this.id = id;
        this.value = value;
    }
}