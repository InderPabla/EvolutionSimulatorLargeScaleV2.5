using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
//using UnityEngine;

//Recurrent neural network
public class RNN : IEquatable<RNN>
{

    private string name;

    private int ID;

    private int calculations;

    private int[] hiddenLayers;
    private int[] hiddenLayerRNNFixed;

    private BehaviourGenome behaviourGenome;
    private float[][] neurons;
    private float[][][] weights;

    private float mutationNumber = 1000;
    private float mutationSign = 0.2f;
    private float mutationRandom = 0.2f;
    private float mutationIncrease = 1f;
    private float mutationDecrease = 1f;
    private float mutationWeakerParentFactor = 1;
    private float BIAS = 0.25f;

    private float speciesSimilarityScore = 0.0075f;

    private static Random random = new Random(DateTime.Now.Millisecond);
    
    public RNN(int ID, int[] hiddenLayers, float speciesSimilarityScore, float mutationNumber, float mutationSign, float mutationRandom, float mutationIncrease, float mutationDecrease, float mutationWeakerParentFactor, float[] initialVisionAngles, float minVisionLength)
    {

        this.ID = ID;
        this.speciesSimilarityScore = speciesSimilarityScore;
        this.mutationNumber = mutationNumber;
        this.mutationSign = mutationSign;
        this.mutationRandom = mutationRandom;
        this.mutationIncrease = mutationIncrease;
        this.mutationDecrease = mutationDecrease;
        this.mutationWeakerParentFactor = mutationWeakerParentFactor;


        this.hiddenLayers = new int[hiddenLayers.Length];
        this.hiddenLayerRNNFixed = new int[hiddenLayers.Length];

        for (int i = 0; i < hiddenLayers.Length; i++)
        {
            this.hiddenLayers[i] = hiddenLayers[i];

            if (i < hiddenLayers.Length - 2)
            {
                this.hiddenLayerRNNFixed[i] = hiddenLayers[i] + hiddenLayers[i + 1];
            }
            else
            {
                this.hiddenLayerRNNFixed[i] = hiddenLayers[i];
            }

            if (i > 0)
                calculations += (hiddenLayers[i] * this.hiddenLayerRNNFixed[i - 1]);

        }

        /*for (int i = 0; i < hiddenLayers.Length; i++)
        {
            Debug.Log(this.hiddenLayers[i] + " " + hiddenLayerRNNFixed[i]);

        }*/

        behaviourGenome = new BehaviourGenome(initialVisionAngles, minVisionLength);

        InitilizeNeurons();
        InitilizeWeights();
        GenerateRandomName();
    }

    public RNN(int ID, int[] hiddenLayers, float speciesSimilarityScore, float[][][] weights, string name, BehaviourGenome genome, float mutationNumber, float mutationSign, float mutationRandom, float mutationIncrease, float mutationDecrease, float mutationWeakerParentFactor, float minVisionLength)
    {
        this.ID = ID;
        this.speciesSimilarityScore = speciesSimilarityScore;
        this.speciesSimilarityScore = speciesSimilarityScore;
        this.mutationNumber = mutationNumber;
        this.mutationSign = mutationSign;
        this.mutationRandom = mutationRandom;
        this.mutationIncrease = mutationIncrease;
        this.mutationDecrease = mutationDecrease;
        this.mutationWeakerParentFactor = mutationWeakerParentFactor;



        this.hiddenLayers = new int[hiddenLayers.Length];
        this.hiddenLayerRNNFixed = new int[hiddenLayers.Length];

        for (int i = 0; i < hiddenLayers.Length; i++)
        {
            this.hiddenLayers[i] = hiddenLayers[i];

            if (i < hiddenLayers.Length - 2)
            {
                this.hiddenLayerRNNFixed[i] = hiddenLayers[i] + hiddenLayers[i + 1];
            }
            else
            {
                this.hiddenLayerRNNFixed[i] = hiddenLayers[i];
            }

            if (i > 0)
                calculations += (hiddenLayers[i] * this.hiddenLayerRNNFixed[i - 1]);

        }


        behaviourGenome = new BehaviourGenome(genome, false, minVisionLength);

        InitilizeNeurons();
        InitilizeWeights();
        GenerateRandomName();

        for (int i = 0; i < this.weights.Length; i++)
        {
            for (int j = 0; j < this.weights[i].Length; j++)
            {
                for (int k = 0; k < this.weights[i][j].Length; k++)
                {

                    this.weights[i][j][k] = weights[i][j][k];
                }
            }
        }

        this.name = name;
    }

