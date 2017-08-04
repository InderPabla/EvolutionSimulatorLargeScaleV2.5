using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Linq;
using System.IO;


//Fast neural network based on matrix operations 
public class NEATBrain : IEquatable<NEATBrain>
{
    private string name;
    private int ID;
    private int calculations;
    private int numberOfInputNeurons;
    private int numberOfOutputNeurons;
    private int numberOfHiddenNeurons;

    private int usedHiddenNeuronIndex;
    private List<Gene> genome = new List<Gene>();
    private HashSet<int> genomeHash;

    private List<Neuron> network = new List<Neuron>();
    private Neuron[] networkArray;
    
    
    private NEATConsultor consultor;

    private BehaviourGenome behaviourGenome;

    //Strc Gene
    /*private genestruc[][] networkStrucArray;
    private float[] nodes;
    public struct genestruc
    {
        public int index;
        public float weight;
    }*/

    public NEATBrain(int numberOfInputNeurons, int numberOfOutputNeurons, int numberOfHiddenNeurons, int ID)
    {
        consultor = NEATConsultor.GetInstance();
        this.ID = ID;
        this.numberOfHiddenNeurons = numberOfHiddenNeurons;
        this.numberOfInputNeurons = numberOfInputNeurons;
        this.numberOfOutputNeurons = numberOfOutputNeurons;
        this.numberOfInputNeurons++; //bias

        this.usedHiddenNeuronIndex = this.numberOfOutputNeurons + this.numberOfInputNeurons;

        GenerateRandomName();

        genomeHash = new HashSet<int>();
       
        
        for (int i = 0; i < 2; i++)
            Mutate();

        InitilizeNeurons();
        MakeNetwork();

        behaviourGenome = new BehaviourGenome(null,1f);
    }

    public NEATBrain(int ID, string name, int numberOfInputNeurons, int numberOfOutputNeurons, int numberOfHiddenNeurons, int usedHiddenNeuronIndex,  List<Gene> genomeCopy, BehaviourGenome behaviourGenome)
    {
        this.ID = ID;
        this.name = name;
        this.numberOfInputNeurons = numberOfInputNeurons +1;
        this.numberOfOutputNeurons = numberOfOutputNeurons;
        this.numberOfHiddenNeurons = numberOfHiddenNeurons;
        this.usedHiddenNeuronIndex = usedHiddenNeuronIndex;
        this.behaviourGenome = behaviourGenome;

        InitilizeGenomeFromParent(genomeCopy);
        InitilizeNeurons();
        MakeNetwork();
    }

    public NEATBrain(NEATBrain copy, Neuron[] neuronArray)
    {
        consultor = NEATConsultor.GetInstance();
        this.numberOfHiddenNeurons = copy.numberOfHiddenNeurons;
        this.numberOfInputNeurons = copy.numberOfInputNeurons;
        this.numberOfOutputNeurons = copy.numberOfOutputNeurons;
        this.usedHiddenNeuronIndex = copy.usedHiddenNeuronIndex;

        this.networkArray = neuronArray;
    }

    // Deep copy constructor of a given Brain
    public NEATBrain(NEATBrain parentBrain, int ID)
    {
        consultor = NEATConsultor.GetInstance();
        this.ID = ID;
        this.calculations = parentBrain.calculations;
        this.name = parentBrain.name;
        this.numberOfHiddenNeurons = parentBrain.numberOfHiddenNeurons;
        this.numberOfInputNeurons = parentBrain.numberOfInputNeurons;
        this.numberOfOutputNeurons = parentBrain.numberOfOutputNeurons;
        this.usedHiddenNeuronIndex = parentBrain.usedHiddenNeuronIndex;

        InitilizeGenomeFromParent(parentBrain.genome);
        
        Mutate();
        InitilizeNeurons();
        MakeNetwork();

        behaviourGenome = new BehaviourGenome(parentBrain.behaviourGenome,true,1f);
    }

    // Deep copy constructor of a given Brain
    public NEATBrain(NEATBrain parentBrain, List<Gene>genomeCopy, int ID)
    {
        consultor = NEATConsultor.GetInstance();
        this.ID = ID;
        this.calculations = parentBrain.calculations;
        this.name = parentBrain.name;
        this.numberOfHiddenNeurons = parentBrain.numberOfHiddenNeurons;
        this.numberOfInputNeurons = parentBrain.numberOfInputNeurons;
        this.numberOfOutputNeurons = parentBrain.numberOfOutputNeurons;
        this.usedHiddenNeuronIndex = parentBrain.usedHiddenNeuronIndex;

        InitilizeGenomeFromParent(genomeCopy);
        
        Mutate();
        InitilizeNeurons();
        MakeNetwork();

       
        behaviourGenome = new BehaviourGenome(parentBrain.behaviourGenome, true,1f);
    }

    public void InitilizeNeurons()
    {
        for (int i = 0; i < usedHiddenNeuronIndex; i++)
        {
            Neuron neuron = new Neuron(i, 0f);
            network.Add(neuron);
        }
    }

