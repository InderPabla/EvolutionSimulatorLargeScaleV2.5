using UnityEngine;
using System.Collections;
using System.IO;
using System.Linq;

public class NEATConsultor  {

    private static NEATConsultor consultor = null;

    private Hashtable consultorGenome;
    private static int innovation = 0;

    private NEATConsultor()
    {
        consultorGenome = new Hashtable();
    }

    public static NEATConsultor GetInstance()
    {
        if (consultor == null)
        {
            consultor = new NEATConsultor();
        }

        return consultor;
    }

    public void AddGene(Gene gene)
    {

        int key = GetGeneHashValue(gene.inNode, gene.outNode);
        if (consultorGenome.Contains(key) == false)
        {
            consultorGenome.Add(key, gene);
            if (gene.inno >= innovation)
            {
                innovation = gene.inno + 1;
            }
        }
    }

    public int GetInnovationNumber(int inNode, int outNode)
    {
        int key = GetGeneHashValue(inNode, outNode);
        if (consultorGenome.Contains(key) == true)
        {
            Gene gene = (Gene)consultorGenome[key];
            return gene.inno;
        }
        else
        {
            Gene gene = new Gene(innovation, inNode, outNode, 0f, true);
            consultorGenome.Add(key,gene);
            innovation++;
            return (innovation-1); 
        }

        
    }

    private int GetGeneHashValue(int inNode, int outNode)
    {
        return ((GetIntegerHashMultiplyFactor(inNode + 1, outNode + 1) *(inNode + 1)) + (outNode + 1));
    }

    private int GetIntegerHashMultiplyFactor(int int1,int int2)
    {
        int integer = int1 > int2 ? int1: int2;

        if (integer >= 100)
            return 1000;
        else if (integer >= 10)
            return 100;
        else
            return 10;
    }

    public void StreamWriteConsultorGenome(StreamWriter writer)
    {
        ICollection keysCol; //will be used to get keys from gene hash
        int[] keys; //will be used to get keys arrray from ICollections
        keysCol = consultorGenome.Keys; //get all keys from gene hash
        keys = new int[keysCol.Count]; //create array with size of number of keys
        keysCol.CopyTo(keys, 0); //copy all keys from ICollections to array
        keys = keys.OrderBy(i => i).ToArray(); //order keys in ascending order

        writer.Write(keys.Length+" ");
        //Debug.Log(keys.Length);
        for (int i = 0; i < keys.Length; i++)
        {
            Gene gene = (Gene)consultorGenome[keys[i]];
            writer.Write(gene.inno + " " + gene.inNode + " " + gene.outNode + " ");
        }
    }

}