    // Deep copy constructor of a given Brain
    public RNN(RNN parentBrain, int ID, float speciesSimilarityScore, float minVisionLength)
    {
        this.ID = ID;
        this.speciesSimilarityScore = speciesSimilarityScore;

        this.calculations = parentBrain.calculations;

        //deep copy layers array
        this.hiddenLayers = new int[parentBrain.hiddenLayers.Length];
        for (int i = 0; i < hiddenLayers.Length; i++)
        {
            this.hiddenLayers[i] = parentBrain.hiddenLayers[i];
        }

        //deep copy layers array
        this.hiddenLayerRNNFixed = new int[parentBrain.hiddenLayerRNNFixed.Length];
        for (int i = 0; i < hiddenLayerRNNFixed.Length; i++)
        {
            this.hiddenLayerRNNFixed[i] = parentBrain.hiddenLayerRNNFixed[i];
        }

        //deep copy neurons
        this.neurons = new float[parentBrain.neurons.Length][];
        for (int i = 0; i < this.neurons.Length; i++)
        {
            this.neurons[i] = (float[])parentBrain.neurons[i].Clone();
        }
        //InitilizeNeurons();  //make neurons 0's 

        //deep copy weights
        this.weights = new float[parentBrain.weights.Length][][];
        for (int i = 0; i < this.weights.Length; i++)
        {
            float[][] parentNeuronWeightsOfLayer = parentBrain.weights[i];
            float[][] weightsOfLayer = new float[parentNeuronWeightsOfLayer.Length][];

            for (int j = 0; j < weightsOfLayer.Length; j++)
            {
                weightsOfLayer[j] = (float[])parentNeuronWeightsOfLayer[j].Clone();
            }

            this.weights[i] = weightsOfLayer;
        }

        this.name = parentBrain.name;

        this.mutationNumber = parentBrain.mutationNumber;
        this.mutationSign = parentBrain.mutationSign;
        this.mutationRandom = parentBrain.mutationRandom;
        this.mutationIncrease = parentBrain.mutationIncrease;
        this.mutationDecrease = parentBrain.mutationDecrease;
        this.mutationWeakerParentFactor = parentBrain.mutationWeakerParentFactor;

        this.behaviourGenome = new BehaviourGenome(parentBrain.behaviourGenome, true, minVisionLength);
    }



    private void InitilizeNeurons()
    {

        //Neuron Initilization
        List<float[]> neuronsList = new List<float[]>();

        for (int i = 0; i < hiddenLayerRNNFixed.Length; i++) //run through all layers
        {
            neuronsList.Add(new float[hiddenLayerRNNFixed[i]]); //add layer to neuron list
        }
        neurons = neuronsList.ToArray(); //convert list to array
    }