    private void MakeNetwork()
    {
        calculations = 0;
        genome.Sort((x, y) => x.outNode.CompareTo(y.outNode));
        /*HashSet<int> tempNeuronHash = new HashSet<int>();

        for (int i = 0; i < numberOfInputNeurons + numberOfOutputNeurons; i++)
        {
            tempNeuronHash.Add(i);
        }

        for (int i = 0; i < genome.Count; i++)
        {
            Gene gene = genome[i];
            if (!tempNeuronHash.Contains(gene.inNode))
            {
                Neuron neuron = new Neuron(gene.inNode,0f);
                tempNeuronHash.Add(gene.inNode);
                network.Add(neuron);
            }

            if (!tempNeuronHash.Contains(gene.outNode))
            {
                Neuron neuron = new Neuron(gene.outNode,0f);
                tempNeuronHash.Add(gene.outNode);
                network.Add(neuron);
            }
        }*/

        network.Sort((x, y) => x.id.CompareTo(y.id));

        for (int i = 0; i < genome.Count; i++)
        {
            Gene gene = genome[i];

            if (gene.active == true)
            {
                network[gene.outNode].incomming.Add(gene);
                calculations++;
            }
        }

        networkArray = network.ToArray();

        for (int i = 0; i < networkArray.Length; i++)
        {
            networkArray[i].incomming.Sort((x, y) => x.inNode.CompareTo(y.inNode));
            networkArray[i].incommingArray = networkArray[i].incomming.ToArray();
        }


            //Strc Gene
            /*List<genestruc[]> fa = new List<genestruc[]>();

            for (int i = 0; i < networkArray.Length; i++)
            {
                networkArray[i].incomming.Sort((x, y) => x.inNode.CompareTo(y.inNode));
                networkArray[i].incommingArray = networkArray[i].incomming.ToArray();



                genestruc[] a = new genestruc[networkArray[i].incommingArray.Length];

                for (int j = 0; j < networkArray[i].incommingArray.Length;j++)
                {

                    a[j] = new genestruc();
                    a[j].index = networkArray[i].incommingArray[j].inNode;
                    a[j].weight = networkArray[i].incommingArray[j].weight;

                }

                fa.Add(a);
            }
            nodes = new float[networkArray.Length];
            networkStrucArray = fa.ToArray();*/


        }

    private void InitilizeGenomeFromParent(List<Gene> parentGenome)
    {
        genomeHash = new HashSet<int>();

        for (int i = 0; i < parentGenome.Count; i++)
        {
            Gene gene = parentGenome[i];
            AddGene(gene.inno, gene.inNode, gene.outNode, gene.weight, gene.active);
        }
    }

    public List<Gene> GetGenome()
    {
        return genome;
    }

    public int GetUsedHiddenNeuronCount()
    {
        return usedHiddenNeuronIndex;
    }

    private int GetGeneHashValue(int inNode, int outNode)
    {
        return ((GetIntegerHashMultiplyFactor(inNode + 1, outNode + 1) * (inNode + 1)) + (outNode + 1));
    }

    private int GetIntegerHashMultiplyFactor(int int1, int int2)
    {
        int integer = int1 > int2 ? int1 : int2;

        if (integer >= 100)
            return 1000;
        else if (integer >= 10)
            return 100;
        else
            return 10;
    }

    private void AddGene(int inNode, int outNode, float weight, bool active)
    {
        int inno = consultor.GetInnovationNumber(inNode,outNode);
        //Debug.Log(inno+" "+ inNode+" "+outNode);
        Gene gene = new Gene(inno, inNode, outNode, weight, active);
        genome.Add(gene);
        int geneHashValue = GetGeneHashValue(inNode, outNode);
        genomeHash.Add(geneHashValue);
    }

    private void AddGene(int inno, int inNode, int outNode, float weight, bool active)
    {
        Gene gene = new Gene(inno, inNode, outNode, weight, active);
        genome.Add(gene);
        int geneHashValue = GetGeneHashValue(inNode, outNode);
        genomeHash.Add(geneHashValue);
    }

    public void RemoveGene(int index)
    {
        int geneHashValue = GetGeneHashValue(genome[index].inNode, genome[index].outNode);
        genome.RemoveAt(index);
        genomeHash.Remove(geneHashValue);
    }


    //hyperbolic tangent activation
    private float Tanh(float value)
    {
        return (float)Math.Tanh(value);
    }

    //random name generation, with atlest 1 vowel per 3 letters 
    private void GenerateRandomName()
    {
        int nameSize = UnityEngine.Random.Range(3, 11);
        char[] name = new char[nameSize];

        int[] vowels = new int[] { 97, 101, 105, 111, 117 };
        int vowelCounter = 1;
        for (int i = 0; i < name.Length; i++)
        {
            int charNum = UnityEngine.Random.Range(97, 123);

            bool isVowel = false;
            for (int j = 0; j < vowels.Length; j++)
            {
                if (charNum == vowels[j])
                {
                    isVowel = true;
                    break;
                }
            }

            if (isVowel)
            {
                vowelCounter = 1;
            }
            else if (vowelCounter == 3)
            {
                vowelCounter = 1;
                charNum = vowels[UnityEngine.Random.Range(0, vowels.Length)];
            }

            vowelCounter++;
            name[i] = (char)charNum;
        }
        this.name = new string(name);
    }

    public string GetName()
    {
        return name;
    }

    public bool Equals(NEATBrain other)
    {
        if (other == null)
            return false;

        return (other.ID == this.ID);
    }

    public void SetID(int ID)
    {
        this.ID = ID;
    }

    public int GetNumberOfInputNeurons()
    {
        return numberOfInputNeurons;
    }

    public int GetNumberOfUsedHiddenNeurons()
    {
        return usedHiddenNeuronIndex-(numberOfInputNeurons+numberOfOutputNeurons);
    }

    public int GetNumberOfOutputNeurons()
    {
        return numberOfOutputNeurons;
    }

    public Neuron[] GetNetworkArray()
    {
        return networkArray;
    }

    public void Mutate()
    {
        bool initialConnection = false;
        int[] numberOfConnectedInputNeurons = new int[numberOfInputNeurons];
        for (int i = 0;i<genome.Count;i++)
        {
            Gene gene = genome[i];
            if (gene.inNode < numberOfInputNeurons && gene.active == true)
            {
                numberOfConnectedInputNeurons[gene.inNode]++;
            }
        }
        for (int i = 0; i < numberOfInputNeurons; i++)
        {
            if (numberOfConnectedInputNeurons[i] == 0)
            {
                int inNode = i;
                int randomOutNode = UnityEngine.Random.Range(numberOfInputNeurons, usedHiddenNeuronIndex);
                bool ret1 = CheckIfGeneExists(inNode, randomOutNode);
                if (ret1 == false)  //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                {
                    AddGene(inNode, randomOutNode, UnityEngine.Random.Range(-0.5f, 0.5f), true);
                }
                initialConnection = true;
            }
        }

       if (initialConnection == false)
       {
            if (genome.Count > 0)
            {
                float randomMutateType = UnityEngine.Random.Range(1f, 100f);

                if (randomMutateType <= 1f)
                {
                    if (genome.Count >= (numberOfInputNeurons * numberOfOutputNeurons))
                    {
                        int randomIndex = UnityEngine.Random.Range(0, genome.Count);
                        RemoveGene(randomIndex);
                    }
                }
                else if (randomMutateType <= 50f)
                {
                    float randomMutateValue = UnityEngine.Random.Range(1f, 100f);
                    if (randomMutateValue <=50)
                    {
                        
                        if (usedHiddenNeuronIndex < (numberOfOutputNeurons + numberOfHiddenNeurons + numberOfInputNeurons))
                        {
                            int randomIndex = UnityEngine.Random.Range(0, genome.Count);
                            Gene gene = genome[randomIndex];
                            gene.active = false;

                            AddGene(gene.inNode, usedHiddenNeuronIndex, /*UnityEngine.Random.Range(-0.5f, 0.5f)*/ 1f, true);
                            AddGene(usedHiddenNeuronIndex, gene.outNode, gene.weight, true);
                            
                            usedHiddenNeuronIndex++;
                        }
                    }
                    else if (randomMutateValue <=100)
                    {
                        int randomInNode = UnityEngine.Random.Range(0, usedHiddenNeuronIndex);
                        int randomOutNode = UnityEngine.Random.Range(numberOfInputNeurons, usedHiddenNeuronIndex);
                        bool ret1 = CheckIfGeneExists(randomInNode, randomOutNode);
                        bool ret2 = CheckIfGeneExists(randomOutNode, randomInNode);
                        bool ret3 = CheckIfGeneExists(randomInNode, randomInNode);
                        bool ret4 = CheckIfGeneExists(randomOutNode, randomOutNode);

                        if (ret1 == true && ret2 == false && randomInNode >= numberOfInputNeurons)
                        {
                            AddGene(randomOutNode, randomInNode, UnityEngine.Random.Range(-0.5f, 0.5f), true);
                        }
                        else if (ret1 == false)
                        {
                            AddGene(randomInNode, randomOutNode, UnityEngine.Random.Range(-0.5f, 0.5f), true);
                        }
                        else if (ret3 == true && randomInNode >= numberOfInputNeurons)
                        {
                            //AddGene(randomInNode, randomInNode, UnityEngine.Random.Range(-0.5f, 0.5f), true);
                        }
                        else if (ret4 == false)
                        {
                            //AddGene(randomOutNode, randomOutNode, UnityEngine.Random.Range(-0.5f, 0.5f), true);
                        }
                        else
                        {
                            //int randomIndex = UnityEngine.Random.Range(0, genome.Count);
                            //genome[randomIndex].active = !genome[randomIndex].active;
                            //if (genome[randomIndex].active == false)
                            //genome[randomIndex].active = true;
                        }
                    }
                }
            }
            else
            {
                int randomInNode = UnityEngine.Random.Range(0, usedHiddenNeuronIndex);
                int randomOutNode = UnityEngine.Random.Range(numberOfInputNeurons, usedHiddenNeuronIndex);
                bool ret1 = CheckIfGeneExists(randomInNode, randomOutNode);
                bool ret2 = CheckIfGeneExists(randomOutNode, randomInNode);
                bool ret3 = CheckIfGeneExists(randomInNode, randomInNode);
                bool ret4 = CheckIfGeneExists(randomOutNode, randomOutNode);

                if (ret1 == true && ret2 == false && randomInNode >= numberOfInputNeurons)
                {
                    AddGene(randomOutNode, randomInNode, UnityEngine.Random.Range(-0.5f, 0.5f), true);
                }
                else if (ret1 == false)
                {
                    AddGene(randomInNode, randomOutNode, UnityEngine.Random.Range(-0.5f, 0.5f), true);
                }
                else if (ret3 == true && randomInNode >= numberOfInputNeurons)
                {
                    //AddGene(randomInNode, randomInNode, UnityEngine.Random.Range(-0.5f, 0.5f), true);
                }
                else if (ret4 == false)
                {
                    //AddGene(randomOutNode, randomOutNode, UnityEngine.Random.Range(-0.5f, 0.5f), true);
                }
            }
        }