    //create a static weights matrix
    private void InitilizeWeights()
    {
        //Weights Initilization

        List<float[][]> weightsList = new List<float[][]>();

        for (int i = 1; i < neurons.Length; i++)
        {
            List<float[]> layerWeightsList = new List<float[]>(); //layer weights list



            for (int j = 0; j < neurons[i].Length; j++)
            {
                int neuronsInPreviousLayer = hiddenLayerRNNFixed[i - 1];
                /* if (!(i < neurons.Length - 1 && j>=(hiddenLayerRNNFixed[i - 1]-hiddenLayers[i-1])))
                 {
                     //neuronsInPreviousLayer = hiddenLayers[i];
                     float[] neuronWeights = new float[neuronsInPreviousLayer]; //neruons weights

                     //set the weights randomly between 1 and -1
                     for (int k = 0; k < neuronsInPreviousLayer; k++)
                     {
                         neuronWeights[k] = UnityEngine.Random.Range(-0.5f, 0.5f);
                     }

                     layerWeightsList.Add(neuronWeights);
                 }*/

                if ((i >= neurons.Length - 2) || (j < hiddenLayers[i]))
                {
                    //neuronsInPreviousLayer = hiddenLayers[i];
                    float[] neuronWeights = new float[neuronsInPreviousLayer]; //neruons weights

                    //set the weights randomly between 1 and -1
                    for (int k = 0; k < neuronsInPreviousLayer; k++)
                    {
                        neuronWeights[k] = (float)random.NextDouble() + (-0.5f); //UnityEngine.Random.Range(-0.5f, 0.5f);
                    }

                    layerWeightsList.Add(neuronWeights);
                }
                else
                {
                    float[] neuronWeights = new float[0]; //neruons weights

                    layerWeightsList.Add(neuronWeights);
                }


            }
            weightsList.Add(layerWeightsList.ToArray());
        }

        weights = weightsList.ToArray(); //convert list to array
                                         /*if (ID == 0)
                                         {
                                             int count = 0;
                                             for (int i = 0; i < weights.Length; i++)
                                             {
                                                 for (int j = 0; j < weights[i].Length; j++)
                                                 {
                                                     for (int k = 0; k < weights[i][j].Length; k++)
                                                     {
                                                         count++;
                                                     }
                                                 }
                                             }
                                             Debug.Log(count);
                                         }*/

        /*List<float[][]> weightsList = new List<float[][]>();

        for (int i = 1; i < neurons.Length; i++)
        {
            List<float[]> layerWeightsList = new List<float[]>(); //layer weights list

            int neuronsInPreviousLayer = hiddenLayerRNNFixed[i - 1];

          
            for (int j = 0; j < neurons[i].Length; j++)
            {
                float[] neuronWeights = new float[neuronsInPreviousLayer]; //neruons weights

                //set the weights randomly between 1 and -1
                for (int k = 0; k < neuronsInPreviousLayer; k++)
                {
                    neuronWeights[k] = UnityEngine.Random.Range(-0.5f, 0.5f);
                }

                layerWeightsList.Add(neuronWeights);
            }
            weightsList.Add(layerWeightsList.ToArray());
        }

        weights = weightsList.ToArray(); //convert list to array*/
    }