        MutateGenes();
        MutateName();
    }

    private void MutateGenes()
    {
        int numberOfGenes = genome.Count; //number of genes

        for (int i = 0; i < numberOfGenes; i++)
        { //run through all genes
            Gene gene = genome[i]; // get gene at index i
            float weight = 0;

            int randomNumber = UnityEngine.Random.Range(1, 101); //random number between 1 and 100

            if (randomNumber <= 1)
            { //if 1
                //flip sign of weight
                weight = gene.weight;
                weight *= -1f;
                gene.weight = weight;
            }
            else if (randomNumber <= 2)
            { //if 2
                //pick random weight between -1 and 1
                weight = UnityEngine.Random.Range(-0.5f, 0.5f);
                gene.weight = weight;
            }
            else if (randomNumber <= 3)
            { //if 3
                //randomly increase by 0% to 100%
                float factor = UnityEngine.Random.Range(0f, 1f) + 1f;
                weight = gene.weight * factor;
                gene.weight = weight;
            }
            else if (randomNumber <= 4)
            { //if 4
                //randomly decrease by 0% to 100%
                float factor = UnityEngine.Random.Range(0f, 1f);
                weight = gene.weight * factor;
                gene.weight = weight;
            }
            else if (randomNumber <= 5)
            { //if 5
                //flip activation state for gene
                //gene.active = !gene.active;
            }

            if (gene.weight > 3f)
            {
                gene.weight = 3f;
            }
            else if (gene.weight < -3f)
            {
                gene.weight = -3f;
            }
        }

    }

    public void MutateExistingNetwork()
    {
        for (int i = 0; i < networkArray.Length; i++)
        {
            Gene[] imcomming = networkArray[i].incommingArray;

            for (int j = 0; j < imcomming.Length; j++)
            {
                Gene gene = imcomming[j]; // get gene at index i
                float weight = 0;

                int randomNumber = UnityEngine.Random.Range(1, 251); //random number between 1 and 100

                if (randomNumber <= 1)
                { //if 1
                  //flip sign of weight
                    weight = gene.weight;
                    weight *= -1f;
                    gene.weight = weight;
                }
                else if (randomNumber <= 2)
                { //if 2
                  //pick random weight between -1 and 1
                    weight = UnityEngine.Random.Range(-0.5f, 0.5f);
                    gene.weight = weight;
                }
                else if (randomNumber <= 3)
                { //if 3
                  //randomly increase by 0% to 100%
                    float factor = UnityEngine.Random.Range(0f, 1f) + 1f;
                    weight = gene.weight * factor;
                    gene.weight = weight;
                }
                else if (randomNumber <= 4)
                { //if 4
                  //randomly decrease by 0% to 100%
                    float factor = UnityEngine.Random.Range(0f, 1f);
                    weight = gene.weight * factor;
                    gene.weight = weight;
                }
                else if (randomNumber <= 5)
                { //if 5
                  //flip activation state for gene
                  gene.active = !gene.active;
                }

                if (gene.weight > 3f)
                {
                    gene.weight = 3f;
                }
                else if (gene.weight < -3f)
                {
                    gene.weight = -3f;
                }
            }
        }
    }

    public bool CheckIfGeneExists(int inNode, int outNode)
    {
        return genomeHash.Contains(GetGeneHashValue(inNode, outNode));
    }

    public int GetCalculations()
    {
        return calculations;
    }

    private void MutateName()
    {
        //Mutate name
        int index = UnityEngine.Random.Range(0, name.Length);
        char[] nameChar = name.ToCharArray();
        List<char> nameCharList = new List<char>(nameChar);

        int randomNumber = UnityEngine.Random.Range(0, 3);
        if (randomNumber == 0)
        {
            nameChar[index] = (char)UnityEngine.Random.Range(97, 123);
            name = new string(nameChar);
        }
        else if (randomNumber == 1)
        {
            if (nameCharList.Count >= 4)
            {
                nameCharList.RemoveAt(UnityEngine.Random.Range(0, nameCharList.Count));

            }
            else
            {
                nameCharList.Add((char)UnityEngine.Random.Range(97, 123));
            }

            nameChar = nameCharList.ToArray();
            name = new string(nameChar);
        }
        else if (randomNumber == 2)
        {

            if (nameCharList.Count < 10)
            {
                int locationToAdd = UnityEngine.Random.Range(0, nameCharList.Count);
                nameCharList.Insert(locationToAdd, (char)UnityEngine.Random.Range(97, 123));
            }
            else
            {
                nameCharList.RemoveAt(UnityEngine.Random.Range(0, nameCharList.Count));
            }

            nameChar = nameCharList.ToArray();
            name = new string(nameChar);

        }
    }

    public float[] GetOutput()
    {
        //Strc Gene
        /*float[] output = new float[numberOfOutputNeurons];
        for (int i = 0; i < output.Length; i++)
        {
            output[i] = nodes[i + numberOfInputNeurons];
        }*/

        float[] output = new float[numberOfOutputNeurons];
        for (int i = 0; i < output.Length; i++)
        {
            output[i] = networkArray[i + numberOfInputNeurons].value;

        }

        return output;
    }

    public float Sigmoid(float value)
    {
        return 2f / (1f + (float)Math.Exp(-2f * value)) - 1f;
    }

    private float activation(float value, int type)
    {
        switch (type)
        {
            case 0: return Tanh(value);
            case 1: return Mathf.Sin(value);
            case 2: return Sigmoid(value);
            default: return Tanh(value);
        }
    }

    public void FeedForward()
    {
        float[] output = new float[numberOfOutputNeurons];

        float[] tempValues = new float[networkArray.Length];
        for (int i = 0; i < tempValues.Length; i++)
            tempValues[i] = networkArray[i].value;

        networkArray[numberOfInputNeurons - 1].value = 1f;

        for (int i = 0; i < networkArray.Length; i++)
        {
            float value = 0;
            Neuron neuron = networkArray[i];
            Gene[] incommingArray = neuron.incommingArray;

            if (incommingArray.Length > 0)
            {
                for (int j = 0; j < incommingArray.Length; j++)
                {
                    if (incommingArray[j].active == true)
                    {
                        value = value + (incommingArray[j].weight * tempValues[incommingArray[j].inNode]);
                    }
                }
                neuron.value = Tanh(value);
            }
        }
    }

    public float[] FeedForward(float[] inputs)
    {
        for (int i = 0; i < inputs.Length; i++)
        {
            networkArray[i].value = inputs[i];

        }

        float[] output = new float[numberOfOutputNeurons];

        float[] tempValues = new float[networkArray.Length];
        for (int i = 0; i < tempValues.Length;i++)
            tempValues[i] = networkArray[i].value;

        networkArray[numberOfInputNeurons - 1].value = 1f;

        for (int i = 0; i < networkArray.Length; i++)
        {
            float value = 0;
            Neuron neuron = networkArray[i];
            Gene[] incommingArray = neuron.incommingArray;

            if (incommingArray.Length > 0)
            {
                for (int j = 0; j < incommingArray.Length; j++)
                {
                    if (incommingArray[j].active == true)
                    {
                        value = value + (incommingArray[j].weight * tempValues[incommingArray[j].inNode]);
                    }
                }
                neuron.value = Tanh(value);
            }
        }
        return GetOutput();


        /*for (int i = 0; i < inputs.Length; i++)
        {
            nodes[i] = inputs[i];
        }

        float[] tempValues = new float[nodes.Length];
        for (int i = 0; i < tempValues.Length; i++)
            tempValues[i] = nodes[i];

        for (int i = 0; i < nodes.Length; i++)
        {
            float value = 0;
            
            if (networkStrucArray[i].Length > 0)
            {
                for (int j = 0; j < networkStrucArray[i].Length; j++)
                {
                    value = value + (networkStrucArray[i][j].weight * tempValues[networkStrucArray[i][j].index]);
                }
                nodes[i] = Tanh(value);
            }
        }

        return nodes;*/
    }

    public void SetInputs(float[] inputs) {
        for (int i = 0; i < inputs.Length; i++)
        {
            networkArray[i].value = inputs[i];
        }
    }

    internal static float BrainSimilarityScore(NEATBrain net1, NEATBrain net2)
    {
        //Debug.Log("___________________________________________");
        Hashtable geneHash = new Hashtable(); //hash table to be used to compared genes from the two networks
        Gene[] geneValue; //will be used to check whether a gene exists in both networks

        List<Gene> geneList1 = net1.genome; //get first network
        List<Gene> geneList2 = net2.genome; //get second network

        ICollection keysCol; //will be used to get keys from gene hash
        int[] keys; //will be used to get keys arrray from ICollections

        int numberOfGenes1 = geneList1.Count; //get number of genes in network 1
        int numberOfGenes2 = geneList2.Count; //get number of genes in network 2
        int largerGenomeSize = numberOfGenes1 > numberOfGenes2 ? numberOfGenes1 : numberOfGenes2; //get one that is larger between the 2 network
        int excessGenes = 0; //number of excess genes (genes that do match and are outside the innovation number of the other network)
        int disjointGenes = 0; //number of disjoint gene (genes that do not match in the two networks)
        int equalGenes = 0; //number of genes both neural network have

        float disjointCoefficient = 0.85f; //get disjoint coefficient from consultor
        float excessCoefficient = 1f; //get excess coefficient from consultor
        float averageWeightDifferenceCoefficient = 1f; //get average weight difference coefficient

        float similarity = 0; //similarity of the two networks 
        float averageWeightDifference = 0; //average weight difference of the two network's equal genes

        bool foundAllExcess = false; //if all excess genes are found
        bool isFirstGeneExcess = false; //if net 1 contains the excess genes

        for (int i = 0; i < geneList1.Count; i++)
        { //run through net 1's genes
            int innovation = geneList1[i].inno; //get innovation number of gene
            //Debug.Log(innovation+" "+ geneList1[i].inNode+" "+ geneList1[i].outNode);
            geneValue = new Gene[] { geneList1[i], null }; //add into the hash with innovation number as the key and gene array of size 2 as value 

            //try {
            geneHash.Add(innovation, geneValue);  //add into the hash with innovation number as the key and gene array of size 2 as value
            /*}
            catch (ArgumentException error)
            {
                Debug.Log(innovation+" "+ geneList1[i].inNode+" "+geneList1[i].outNode+" ERRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRROR");
            }*/
        }

        for (int i = 0; i < geneList2.Count; i++)
        { //run through net 2's genes
            int innovation = geneList2[i].inno; //get innovation number of gene

            if (!geneHash.ContainsKey(innovation))
            { //if innovation key does not exist
                geneValue = new Gene[] { null, geneList2[i] }; //create array of size 2 with new gene in the second position
                geneHash.Add(innovation, geneValue); //add into  the hash with innovation number as the key and gene array of size 2 as value
            }
            else
            { //key exists
                geneValue = (Gene[])geneHash[innovation]; //get value
                geneValue[1] = geneList2[i]; //add into second position net 2's gene
            }
        }

        keysCol = geneHash.Keys; //get all keys from gene hash
        keys = new int[keysCol.Count]; //create array with size of number of keys
        keysCol.CopyTo(keys, 0); //copy all keys from ICollections to array
        keys = keys.OrderBy(i => i).ToArray(); //order keys in ascending order

        for (int i = keys.Length - 1; i >= 0; i--)
        { //run through all keys backwards (to get all excess gene's first)
            geneValue = (Gene[])geneHash[keys[i]]; //get value with key

            if (foundAllExcess == false)
            { //if all excess genes have not been found
                if (i == keys.Length - 1 && geneValue[1] == null)
                { //this is the first itteration and second gene location is null
                    isFirstGeneExcess = true; //excess genes exit in net 1
                }

                if (isFirstGeneExcess == true && geneValue[1] == null)
                { //excess gene exist in net 1 and there is no gene in second location of the value
                    excessGenes++; //this is an excess gene and increment excess gene
                }
                else if (isFirstGeneExcess == false && geneValue[0] == null)
                { //excess gene exist in net 12 and there is no gene in first location of the value
                    excessGenes++; //this is an excess gene and increment excess gene
                }
                else
                { //no excess genes
                    foundAllExcess = true; //all excess genes are found
                }

            }

            if (foundAllExcess == true)
            { //if all excess genes are found
                if (geneValue[0] != null && geneValue[1] != null)
                { //both gene location are not null
                    equalGenes++; //increment equal genes
                    averageWeightDifference += Mathf.Abs(geneValue[0].weight - geneValue[1].weight); //add absolute difference between 2 weight
                }
                else
                { //this is disjoint gene
                    disjointGenes++; //increment disjoint
                }
            }
        }

        averageWeightDifference = averageWeightDifference / (float)equalGenes; //get average weight difference of equal genes

        //similarity formula -> Sim = (AVG_DIFF * AVG_COFF) + (((DISJ*DISJ_COFF) + (EXSS*EXSS_COFF)) /GENOME_SIZE)
        similarity = (averageWeightDifference * averageWeightDifferenceCoefficient) + //calculate weight difference disparity
                     (((float)disjointGenes * disjointCoefficient) / (float)largerGenomeSize) +  //calculate disjoint disparity
                     (((float)excessGenes * excessCoefficient) / (float)largerGenomeSize); //calculate excess disparity

        //if similairty is <= to threshold then return true, otherwise false
        return similarity; //return boolean compare value

    }

    internal static NEATBrain MakeDifferentialBrain(NEATBrain net1, NEATBrain net2)
    {
        NEATBrain difference = null;
        List<Neuron> differenceNeurons = new List<Neuron>();
        Neuron[] differenceNeuronsArray;

        bool net1IsSmaller = false;
        Neuron[] net1Neurons = net1.GetNetworkArray();
        Neuron[] net2Neurons = net2.GetNetworkArray();
        Neuron[] tempNeurons = net1Neurons;
        ICollection keysCol; 
        int[] keys; 

        if (net1Neurons.Length < net2Neurons.Length)
        {
            net1Neurons = net2Neurons;
            net2Neurons = tempNeurons;
            net1IsSmaller = true;
        }

        for (int i = 0; i < net1Neurons.Length; i++)
        {
            Neuron neuron = new Neuron(net1Neurons[i].id,0f);
            Hashtable geneHash = new Hashtable();
            for (int j = 0; j < net1Neurons[i].incommingArray.Length; j++)
            {
                geneHash.Add(net1Neurons[i].incommingArray[j].inno, new Gene[] { net1Neurons[i].incommingArray[j], null});
            }

            if (i <= net2Neurons.Length - 1)
            {
                for (int j = 0; j < net2Neurons[i].incommingArray.Length; j++)
                {
                    if (geneHash.ContainsKey(net2Neurons[i].incommingArray[j].inno))
                    {
                        Gene[] genes = (Gene[])geneHash[net2Neurons[i].incommingArray[j].inno];
                        genes[1] = net2Neurons[i].incommingArray[j];
                    }
                    else
                    {
                        geneHash.Add(net2Neurons[i].incommingArray[j].inno, new Gene[] { null, net2Neurons[i].incommingArray[j] });
                    }
                }
            }

            keysCol = geneHash.Keys; //get all keys from gene hash
            keys = new int[keysCol.Count]; //create array with size of number of keys
            keysCol.CopyTo(keys, 0); //copy all keys from ICollections to array
            keys = keys.OrderBy(x => x).ToArray(); //order keys in ascending order

            for (int j = 0; j < keys.Length; j++)
            {
                Gene[] genes = (Gene[])geneHash[keys[j]];
                Gene gene = null;
                if (genes[0] != null)
                {
                    gene = new Gene(genes[0]);
                    if (genes[1] != null)
                    {
                        gene.weight = Mathf.Abs((gene.weight + genes[1].weight)/2f);
                    }
                    else
                    {
                        gene.weight = -1 * Mathf.Abs(gene.weight);
                    }
                }
                else
                {
                    gene = new Gene(genes[1]);
                    if (genes[0] != null)
                    {
                        gene.weight = Mathf.Abs((gene.weight + genes[0].weight) / 2f); 
                    }
                    else
                    {
                        gene.weight = -1 * Mathf.Abs(gene.weight);
                    }
                }

                //gene.weight = Mathf.Abs(gene.weight);
                neuron.incomming.Add(gene);
            }

            differenceNeurons.Add(neuron);
        }

        differenceNeurons.Sort((x, y) => x.id.CompareTo(y.id));

        differenceNeuronsArray = differenceNeurons.ToArray();
        for (int i = 0; i < differenceNeuronsArray.Length; i++)
        {
            differenceNeuronsArray[i].incomming.Sort((x, y) => x.inNode.CompareTo(y.inNode));
            differenceNeuronsArray[i].incommingArray = differenceNeuronsArray[i].incomming.ToArray();
        }

        difference = new NEATBrain(net1IsSmaller == true? net2:net1, differenceNeuronsArray);

        return difference;
    }


    /// <summary>
    /// Corssover between two parents neural networks to create a child neural network.
    /// Crossover method is as described by the NEAT algorithm.
    /// </summary>
    /// <param name="parent1">Neural network parent</param>
    /// <param name="parent2">Neural network parent</param>
    /// <returns>Child neural network</returns>
    internal static NEATBrain Corssover(NEATBrain parent1, NEATBrain parent2, float parent1TimeLived, float parent2TimeLived, int ID)
    {
        NEATBrain child = null; //child to create

        Hashtable geneHash = new Hashtable(); //hash table to be used to compared genes from the two parents

        List<Gene> childGeneList = new List<Gene>(); //new gene child gene list to be created

        List<Gene> geneList1 = parent1.genome; //get gene list of the parent 1
        List<Gene> geneList2 = parent2.genome; //get gene list of parent 2

        NEATConsultor consultor = parent1.consultor; //get consultor (consultor is the same for all neural network as it's just a pointer location)

        int numberOfGenes1 = geneList1.Count; //get number of genes in parent 1
        int numberOfGenes2 = geneList2.Count; //get number of genes in parent 2
        int numberOfInputs = parent1.numberOfInputNeurons; //number of inputs (same for both parents)
        int numberOfOutputs = parent1.numberOfOutputNeurons; //number of outputs (same for both parents)

        bool net1IsSmaller = false;
        if (parent1.networkArray.Length < parent2.networkArray.Length)
        {
            net1IsSmaller = true;
        }

        for (int i = 0; i < numberOfGenes1; i++)
        { //run through all genes in parent 1
            geneHash.Add(geneList1[i].inno, new Gene[] { geneList1[i], null }); //add into the hash with innovation number as the key and gene array of size 2 as value
        }

        for (int i = 0; i < numberOfGenes2; i++)
        { //run through all genes in parent 2
            int innovationNumber = geneList2[i].inno; //get innovation number 

            if (geneHash.ContainsKey(innovationNumber) == true)
            { //if there is a key in the hash with the given innovation number
                Gene[] geneValue = (Gene[])geneHash[innovationNumber]; //get gene array value with the innovation key
                geneValue[1] = geneList2[i]; //since this array already contains value in first location, we can add the new gene in the second location
                geneHash.Remove(innovationNumber); //remove old value with the key
                geneHash.Add(innovationNumber, geneValue); //add new value with the key
            }
            else
            { //there exists no key with the given innovation number
                geneHash.Add(innovationNumber, new Gene[] { null, geneList2[i] }); //add into  the hash with innovation number as the key and gene array of size 2 as value
            }
        }

        ICollection keysCol = geneHash.Keys; //get all keys in the hash

        Gene gene = null; //

        int[] keys = new int[keysCol.Count]; //int array with size of nuumber of keys in the hash

        keysCol.CopyTo(keys, 0); //copy Icollentions keys list to keys array
        keys = keys.OrderBy(i => i).ToArray(); //order keys in asending order

        for (int i = 0; i < keys.Length; i++)
        { //run through all keys
            Gene[] geneValue = (Gene[])geneHash[keys[i]]; //get value at each index

            //compare value is used to compare gene activation states in each parent 
            int compareValue = -1;
            //0 = both genes are true, 1 = both are false, 2 = one is false other is true
            //3 = gene is dominant in one of the parents and is true, 4 = gene is dominant in one of the parents and is false

            if (geneValue[0] != null && geneValue[1] != null)
            { //gene eixts in both parents
                int randomIndex = UnityEngine.Random.Range(0, 2);

                if (geneValue[0].active == true && geneValue[1].active == true)
                { //gene is true in both
                    compareValue = 0; //set compared value to 0
                }
                else if (geneValue[0].active == false && geneValue[1].active == false)
                { //gene is false in both
                    compareValue = 1; //set compared value to 1
                }
                else
                { //gene is true in one and false in the other
                    compareValue = 2; //set compared value to 2
                }

                gene = CrossoverCopyGene(geneValue[randomIndex], compareValue); //randomly pick a gene from eaither parent and create deep copy 
                childGeneList.Add(gene); //add gene to the child gene list
            }
            else if (parent1TimeLived > parent2TimeLived)
            { //parent 1's fitness is greater than parent 2
                if (geneValue[0] != null)
                { //gene value at first index from parent 1 exists
                    if (geneValue[0].active == true)
                    { //gene is active
                        compareValue = 3; //set compared value to 3
                    }
                    else
                    { //gene is not active
                        compareValue = 4; //set compared value to 4
                    }

                    gene = CrossoverCopyGene(geneValue[0], compareValue); //deep copy parent 1's gene
                    childGeneList.Add(gene); //add gene to the child gene list
                }
            }
            else if (parent1TimeLived < parent2TimeLived)
            { //parent 2's fitness is greater than parent 1
                if (geneValue[1] != null)
                { //gene value at second index from parent 2 exists
                    if (geneValue[1].active == true)
                    { //gene is active
                        compareValue = 3; //set compared value to 3
                    }
                    else
                    { //gene is not active
                        compareValue = 4; //set compared value to 4
                    }

                    gene = CrossoverCopyGene(geneValue[1], compareValue); //deep copy parent 2's gene 
                    childGeneList.Add(gene); //add gene to the child gene list
                }
            }
            else if (geneValue[0] != null)
            { //both parents have equal fitness and gene value at first index from parent 1 exists
                if (geneValue[0].active == true)
                { //gene is active
                    compareValue = 3; //set compared value to 3
                }
                else
                { //gene is not active
                    compareValue = 4; //set compared value to 4
                }

                gene = CrossoverCopyGene(geneValue[0], compareValue); //deep copy parent 1's gene 
                childGeneList.Add(gene); //add gene to the child gene list
            }
            else if (geneValue[1] != null)
            { //both parents have equal fitness and gene value at second index from parent 2 exists
                if (geneValue[1].active == true)
                { //gene is active
                    compareValue = 3; //set compared value to 3
                }
                else
                { //gene is not active
                    compareValue = 4; //set compared value to 4
                }

                gene = CrossoverCopyGene(geneValue[1], compareValue); //deep copy parent 2's gene 
                childGeneList.Add(gene); //add gene to the child gene list
            }
        }

        child = new NEATBrain(net1IsSmaller == true ? parent2 : parent1, childGeneList, ID); //create new child neural network 
        return child; //return newly created neural network
    }

    /// <summary>
    /// Created a deep copy of a given gene. 
    /// This gene can be muated with a small chance based on the compare value. 
    /// Deactivated genes have a small chance of being activated based on the compare value. 
    /// </summary>
    /// <param name="copyGene">Gene to deep copy</param>
    /// <param name="compareValue">Value to use when activating a gene</param>
    /// <returns>Deep copied gene</returns>
    private static Gene CrossoverCopyGene(Gene copyGene, int compareValue)
    {
        Gene gene = new Gene(copyGene); //deep copy gene 

        int factor = 2;
        if (compareValue == 1)
        {
            int randomNumber = UnityEngine.Random.Range(0, 25 * factor);
            if (randomNumber == 0)
            {
                gene.active = false;
            }
        }
        else if (compareValue == 2)
        {
            int randomNumber = UnityEngine.Random.Range(0, 10 * factor);
            if (randomNumber == 0)
            {
                gene.active = true;
            }
        }
        else
        {
            int randomNumber = UnityEngine.Random.Range(0, 25 * factor);
            if (randomNumber == 0)
            {
                gene.active = !gene.active;
            }
        }

        return gene; //return new gene
    }

    public BehaviourGenome GetBehaviourGenome()
    {
        return behaviourGenome;
    }

    public void StreamWriterNEATBrain(StreamWriter writer)
    {
        //Debug.Log(genome.Count+" "+usedHiddenNeuronIndex);
        writer.Write(genome.Count+" ");
       
        writer.Write(usedHiddenNeuronIndex + " ");

        for (int i = 0; i < genome.Count; i++)
        {
            Gene gene = genome[i];
            writer.Write(gene.inNode + " " + gene.outNode + " " + gene.weight + " " + (gene.active == true ? 1 : 0) + " ");
        }
    }

    


}