    //random name generation, with atlest 1 vowel per 3 letters 
    private void GenerateRandomName()
    {
        int nameSize = (int)(random.NextDouble() * (11f - 3f) + 3f);//UnityEngine.Random.Range(3, 11);
        char[] name = new char[nameSize];

        int[] vowels = new int[] { 97, 101, 105, 111, 117 };
        int vowelCounter = 1;
        for (int i = 0; i < name.Length; i++)
        {
            int charNum = ((int)(random.NextDouble() * (123 - 97) + (97)));

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
                charNum = vowels[(int)(random.NextDouble() * (float)vowels.Length)];
            }

            vowelCounter++;
            name[i] = (char)charNum;
        }
        this.name = new string(name);
    }

    public float[] GetOutput()
    {
        return neurons[hiddenLayerRNNFixed.Length - 1]; //return output field
    }

    public bool Equals(RNN other)
    {
        if (other == null)
            return false;

        return (other.ID == this.ID);
    }

    public string GetName()
    {
        return name;
    }

    public int GetNumberOfInputNeurons()
    {
        return hiddenLayers[0];
    }

    public void SetID(int ID)
    {
        this.ID = ID;
    }

    public void Mutate(float[][][] parentWeights, float mutationRatio)
    {
  
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    int randomNumber = (int)(random.NextDouble() * 1000);

                    if (randomNumber < mutationWeakerParentFactor)
                    {
                        weights[i][j][k] = parentWeights[i][j][k];
                    }
                }
            }
        }

        Mutate();
    }


    public void Mutate()
    {
        MutateWeights();
        MutateName();
    }

    public void MutateWeights()
    {
 
        //Mutate weight, each weight has a 4%
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    float weight = weights[i][j][k];

                    float randomNumber1 = (float)random.NextDouble() * mutationNumber; 
                    if (randomNumber1 <= mutationSign)
                    { //if 1
                      //flip sign of weight
                        weight *= -1f; 
                    }
                    else if (randomNumber1 <= mutationSign + mutationRandom)
                    { //if 2
                      //pick random weight between -1 and 1
                        weight = (float)random.NextDouble() + (-0.5f); //count++;
                    }
                    else if (randomNumber1 <= mutationSign + mutationRandom + mutationIncrease)
                    { //if 3
                      //randomly increase by 0% to 100%
                        float factor = (float)random.NextDouble() + 1f;
                        weight *= factor; 
                    }
                    else if (randomNumber1 <= mutationSign + mutationRandom + mutationIncrease + mutationDecrease)
                    { //if 4
                      //randomly decrease by 0% to 100%
                        float factor = (float)random.NextDouble();
                        weight *= factor; 
                    }



                    /*if (weight > 3f)
                    {
                        weight = 3f;
                    }
                    else if (weight < -3f) {
                        weight = -3f;
                    }*/

                    weights[i][j][k] = weight;
                }
            }
        }


    }

    public static float maxMutations = 0;

    public void MutateName()
    {
        //Mutate name
        int index = (int)(random.NextDouble() * (double)name.Length);
        char[] nameChar = name.ToCharArray();
        List<char> nameCharList = new List<char>(nameChar);

        int randomNumber = (int)(random.NextDouble() * 3);
        if (randomNumber == 0)
        {
            nameChar[index] = (char)((int)(random.NextDouble() * (123 - 97) + (97)));  //UnityEngine.Random.Range(97, 123);
            name = new string(nameChar);
        }
        else if (randomNumber == 1)
        {
            if (nameCharList.Count >= 4)
            {
                nameCharList.RemoveAt((int)(random.NextDouble() * nameCharList.Count));

            }
            else
            {
                nameCharList.Add((char)((int)(random.NextDouble() * (123 - 97) + (97))));
            }

            nameChar = nameCharList.ToArray();
            name = new string(nameChar);
        }
        else if (randomNumber == 2)
        {

            if (nameCharList.Count < 10)
            {
                int locationToAdd = (int)(random.NextDouble() * nameCharList.Count);
                nameCharList.Insert(locationToAdd, (char)((int)(random.NextDouble() * (123 - 97) + (97))));
            }
            else
            {
                nameCharList.RemoveAt((int)(random.NextDouble() * nameCharList.Count));
            }

            nameChar = nameCharList.ToArray();
            name = new string(nameChar);

        }
    }


    public float[][] GetNeurons()
    {
        return neurons;
    }

    //hyperbolic tangent activation
    private float Tanh(float value)
    {
        return (float)Math.Tanh(value);


    }


    public float[][][] GetWeights()
    {
        return weights;
    }

    public int GetCalculations()
    {
        return calculations;
    }

    //neural network feedword by matrix operation
    public float[] FeedForward_Fast_Tanh(float[] inputs)
    {
        //add inputs to the neurons matrix
        for (int i = 0; i < inputs.Length; i++)
        {
            neurons[0][i] = inputs[i];
        }

        int weightLayerIndex = 0;

        // run through all neurons starting from the second layer
        for (int i = 1; i < neurons.Length; i++) //layers
        {
            //context layer copy
            if (i < hiddenLayers.Length - 1)
            {
                /*int k = 0;
                for (int j = hiddenLayers[i - 1]; j < hiddenLayerRNNFixed[i - 1]; j++, k++)
                {
                    neurons[i - 1][j] = neurons[i][k];
                }*/
                Buffer.BlockCopy(neurons[i], 0, neurons[i - 1], hiddenLayers[i - 1] * 4, (hiddenLayerRNNFixed[i - 1] - hiddenLayers[i - 1]) * 4);
            }

            for (int j = 0; j < hiddenLayers[i]; j++) //neurons of this layers minus the context neurons!
            {
                float value = BIAS;

                for (int k = 0; k < neurons[i - 1].Length; k++) //neurons of the previous layer
                {
                    value += weights[weightLayerIndex][j][k] * neurons[i - 1][k];
                }

                if (value < -3)
                    neurons[i][j] = - 1;
                else if (value > 3)
                    neurons[i][j] = 1;
                else
                    neurons[i][j] = value * (27 + value * value) / (27 + 9 * value * value);

            }

            weightLayerIndex++;
        }

        return neurons[hiddenLayerRNNFixed.Length - 1]; //return output field
    }

    public float[] FeedForward_Slow_Tanh(float[] inputs)
    {
        //add inputs to the neurons matrix
        for (int i = 0; i < inputs.Length; i++)
        {
            neurons[0][i] = inputs[i];
        }

        int weightLayerIndex = 0;

        // run through all neurons starting from the second layer
        for (int i = 1; i < neurons.Length; i++) //layers
        {
            //context layer copy
            if (i < hiddenLayers.Length - 1)
            {
                /*int k = 0;
                for (int j = hiddenLayers[i - 1]; j < hiddenLayerRNNFixed[i - 1]; j++, k++)
                {
                    neurons[i - 1][j] = neurons[i][k];
                }*/
                Buffer.BlockCopy(neurons[i], 0, neurons[i - 1], hiddenLayers[i - 1] * 4, (hiddenLayerRNNFixed[i - 1] - hiddenLayers[i - 1]) * 4);
            }

            for (int j = 0; j < hiddenLayers[i]; j++) //neurons of this layers minus the context neurons!
            {
                float value = BIAS;

                for (int k = 0; k < neurons[i - 1].Length; k++) //neurons of the previous layer
                {
                    value += weights[weightLayerIndex][j][k] * neurons[i - 1][k];
                }

               
                neurons[i][j] = (float)Math.Tanh(value);
            }

            weightLayerIndex++;
        }

        return neurons[hiddenLayerRNNFixed.Length - 1]; //return output field
    }

    //neural network feedword by matrix operation
    public void FeedForward()
    {
        // int weightLayerIndex = 0;

        // run through all neurons starting from the second layer
        for (int i = 1; i < neurons.Length; i++) //layers
        {
            //context layer copy
            if (i < hiddenLayers.Length - 1)
            {
                /*int k = 0;
                for (int j = hiddenLayers[i - 1]; j < hiddenLayerRNNFixed[i - 1]; j++, k++)
                {
                    neurons[i - 1][j] = neurons[i][k];
                }*/
                //Buffer.BlockCopy(neurons[i], 0, neurons[i - 1], hiddenLayers[i - 1] * 4, (hiddenLayerRNNFixed[i - 1] - hiddenLayers[i - 1]) * 4);
            }

            for (int j = 0; j < hiddenLayers[i]; j++) //neurons of this layers minus the context neurons!
            {
                float value = BIAS;

                for (int k = 0; k < neurons[i - 1].Length; k++) //neurons of the previous layer
                {
                    value += weights[i - 1][j][k] * neurons[i - 1][k];
                }

                //neurons[i][j] = (float)Math.Tanh(value);
            }

            //weightLayerIndex++;
        }
    }

    public static float[][][] CreateDifferenceConnectionMatrix(RNN net1, RNN net2)
    {

        float[][][] connection1 = net1.weights;
        float[][][] connection2 = net2.weights;
        float speciesSimilarityScore = net1.speciesSimilarityScore;

        float[][][] difference = new float[connection1.Length][][];
        for (int i = 0; i < connection1.Length; i++)
        {
            float[][] parentNeuronWeightsOfLayer = connection1[i];
            float[][] weightsOfLayer = new float[parentNeuronWeightsOfLayer.Length][];

            for (int j = 0; j < weightsOfLayer.Length; j++)
            {
                weightsOfLayer[j] = (float[])parentNeuronWeightsOfLayer[j].Clone();

                for (int k = 0; k < weightsOfLayer[j].Length; k++)
                {
                    //weightsOfLayer[j][k] = Math.Abs(connection1[i][j][k] - connection2[i][j][k]) > speciesSimilarityScore ? 1 : 0;
                    weightsOfLayer[j][k] = connection1[i][j][k] == connection2[i][j][k] ? 1f : -1f;
                }
            }

            difference[i] = weightsOfLayer;
        }



        return difference;
    }

    public static float BrainSimilarityScore(RNN net1, RNN net2)
    {
        float averageWeightDifference = 0; //average weight difference of the two network's equal genes
        float[][][] connection1 = net1.GetWeights();
        float[][][] connection2 = net2.GetWeights();
        int[] hiddenLayers = net1.GetHiddenLayers();
        float count = 0;
        float eqCount = 0;
        for (int layerIndex = 0; layerIndex < connection1.Length; layerIndex++)
        {

            int numOfCons = connection1[layerIndex].Length;

            if (layerIndex < connection1.Length - 2)
            {
                numOfCons = hiddenLayers[layerIndex + 1];
            }

            for (int neuronOfLayerIndex = 0; neuronOfLayerIndex < numOfCons; neuronOfLayerIndex++)
            {

                for (int previousLayerNeuronIndex = 0; previousLayerNeuronIndex < connection1[layerIndex][neuronOfLayerIndex].Length; previousLayerNeuronIndex++)
                {
                    // connections[layerIndex][neuronOfLayerIndex][previousLayerNeuronIndex]
                    averageWeightDifference += Math.Abs(connection1[layerIndex][neuronOfLayerIndex][previousLayerNeuronIndex] - connection2[layerIndex][neuronOfLayerIndex][previousLayerNeuronIndex]);
                    count++;

                    if (connection1[layerIndex][neuronOfLayerIndex][previousLayerNeuronIndex] == connection2[layerIndex][neuronOfLayerIndex][previousLayerNeuronIndex])
                        eqCount++;
                }
            }
        }
        averageWeightDifference = averageWeightDifference / count;

        return /*averageWeightDifference*/ eqCount / net1.calculations;

    }


    public void SetInputs(float[] inputs)
    {
        //add inputs to the neurons matrix
        for (int i = 0; i < inputs.Length; i++)
        {
            neurons[0][i] = inputs[i];
        }
    }

    public BehaviourGenome GetBehaviourGenome()
    {
        return behaviourGenome;
    }

    public void MutateExistingNetwork()
    {
        int randomNum = (int)(random.NextDouble() * 100f);//UnityEngine.Random.Range(0,100);
        if (randomNum <= 50)
        {
            for (int i = 0; i < weights.Length; i++)
            {
                for (int j = 0; j < weights[i].Length; j++)
                {
                    for (int k = 0; k < weights[i][j].Length; k++)
                    {
                        float weight = weights[i][j][k];

                        float randomNumber1 = (float)random.NextDouble() * mutationNumber;//UnityEngine.Random.Range(1, mutationNumber); //random number between 1 and 100
                        if (randomNumber1 <= 5f)
                        { //if 1
                          //flip sign of weight
                            weight *= -1f;
                        }
                        else if (randomNumber1 <= 10f)
                        { //if 2
                          //pick random weight between -1 and 1
                            weight = (float)random.NextDouble() + (-0.5f); //UnityEngine.Random.Range(-0.5f, 0.5f);
                        }
                        else if (randomNumber1 <= 15f)
                        { //if 3
                          //randomly increase by 0% to 100%
                            float factor = (float)random.NextDouble() + 1f;
                            weight *= factor;
                        }
                        else if (randomNumber1 <= 20f)
                        { //if 4
                          //randomly decrease by 0% to 100%
                            float factor = (float)random.NextDouble();
                            weight *= factor;
                        }




                        weights[i][j][k] = weight;
                    }
                }
            }
        }

    }

    public int[] GetHiddenLayers()
    {
        return hiddenLayers;
    }

    public void StreamWriterRNNBrain(StreamWriter writer)
    {
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {

                    writer.Write(weights[i][j][k] + " ");
                }
            }
        }
    }

    public void mutation_number(float value)
    {
        mutationNumber = value;
    }

    public void mutation_weaker_parent_factor(float value)
    {
        mutationWeakerParentFactor = value;
    }

    public void mutation_sign(float value)
    {
        mutationSign = value;
    }

    public void mutation_random(float value)
    {
        mutationRandom = value;
    }

    public void mutation_increase(float value)
    {
        mutationIncrease = value;
    }

    public void mutation_decrease(float value)
    {
        mutationDecrease = value;
    }

    public void species_similarity_score(float value)
    {
        speciesSimilarityScore = value;
    }
}
