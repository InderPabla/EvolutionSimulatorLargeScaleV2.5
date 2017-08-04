using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GUINetDraw : MonoBehaviour {
    public int createButtonState = 0;
    public int fastButtonState = 0;
    public int slowButtonState = 0;
    public int visionButtonState = 0;
    public int saveButtonState = 0;
    public int loadButtonState = 0;
    public int drawNetButtonState = 0;
    public int mapButtonState = 0;
    public bool isGUISelected = false;
    public bool drawNetState = false;

    private Texture2D texture;
    private RNN brain;
    private Creature_V2 creature;
    private BehaviourGenome behaviourGenome;
    private float screenWidth, screenHeight;

    /*public Neuron[] network;
    public Neuron[] differenceNetwork;*/
    private float[][] neurons;
    private float[][][] connections;
    private float[][][] differenceConnection;

    int numberOfInputNeurons;
    int numberOfOutputNeurons;
    int numberOfHiddenNeurons;
      
    private Color backgroundColor;
    private Color deadColor;
    private Color negativeLineColor;
    private Color positiveLineColor;
    private Color neuronColor;

    private float yOffset = 0f;
    private float highest = 0f;
    private List<TreeData> treeDataList;

    private int brainCalculations = 0;
    private int totalNumberOfCreatures = 0;
    private int creatureCount = 0;
    private float playSpeed = 1;
    
    private string[] inputNames = new string[] {"[Sensor_1_Dist]", "[Sensor_2_Dist]", "[Sensor_3_Dist]", "[Sensor_4_Dist]",
                                                 "[Sensor_1_Hue]", "[Sensor_2_Hue]", "[Sensor_3_Hue]", "[Sensor_4_Hue]","[Spike_Hue]",
                                                 "[Collide?]", "[Collide_Hue]", "[Tile_Hue]", "[Tile_Sat]", "[Left_Hue]", "[Left_Sat]", "[Right_Hue]", "[Right_Sat]",
                                                 /*"[Life]","[Energy]"*/"[Radius]", "[Mem_In_1]","[Mem_In_2]" };

    private string[] outputNames = new string[] {"[Velo_Fwd]","[Velo_Ang]","[MouthHue]","[BodyHue]","[Eat?]","[Birth?]","[Spike_Len]","[Mem_Out_1]","[Mem_Out_2]"};
    private int[] hiddenLayers;

    List<Species> speciesList = null;
    int speciesListCount = 0;
    bool started = false;

    public void SetStarted(bool started)
    {
        this.started = started;
    }

    // Use this for initialization
    void Start () {
        texture = new Texture2D(1,1);
        this.screenWidth = (float)Screen.width * 0.4f;
        this.screenHeight = Screen.height;

        backgroundColor = Color.grey; backgroundColor.a = 1f;
        negativeLineColor = Color.red; negativeLineColor.a = 1f;
        positiveLineColor = Color.green; positiveLineColor.a = 1f;
        neuronColor = Color.magenta; neuronColor.a = 1f;
        deadColor = new Color(0.5f,0f,0f); deadColor.a = 1f;
    }

    void OnGUI()
    {
        this.screenWidth = (float)Screen.width * 0.4f;
        this.screenHeight = Screen.height;

        if (drawNetButtonState == 2)
        {
            drawNetState = !drawNetState;
        }

        /*if (creature == null || creature.IsAlive())
            GUI.color = backgroundColor;
        else
            GUI.color = deadColor;*/

        GUI.color = backgroundColor;
        GUI.DrawTexture(new Rect(0, 0, screenWidth+10f, screenHeight), texture);

        /*if (!(creature == null || creature.IsAlive()))
        {
            GUI.color = deadColor;
            GUI.DrawTexture(new Rect(0, 0, screenWidth + 10f, (int)(screenHeight * 0.025f)), texture);
            
        }*/
        
       
        
        if (creatureCount > 0)
        {
            speciesListCount = speciesList.Count;

            float xOffset = 0;
            float width = ((screenWidth + 10f) / (float)creatureCount);
            float height = (screenHeight * 0.025f);
            if (neurons == null)
                height = (screenHeight * 0.25f);
            for (int i = 0; i < speciesList.Count; i++)
            {
                float totalWidth = width * speciesList[i].GetPopulation().Count;
                GUI.color = speciesList[i].GetColor();
                GUI.DrawTexture(new Rect(xOffset, 0, (int)totalWidth, (int)height), texture);
                xOffset += totalWidth;

            }
        }


        DrawBrain();
        DrawUI();
    }

    private void DrawBrain()
    {
        if (brain != null)
        {

            //float xOff = (int)(screenWidth / (neurons.Length - 1));
            float xOff = screenWidth * 0.025f;
            float yOff = (int)(screenHeight * 0.025f);
            float rectWidth = (int)((screenWidth * 0.075f));
            float rectHeight = (int)((rectWidth / 1.5f));


            GUIStyle myStyle = new GUIStyle();
            myStyle.fontStyle = FontStyle.Bold;
            myStyle.fontSize = (int)(screenWidth * 0.025f);

            for (int i = 0; i < treeDataList.Count; i++)
            {
                GUI.color = treeDataList[i].color;
                myStyle.normal.textColor = treeDataList[i].color;

                if (i == 0)
                {
                    string state = creature.IsAlive() == true ? "[ALIVE]" : "[DEAD]";

                    string cretureInformation = treeDataList[i].name + " " + state +
                                                "\n                                                         Parents: [" + creature.GetParentNames() + "]" +
                                                "\n                                                         Child Count: " + creature.GetChildCount() +
                                                "\n                                                         Generation: " + creature.GetGeneration() +
                                                "\n                                                         Time Lived: " + creature.GetTimeLived().ToString("0.000") +
                                                "\n                                                         Life: " + creature.GetLife().ToString("0.000") +
                                                "\n                                                         Energy: " + creature.GetEnergy().ToString("0.000") +
                                                "\n                                                         Delta: " + creature.GetDeltaEnergy() +
                                                "\n                                                         Egg Timer: " + behaviourGenome.eggBirthTimer+
                                                "\n                                                         Pred Level: " + behaviourGenome.predLevel+
                                                "\n                                                         Body Hue: " + behaviourGenome.bodyHue;

                    GUI.Label(new Rect(1f, screenHeight / 1.9f + rectHeight + i * (yOff / 1.5f), rectWidth, rectHeight), cretureInformation, myStyle);
                }
                else
                {
                    GUI.Label(new Rect(1f, screenHeight / 1.9f + rectHeight + i * (yOff / 1.5f), rectWidth, rectHeight), treeDataList[i].name, myStyle);
                }
            }

            /*Rect[] neuronPositions = new Rect[numberOfInputNeurons + numberOfHiddenNeurons + numberOfOutputNeurons];
            int neuronPositionIndex = 0;
            float yRatio = (screenHeight / 2f) / numberOfInputNeurons;
            float ypos = 0f;
            Rect neuronRect;

            for (int i = 0; i < numberOfInputNeurons; i++, neuronPositionIndex++)
            {
                ypos += yRatio;
                neuronRect = new Rect(xOff, ypos, rectWidth / 2f, rectHeight / 2f);
                neuronPositions[neuronPositionIndex] = neuronRect;
            }

            yRatio = (screenHeight / 2f) / numberOfOutputNeurons;
            ypos = 0f;
            for (int i = 0; i < numberOfOutputNeurons; i++, neuronPositionIndex++)
            {
                ypos += yRatio;
                neuronRect = new Rect(screenWidth - xOff - (rectWidth * 0.25f), ypos, rectWidth / 2f, rectHeight / 2f);
                neuronPositions[neuronPositionIndex] = neuronRect;
            }

            if (numberOfHiddenNeurons >= 0)
            {
                float quater = screenHeight / 4f;
                float centerX = screenWidth / 2f;
                float centerY = quater;
                float distance = rectWidth * 3f;
                float degreeDivision = 360f / numberOfHiddenNeurons;
                float degree = 0;

                for (int i = 0; i < numberOfHiddenNeurons; i++, neuronPositionIndex++)
                {
                    int resultX = (int)Mathf.Round(centerX + distance * Mathf.Sin(degree * Mathf.Deg2Rad));
                    int resultY = (int)Mathf.Round(centerY + distance * Mathf.Cos(degree * Mathf.Deg2Rad));
                    degree += degreeDivision;
                    neuronRect = new Rect(resultX - rectWidth / 2, resultY - rectHeight / 2, rectWidth / 2f, rectHeight / 2f);
                    neuronPositions[neuronPositionIndex] = neuronRect;
                }
            }*/

            xOff = (int)(screenWidth / (neurons.Length - 1));
            yOff = (int)(screenHeight * 0.025f);
            rectWidth = (int)(screenWidth * 0.075f);
            rectHeight = (int)(rectWidth / 1.5f);

            if (drawNetState == true) //Draw entire neural network
            {
                if (differenceConnection == null)
                {
                    for (int layerIndex = 0; layerIndex < connections.Length; layerIndex++)
                    {

                        float currentLayerYRatio = (screenHeight / 2f) / (float)connections[layerIndex].Length;
                        float currentXPos = (int)((layerIndex + 1) * xOff);
                        float previousXPos = (int)((layerIndex) * xOff);

                        int numOfCons = connections[layerIndex].Length;
                        if (layerIndex < connections.Length - 2)
                        {
                            numOfCons = hiddenLayers[layerIndex + 1];
                        }

                        //Debug.Log(numOfCons);

                        for (int neuronOfLayerIndex = 0; neuronOfLayerIndex < numOfCons; neuronOfLayerIndex++)
                        {

                            float previousLayerYRatio = (screenHeight / 2f) / (float)connections[layerIndex][neuronOfLayerIndex].Length;

                            for (int previousLayerNeuronIndex = 0; previousLayerNeuronIndex < connections[layerIndex][neuronOfLayerIndex].Length; previousLayerNeuronIndex++)
                            {
                                Vector2 pointA = new Vector2(currentXPos + (int)(rectWidth / 2f), currentLayerYRatio * neuronOfLayerIndex + yOff + (int)(rectHeight / 2f));
                                Vector2 pointB = new Vector2(previousXPos + (int)(rectWidth / 2f), previousLayerYRatio * previousLayerNeuronIndex + yOff + (int)(rectHeight / 2f));

                                Color lineColor = positiveLineColor;

                                if (connections[layerIndex][neuronOfLayerIndex][previousLayerNeuronIndex] <= 0f)
                                {
                                    lineColor = negativeLineColor;
                                }

                                Drawing.DrawLine(pointA, pointB, lineColor, 1f, texture);
                            }
                        }
                    }

                    //Debug.Log("--------");
                }
                else {
                    for (int layerIndex = 0; layerIndex < differenceConnection.Length; layerIndex++)
                    {

                        float currentLayerYRatio = (screenHeight / 2f) / (float)differenceConnection[layerIndex].Length;
                        float currentXPos = (int)((layerIndex + 1) * xOff);
                        float previousXPos = (int)((layerIndex) * xOff);

                        int numOfCons = differenceConnection[layerIndex].Length;

                        if (layerIndex < differenceConnection.Length - 2)
                        {
                            numOfCons = hiddenLayers[layerIndex + 1];
                        }

                        for (int neuronOfLayerIndex = 0; neuronOfLayerIndex < numOfCons; neuronOfLayerIndex++)
                        {

                            float previousLayerYRatio = (screenHeight / 2f) / (float)differenceConnection[layerIndex][neuronOfLayerIndex].Length;



                            for (int previousLayerNeuronIndex = 0; previousLayerNeuronIndex < differenceConnection[layerIndex][neuronOfLayerIndex].Length; previousLayerNeuronIndex++)
                            {
                                Vector2 pointA = new Vector2(currentXPos + (int)(rectWidth / 2f), currentLayerYRatio * neuronOfLayerIndex + yOff + (int)(rectHeight / 2f));
                                Vector2 pointB = new Vector2(previousXPos + (int)(rectWidth / 2f), previousLayerYRatio * previousLayerNeuronIndex + yOff + (int)(rectHeight / 2f));

                                Color lineColor = positiveLineColor;

                                float weight = differenceConnection[layerIndex][neuronOfLayerIndex][previousLayerNeuronIndex];

                                if (weight == -1)
                                    lineColor = Color.blue;
                                else if (weight == 1)
                                    lineColor = new Color(1f, 0.388f, 0.278f);
                                else
                                    lineColor = Color.red;

                                /*if (weight > 1)
                                    weight = 1;
                               
                                 lineColor = new Color((1f-weight), (1f-weight)/2f,weight*weight);*/


                                Drawing.DrawLine(pointA, pointB, lineColor, 1f, texture);
                            }
                        }
                    }
                }

                /*if (differenceNetwork != null)
                {
                    for (int i = 0; i < differenceNetwork.Length; i++)
                    {
                        Gene[] incomming = differenceNetwork[i].incommingArray;
                        for (int j = 0; j < incomming.Length; j++)
                        {
                            Gene gene = incomming[j];

                            Vector2 point1 = new Vector2((int)(neuronPositions[i].x + neuronPositions[i].width / 2), (int)(neuronPositions[i].y + neuronPositions[i].height / 2));
                            Vector2 point2 = new Vector2((int)(neuronPositions[gene.inNode].x + neuronPositions[i].width / 2), (int)(neuronPositions[gene.inNode].y + neuronPositions[i].height / 2));
                            Color color;

                            float weight = gene.weight;
                            float size = 2f;

                            if (weight < 0f)
                            {
                                color = Color.red;
                                size = Mathf.Abs(gene.weight) * 5f;
                                if (size < 1.5f)
                                    size = 1.5f;
                            }
                            else
                            {
                                weight = (float)Math.Tanh(weight);
                                color = new Color(weight, weight * weight, weight * weight);
                            }

                            if (point1.x == point2.x && point1.y == point2.y)
                            {
                                point2 = point1 + new Vector2((int)(neuronPositions[i].width * 2), (int)(neuronPositions[i].height / 2));
                                Vector3 point3 = point2 - new Vector2(0, neuronPositions[i].height);
                                Vector3 point4 = point1;

                                Drawing.DrawLine(point1, point2, color, size, texture);
                                Drawing.DrawLine(point2, point3, color, size, texture);
                                Drawing.DrawLine(point3, point4, color, size, texture);
                            }
                            else
                            {
                                Drawing.DrawLine(point1, point2, color, size, texture);
                            }
                        }
                    }
                }
                else
                {
                   // if (creature.IsAlive() == false)
                        //brain.MutateExistingNetwork();

                    for (int i = 0; i < network.Length; i++)
                    {
                        Gene[] incomming = network[i].incommingArray;
                        for (int j = 0; j < incomming.Length; j++)
                        {
                            Gene gene = incomming[j];

                            Vector2 point1 = new Vector2((int)(neuronPositions[i].x + neuronPositions[i].width / 2), (int)(neuronPositions[i].y + neuronPositions[i].height / 2));
                            Vector2 point2 = new Vector2((int)(neuronPositions[gene.inNode].x + neuronPositions[i].width / 2), (int)(neuronPositions[gene.inNode].y + neuronPositions[i].height / 2));

                            Color color;

                            if (gene.weight >= 0)
                                color = positiveLineColor;
                            else
                                color = negativeLineColor;

                            float size = Mathf.Abs(gene.weight) * 5f;
                            if (size < 1.5f)
                                size = 1.5f;

                            if (gene.active == false)
                            {
                                size = 2f;
                                color = Color.white;
                            }

                            if (point1.x == point2.x && point1.y == point2.y)
                            {
                                point2 = point1 + new Vector2((int)(neuronPositions[i].width * 2), (int)(neuronPositions[i].height / 2));
                                Vector3 point3 = point2 - new Vector2(0, neuronPositions[i].height);
                                Vector3 point4 = point1;

                                Drawing.DrawLine(point1, point2, color, size, texture);
                                Drawing.DrawLine(point2, point3, color, size, texture);
                                Drawing.DrawLine(point3, point4, color, size, texture);
                            }
                            else
                            {
                                Drawing.DrawLine(point1, point2, color, size, texture);
                            }
                        }
                    }
                }*/




            }
            else //Draw with mouse over only
            {
                /*Vector2 mousePosition = Event.current.mousePosition;

                for (int x = 0; x < neurons.Length; x++)
                {
                    float numberOfNeuronsInLayer = neurons[x].Length;
                    float yRatio = (screenHeight / 2f) / numberOfNeuronsInLayer;

                    float xpos = x * xOff;
                    float ypos = 0f;
                    for (int y = 0; y < numberOfNeuronsInLayer; y++)
                    {

                        ypos = y * yRatio + yOff;
                        Rect rec = new Rect(xpos, ypos, rectWidth, rectHeight);
                        if (rec.Contains(mousePosition) == true)
                        {
                            if (x == 0)
                            {
                                float nextLayerYRatio = (screenHeight / 2f) / (float)neurons[x + 1].Length;
                                float nextXPos = (int)((x + 1) * xOff);
                                float currentXPos = (int)((x) * xOff);

                                for (int z = 0; z < neurons[x + 1].Length; z++)
                                {
                                    Vector2 pointA = new Vector2(rec.x + (int)(rectWidth / 2f), rec.y + (int)(rectHeight / 2f));
                                    Vector2 pointB = new Vector2(nextXPos + (int)(rectWidth / 2f), nextLayerYRatio * z + yOff + (int)(rectHeight / 2f));

                                    Color lineColor = positiveLineColor;

                                    if (connections[x][z][y] <= 0f)
                                    {
                                        lineColor = negativeLineColor;
                                    }

                                    Drawing.DrawLine(pointA, pointB, lineColor, 1f, texture);
                                }
                                break;
                            }
                            else if (x == (neurons.Length - 1))
                            {
                                float nextLayerYRatio = (screenHeight / 2f) / (float)neurons[x - 1].Length;
                                float nextXPos = (int)((x - 1) * xOff);

                                for (int z = 0; z < neurons[x - 1].Length; z++)
                                {
                                    Vector2 pointA = new Vector2(rec.x + (int)(rectWidth / 2f), rec.y + (int)(rectHeight / 2f));
                                    Vector2 pointB = new Vector2(nextXPos + (int)(rectWidth / 2f), nextLayerYRatio * z + yOff + (int)(rectHeight / 2f));

                                    Color lineColor = positiveLineColor;

                                    if (connections[x - 1][y][z] <= 0f)
                                    {
                                        lineColor = negativeLineColor;
                                    }

                                    Drawing.DrawLine(pointA, pointB, lineColor, 1f, texture);
                                }

                                break;
                            }
                            else
                            {
                                float nextLayerYRatio = (screenHeight / 2f) / (float)neurons[x - 1].Length;
                                float nextXPos = (int)((x - 1) * xOff);

                                for (int z = 0; z < neurons[x - 1].Length; z++)
                                {
                                    Vector2 pointA = new Vector2(rec.x + (int)(rectWidth / 2f), rec.y + (int)(rectHeight / 2f));
                                    Vector2 pointB = new Vector2(nextXPos + (int)(rectWidth / 2f), nextLayerYRatio * z + yOff + (int)(rectHeight / 2f));

                                    Color lineColor = positiveLineColor;

                                    if (connections[x - 1][y][z] <= 0f)
                                    {
                                        lineColor = negativeLineColor;
                                    }

                                    Drawing.DrawLine(pointA, pointB, lineColor, 1f, texture);
                                }

                                float nextLayerYRatio2 = (screenHeight / 2f) / (float)neurons[x + 1].Length;
                                float nextXPos2 = (int)((x + 1) * xOff);

                                for (int z = 0; z < neurons[x + 1].Length; z++)
                                {
                                    Vector2 pointA = new Vector2(rec.x + (int)(rectWidth / 2f), rec.y + (int)(rectHeight / 2f));
                                    Vector2 pointB = new Vector2(nextXPos2 + (int)(rectWidth / 2f), nextLayerYRatio2 * z + yOff + (int)(rectHeight / 2f));

                                    Color lineColor = positiveLineColor;

                                    if (connections[x][z][y] <= 0f)
                                    {
                                        lineColor = negativeLineColor;
                                    }

                                    Drawing.DrawLine(pointA, pointB, lineColor, 1f, texture);
                                }
                                break;
                            }
                        }
                    }
                }*/

                

                /* Vector2 mousePosition = Event.current.mousePosition;
                for (int i = 0; i < network.Length; i++)
                {
                    if (neuronPositions[i].Contains(mousePosition) == true)
                    {
                        Gene[] incomming = network[i].incommingArray;
                        for (int j = 0; j < incomming.Length; j++)
                        {
                            Gene gene = incomming[j];
                            Vector2 point1 = new Vector2((int)(neuronPositions[i].x + neuronPositions[i].width / 2), (int)(neuronPositions[i].y + neuronPositions[i].height / 2));
                            Vector2 point2 = new Vector2((int)(neuronPositions[gene.inNode].x + neuronPositions[i].width / 2), (int)(neuronPositions[gene.inNode].y + neuronPositions[i].height / 2));
                            Color color;

                            if (gene.weight >= 0)
                                color = positiveLineColor;
                            else
                                color = negativeLineColor;

                            float size = Mathf.Abs(gene.weight) * 5f;
                            if (size < 1.5f)
                                size = 1.5f;

                            if (gene.active == false)
                            {
                                size = 2f;
                                color = Color.white;
                            }

                            if (point1.x == point2.x && point1.y == point2.y)
                            {
                                point2 = point1 + new Vector2((int)(neuronPositions[i].width * 2), (int)(neuronPositions[i].height / 2));
                                Vector3 point3 = point2 - new Vector2(0, neuronPositions[i].height);
                                Vector3 point4 = point1;

                                Drawing.DrawLine(point1, point2, color, size, texture);
                                Drawing.DrawLine(point2, point3, color, size, texture);
                                Drawing.DrawLine(point3, point4, color, size, texture);
                            }
                            else
                            {
                                Drawing.DrawLine(point1, point2, color, size, texture);
                            }
                        }

                        for (int j = 0; j < network.Length; j++)
                        {
                            if (j != i)
                            {
                                Gene[] incomming2 = network[j].incommingArray;
                                for (int k = 0; k < incomming2.Length; k++)
                                {
                                  
                                    Gene gene = incomming2[k];
                                    Vector2 point1 = new Vector2((int)(neuronPositions[i].x + neuronPositions[i].width / 2), (int)(neuronPositions[i].y + neuronPositions[i].height / 2));
                                    Vector2 point2 = new Vector2((int)(neuronPositions[gene.outNode].x + neuronPositions[i].width / 2), (int)(neuronPositions[gene.outNode].y + neuronPositions[i].height / 2));


                                    if (gene.active == true )
                                    {

                                        if (gene.inNode == i)
                                        {
                                            Color color;

                                            if (gene.weight >= 0)
                                                color = positiveLineColor;
                                            else
                                                color = negativeLineColor;

                                            float size = Mathf.Abs(gene.weight) * 5f;
                                            if (size < 1.5f)
                                                size = 1.5f;
                                            Drawing.DrawLine(point1, point2, color, size, texture);
                                        }
                                    }
                                    else
                                    {
                                        Drawing.DrawLine(point1, point2, Color.white, 2f, texture);
                                    }
                                }
                            }
                        }

                    } //this is the rect
                }*/


            }

            /*GUI.color = Color.white;
            if (differenceNetwork != null)
            {
                for (int i = 0; i < neuronPositions.Length; i++)
                {
                    GUI.DrawTexture(neuronPositions[i], texture);
                    myStyle.normal.textColor = Color.black;
                    GUI.Label(neuronPositions[i], differenceNetwork[i].value.ToString("0.000") + "", myStyle);

                }
            }
            else
            {
                for (int i = 0; i < neuronPositions.Length; i++)
                {
                    GUI.DrawTexture(neuronPositions[i], texture);
                    myStyle.normal.textColor = Color.black;
                    GUI.Label(neuronPositions[i], network[i].value.ToString("0.000") + "", myStyle);

                }
            }*/

            myStyle.fontStyle = FontStyle.Bold;
            myStyle.fontSize = (int)(screenWidth * 0.025f);

            for (int x = 0; x < neurons.Length; x++)
            {
                float numberOfNeuronsInLayer = neurons[x].Length;
                float yRatio = (screenHeight / 2f) / numberOfNeuronsInLayer;

                float xpos = x * xOff;
                float ypos = 0f;
                for (int y = 0; y < numberOfNeuronsInLayer; y++)
                {
                    ypos = y * yRatio + yOff;

                    if (y >= hiddenLayers[x])
                        GUI.color = Color.green;
                    else
                        GUI.color = Color.white;

                    GUI.DrawTexture(new Rect(xpos, ypos, rectWidth, rectHeight), texture);

                    myStyle.normal.textColor = Color.black;
                    GUI.Label(new Rect((int)(xpos), (int)(ypos + (rectHeight / 4f)), rectWidth, rectHeight), neurons[x][y].ToString("0.000") + "", myStyle);


                }
            }

        }

        /*if (brain != null)
        {

            float xOff = (int)(screenWidth / (neurons.Length - 1));
            float yOff = (int)(screenHeight * 0.025f);
            float rectWidth = (int)(screenWidth * 0.075f);
            float rectHeight = (int)(rectWidth / 1.5f);

            if (drawNetState == true)
            {
                for (int layerIndex = 0; layerIndex < connections.Length; layerIndex++)
                {

                    float currentLayerYRatio = (screenHeight / 2f) / (float)connections[layerIndex].Length;
                    float currentXPos = (int)((layerIndex + 1) * xOff);
                    float previousXPos = (int)((layerIndex) * xOff);

                    for (int neuronOfLayerIndex = 0; neuronOfLayerIndex < connections[layerIndex].Length; neuronOfLayerIndex++)
                    {

                        float previousLayerYRatio = (screenHeight / 2f) / (float)connections[layerIndex][neuronOfLayerIndex].Length;
                                               
                        for (int previousLayerNeuronIndex = 0; previousLayerNeuronIndex < connections[layerIndex][neuronOfLayerIndex].Length; previousLayerNeuronIndex++)
                        {
                            Vector2 pointA = new Vector2(currentXPos + (int)(rectWidth / 2f), currentLayerYRatio * neuronOfLayerIndex + yOff + (int)(rectHeight / 2f));
                            Vector2 pointB = new Vector2(previousXPos + (int)(rectWidth / 2f), previousLayerYRatio * previousLayerNeuronIndex + yOff + (int)(rectHeight / 2f));

                            Color lineColor = positiveLineColor;

                            if (connections[layerIndex][neuronOfLayerIndex][previousLayerNeuronIndex] <= 0f)
                            {
                                lineColor = negativeLineColor;
                            }

                            Drawing.DrawLine(pointA, pointB, lineColor, 2f, texture);
                        }
                    }
                }
            }
            else
            {
                Vector2 mousePosition = Event.current.mousePosition;

                for (int x = 0; x < neurons.Length; x++)
                {
                    float numberOfNeuronsInLayer = neurons[x].Length;
                    float yRatio = (screenHeight / 2f) / numberOfNeuronsInLayer;

                    float xpos = x * xOff;
                    float ypos = 0f;
                    for (int y = 0; y < numberOfNeuronsInLayer; y++)
                    {

                        ypos = y * yRatio + yOff;
                        Rect rec = new Rect(xpos, ypos, rectWidth, rectHeight);
                        if (rec.Contains(mousePosition) == true)
                        {
                            if (x == 0)
                            {
                                float nextLayerYRatio = (screenHeight / 2f) / (float)neurons[x+1].Length;
                                float nextXPos = (int)((x + 1) * xOff);
                                float currentXPos = (int)((x) * xOff);

                                for (int z = 0; z < neurons[x + 1].Length; z++)
                                {
                                    Vector2 pointA = new Vector2(rec.x + (int)(rectWidth / 2f), rec.y + (int)(rectHeight / 2f));
                                    Vector2 pointB = new Vector2(nextXPos + (int)(rectWidth / 2f), nextLayerYRatio * z + yOff + (int)(rectHeight / 2f));

                                    Color lineColor = positiveLineColor;

                                    if (connections[x][z][y] <= 0f)
                                    {
                                        lineColor = negativeLineColor;
                                    }

                                    Drawing.DrawLine(pointA, pointB, lineColor, 1f, texture);
                                }
                                break;
                            }
                            else if (x == (neurons.Length - 1))
                            {
                                float nextLayerYRatio = (screenHeight / 2f) / (float)neurons[x - 1].Length;
                                float nextXPos = (int)((x - 1) * xOff);

                                for (int z = 0; z < neurons[x - 1].Length; z++)
                                {
                                    Vector2 pointA = new Vector2(rec.x + (int)(rectWidth / 2f), rec.y + (int)(rectHeight / 2f));
                                    Vector2 pointB = new Vector2(nextXPos + (int)(rectWidth / 2f), nextLayerYRatio * z + yOff + (int)(rectHeight / 2f));

                                    Color lineColor = positiveLineColor;

                                    if (connections[x - 1][y][z] <= 0f)
                                    {
                                        lineColor = negativeLineColor;
                                    }

                                    Drawing.DrawLine(pointA, pointB, lineColor, 1f, texture);
                                }

                                break;
                            }
                            else
                            {
                                float nextLayerYRatio = (screenHeight / 2f) / (float)neurons[x - 1].Length;
                                float nextXPos = (int)((x - 1) * xOff);

                                for (int z = 0; z < neurons[x - 1].Length; z++)
                                {
                                    Vector2 pointA = new Vector2(rec.x + (int)(rectWidth / 2f), rec.y + (int)(rectHeight / 2f));
                                    Vector2 pointB = new Vector2(nextXPos + (int)(rectWidth / 2f), nextLayerYRatio * z + yOff + (int)(rectHeight / 2f));

                                    Color lineColor = positiveLineColor;

                                    if (connections[x - 1][y][z] <= 0f)
                                    {
                                        lineColor = negativeLineColor;
                                    }

                                    Drawing.DrawLine(pointA, pointB, lineColor, 1f, texture);
                                }

                                float nextLayerYRatio2 = (screenHeight / 2f) / (float)neurons[x + 1].Length;
                                float nextXPos2 = (int)((x + 1) * xOff);

                                for (int z = 0; z < neurons[x + 1].Length; z++)
                                {
                                    Vector2 pointA = new Vector2(rec.x + (int)(rectWidth / 2f), rec.y + (int)(rectHeight / 2f));
                                    Vector2 pointB = new Vector2(nextXPos2 + (int)(rectWidth / 2f), nextLayerYRatio2 * z + yOff + (int)(rectHeight / 2f));

                                    Color lineColor = positiveLineColor;

                                    if (connections[x][z][y] <= 0f)
                                    {
                                        lineColor = negativeLineColor;
                                    }

                                    Drawing.DrawLine(pointA, pointB, lineColor, 1f, texture);
                                }
                                break;   
                            }
                        }
                    }
                }

            }

            GUIStyle myStyle = new GUIStyle();
            myStyle.fontStyle = FontStyle.Bold;
            myStyle.fontSize = (int)(screenWidth * 0.025f);

            for (int x = 0; x < neurons.Length; x++)
            {
                float numberOfNeuronsInLayer = neurons[x].Length;
                float yRatio = (screenHeight / 2f) / numberOfNeuronsInLayer;

                float xpos = x * xOff;
                float ypos = 0f;
                for (int y = 0; y < numberOfNeuronsInLayer; y++)
                {
                    ypos = y*yRatio + yOff;
                    GUI.color = Color.white;
                    GUI.DrawTexture(new Rect(xpos , ypos, rectWidth, rectHeight), texture);
                    myStyle.normal.textColor = Color.black;
                    GUI.Label(new Rect((int)(xpos ), (int)(ypos + (rectHeight / 4f)), rectWidth, rectHeight), neurons[x][y].ToString("0.000") + "", myStyle);

                    if (x == 0)
                    {
                        myStyle.normal.textColor = Color.green;
                        GUI.Label(new Rect((int)(xpos + (rectWidth / 0.85f)), (int)(ypos + (rectHeight / 4f)), rectWidth, rectHeight), inputNames[y], myStyle);
                    }
                    else if (x == neurons.Length - 1)
                    {
                        myStyle.normal.textColor = Color.green;
                        GUI.Label(new Rect((int)(xpos - (rectWidth / 0.475f)), (int)(ypos + (rectHeight / 4f)), rectWidth, rectHeight), outputNames[y], myStyle);
                    }
                }
            }

            for (int i = 0; i < treeDataList.Count; i++)
            {
                myStyle.normal.textColor = treeDataList[i].color;

                if (i == 0)
                {
                    string state = creature.IsAlive() == true ? "[ALIVE]" : "[DEAD]";

                    string cretureInformation = treeDataList[i].name +" "+ state+
                                                "\n                                                         Parents: [" + creature.GetParentNames() + "]" +
                                                "\n                                                         Child Count: " + creature.GetChildCount() +
                                                "\n                                                         Generation: " + creature.GetGeneration() +
                                                "\n                                                         Time Lived: " + creature.GetTimeLived().ToString("0.000") +
                                                "\n                                                         Life: " + creature.GetLife().ToString("0.000") +
                                                "\n                                                         Energy: " + creature.GetEnergy().ToString("0.000") +
                                                "\n                                                         Delta: " + creature.GetDeltaEnergy();

                    GUI.Label(new Rect(1f, screenHeight / 1.9f + rectHeight + i * ( yOff / 1.5f), rectWidth, rectHeight), cretureInformation, myStyle);
                }
                else
                {
                    GUI.Label(new Rect(1f, screenHeight / 1.9f + rectHeight + i * (yOff / 1.5f), rectWidth, rectHeight), treeDataList[i].name, myStyle);
                }
            }
        }*/
    }

    public void DrawUI()
    {
        GUIStyle myStyle = new GUIStyle();
        myStyle.fontStyle = FontStyle.Bold;
        myStyle.fontSize = (int)(screenWidth * 0.05f);

        int buttonWidth = (int)(screenWidth * 0.2f);
        int buttonHeight = (int)(buttonWidth / 1.8f);

        //create button
        Rect createButton = new Rect(screenWidth - buttonWidth, screenHeight-buttonHeight, buttonWidth,  buttonHeight);

        if (Input.GetMouseButtonUp(0) && createButton.Contains(Event.current.mousePosition))
        {
            createButtonState++;
        }
        else if (createButton.Contains(Event.current.mousePosition))
        {
            GUI.color = Color.red;
            GUI.DrawTexture(createButton, texture);
            createButtonState = 0;
        }
        else
        {
            GUI.color = Color.blue;
            GUI.DrawTexture(createButton, texture);
            createButtonState = 0;
        }

        GUI.color = Color.white;
        myStyle.normal.textColor = Color.white;
        if (started == false)
        {
            GUI.Label(createButton, "Create", myStyle);
        }
        else
        {
            GUI.Label(createButton, "Settings", myStyle);
        }


        //play speed buttons
        GUI.color = Color.blue;

        Rect fastSpeedButton = new Rect(screenWidth - buttonWidth, screenHeight - buttonHeight*2f - 3, buttonWidth/2.5f, buttonHeight);
        Rect slowSpeedButton = new Rect(screenWidth - buttonWidth + buttonWidth/2.5f +(buttonWidth-(buttonWidth/2.5f * 2f)), screenHeight - buttonHeight*2f - 3, buttonWidth/2.5f, buttonHeight);

        if (Input.GetMouseButtonUp(0) && fastSpeedButton.Contains(Event.current.mousePosition))
        {
            fastButtonState++;
        }
        else if (fastSpeedButton.Contains(Event.current.mousePosition))
        {
            GUI.color = Color.red;
            GUI.DrawTexture(fastSpeedButton, texture);
            fastButtonState = 0;
        }
        else
        {
            GUI.color = Color.blue;
            GUI.DrawTexture(fastSpeedButton, texture);
            fastButtonState = 0;
        }

        if (Input.GetMouseButtonUp(0) && slowSpeedButton.Contains(Event.current.mousePosition))
        {
            slowButtonState++;
        }
        else if (slowSpeedButton.Contains(Event.current.mousePosition))
        {
            GUI.color = Color.red;
            GUI.DrawTexture(slowSpeedButton, texture);
            slowButtonState = 0;
        }
        else
        {
            GUI.color = Color.blue;
            GUI.DrawTexture(slowSpeedButton, texture);
            slowButtonState = 0;
        }

        //vision button
        Rect visionButton = new Rect(screenWidth - buttonWidth * 2 - 3, screenHeight - buttonHeight*2 - 3, buttonWidth, buttonHeight);
        if (Input.GetMouseButtonUp(0) && visionButton.Contains(Event.current.mousePosition))
        {
            visionButtonState++;
        }
        else if (visionButton.Contains(Event.current.mousePosition))
        {
            GUI.color = Color.red;
            GUI.DrawTexture(visionButton, texture);
            visionButtonState = 0;
        }
        else
        {
            GUI.color = Color.blue;
            GUI.DrawTexture(visionButton, texture);
            visionButtonState = 0;
        }

        GUI.color = Color.white;
        myStyle.normal.textColor = Color.white;
        GUI.Label(visionButton, "Vision", myStyle);

        if (started == false)
        {
            //vision button
            Rect mapButton = new Rect(screenWidth - buttonWidth * 2.5f  - 3, screenHeight - buttonHeight * 3 - 6, buttonWidth*1.3f, buttonHeight);
            if (Input.GetMouseButtonUp(0) && mapButton.Contains(Event.current.mousePosition))
            {
                mapButtonState++;
            }
            else if (mapButton.Contains(Event.current.mousePosition))
            {
                GUI.color = Color.red;
                GUI.DrawTexture(mapButton, texture);
                mapButtonState = 0;
            }
            else
            {
                GUI.color = Color.blue;
                GUI.DrawTexture(mapButton, texture);
                mapButtonState = 0;
            }

            GUI.color = Color.white;
            myStyle.normal.textColor = Color.white;
            GUI.Label(mapButton, "Map Maker", myStyle);

        }

        //draw net button
        Rect drawNetButton = new Rect(screenWidth - buttonWidth * 2 - 3, screenHeight - buttonHeight, buttonWidth, buttonHeight);
        if (Input.GetMouseButtonUp(0) && drawNetButton.Contains(Event.current.mousePosition))
        {
            drawNetButtonState++;
        }
        else if (drawNetButton.Contains(Event.current.mousePosition))
        {
            GUI.color = Color.red;
            GUI.DrawTexture(drawNetButton, texture);
            drawNetButtonState = 0;
        }
        else
        {
            GUI.color = Color.blue;
            GUI.DrawTexture(drawNetButton, texture);
            drawNetButtonState = 0;
        }

        GUI.color = Color.white;
        myStyle.normal.textColor = Color.white;
        GUI.Label(drawNetButton, "Draw\nNet", myStyle);

        //save button
        Rect saveButton = new Rect(screenWidth - buttonWidth * 3 - 6, screenHeight - buttonHeight, buttonWidth, buttonHeight);
        if (Input.GetMouseButtonUp(0) && saveButton.Contains(Event.current.mousePosition))
        {
            saveButtonState++;
        }
        else if (saveButton.Contains(Event.current.mousePosition))
        {
            GUI.color = Color.red;
            GUI.DrawTexture(saveButton, texture);
            saveButtonState = 0;
        }
        else
        {
            GUI.color = Color.blue;
            GUI.DrawTexture(saveButton, texture);
            saveButtonState = 0;
        }

        GUI.color = Color.white;
        myStyle.normal.textColor = Color.white;
        GUI.Label(saveButton, "Save", myStyle);

        //load button
        Rect loadButton = new Rect(screenWidth - buttonWidth * 3 - 6, screenHeight - buttonHeight*2 -3, buttonWidth, buttonHeight);
        if (Input.GetMouseButtonUp(0) && loadButton.Contains(Event.current.mousePosition))
        {
            loadButtonState++;
        }
        else if (loadButton.Contains(Event.current.mousePosition))
        {
            GUI.color = Color.red;
            GUI.DrawTexture(loadButton, texture);
            loadButtonState = 0;
        }
        else
        {
            GUI.color = Color.blue;
            GUI.DrawTexture(loadButton, texture);
            loadButtonState = 0;
        }

        GUI.color = Color.white;
        myStyle.normal.textColor = Color.white;
        GUI.Label(loadButton, "Load", myStyle);


        //UI numbers
        GUI.color = Color.white;
        myStyle.normal.textColor = Color.white;
        GUI.Label(fastSpeedButton, ">>", myStyle);
        GUI.Label(slowSpeedButton, "<<", myStyle);

        myStyle.fontSize = (int)(screenWidth * 0.03f);

        //draw play speed
        Rect playSpeedNumber = new Rect(fastSpeedButton.x - (int)(screenWidth * 0.04f), fastSpeedButton.y-buttonHeight/2f, buttonWidth,buttonHeight);
        GUI.Label(playSpeedNumber, "Speed: "+playSpeed.ToString("0.0"), myStyle);

        //draw total number of creatures 
        Rect totalNumber = new Rect(fastSpeedButton.x - (int)(screenWidth * 0.04f), fastSpeedButton.y - buttonHeight / 2f - (int)(screenWidth * 0.04f), buttonWidth, buttonHeight);
        
        GUI.Label(totalNumber, "Total: " + totalNumberOfCreatures, myStyle);

        //draw total number of creatures 
        Rect countNumber = new Rect(fastSpeedButton.x - (int)(screenWidth * 0.04f), fastSpeedButton.y - buttonHeight / 2f - (int)(screenWidth * 0.04f*2f), buttonWidth, buttonHeight);
        GUI.Label(countNumber, "Current: " + creatureCount, myStyle);

        //draw total number of creatures 
        Rect speciesCount = new Rect(fastSpeedButton.x - (int)(screenWidth * 0.04f), fastSpeedButton.y - buttonHeight / 2f - (int)(screenWidth * 0.04f * 3f), buttonWidth, buttonHeight);
        GUI.Label(speciesCount, "Species Count: " + speciesListCount, myStyle);

        //draw total number of creatures 
        Rect calculationsNumber = new Rect(fastSpeedButton.x - (int)(screenWidth * 0.04f), fastSpeedButton.y - buttonHeight / 2f - (int)(screenWidth * 0.04f * 4f), buttonWidth, buttonHeight);
        GUI.Label(calculationsNumber, "Calculations: " + brainCalculations, myStyle);

        //World time
        Rect worldNumber = new Rect(fastSpeedButton.x - (int)(screenWidth * 0.04f), fastSpeedButton.y - buttonHeight / 2f - (int)(screenWidth * 0.04f * 5f), buttonWidth, buttonHeight);
        GUI.Label(worldNumber, "World-Time: " + WolrdManager_V2.WORLD_CLOCK.ToString("0.000"), myStyle);

        if (createButtonState > 0 || fastButtonState > 0 || slowButtonState > 0 || visionButtonState > 0 || saveButtonState > 0 || loadButtonState > 0 || drawNetButtonState > 0 || mapButtonState>0)
        {
            isGUISelected = true;
        }
        else
        {
            isGUISelected = false;
        }
    }

    public void SetBrain(/*NEATBrain brain*/ RNN brain, List<TreeData> treeDataList, Creature_V2 creature)
    {
        this.brain = brain;
        this.treeDataList = treeDataList;
        this.creature = creature;
        this.behaviourGenome = brain.GetBehaviourGenome();

        this.neurons = brain.GetNeurons();
        this.connections = brain.GetWeights();
        this.hiddenLayers = brain.GetHiddenLayers();

        float xOff = (int)(screenWidth / (neurons.Length - 1));
        float yOff = (int)(screenHeight * 0.025f);
        float rectWidth = (int)(screenWidth * 0.075f);
        float rectHeight = (int)(rectWidth / 1.5f);


        //IMPORTANT!
        /*network = brain.GetNetworkArray();
        numberOfInputNeurons = brain.GetNumberOfInputNeurons();
        numberOfOutputNeurons = brain.GetNumberOfOutputNeurons();
        numberOfHiddenNeurons = brain.GetNumberOfUsedHiddenNeurons();*/


    }

    public void ResetBrain()
    {
        brain = null;
        
        creature = null;
        behaviourGenome = null;

        //network = null;
        //differenceNetwork = null;


        treeDataList = new List<TreeData>();

        neurons = null;
        connections = null;
        differenceConnection = null;
    }

    public void SetWorldDrawInformation(int total, int count, float playSpeed, int brainCalculations)
    {
        this.totalNumberOfCreatures = total;
        this.creatureCount = count;
        this.playSpeed = playSpeed;
        this.brainCalculations = brainCalculations;
    }

    public bool IsBrainNull()
    {
        return brain == null;
    }

    /*public NEATBrain GetBrain()
    {
        return brain;
    }*/
    
    public RNN GetBrain()
    {
        return brain;
    }

    public void AddDifferenceConnection(RNN differenceBrain)
    {
        differenceConnection = RNN.CreateDifferenceConnectionMatrix(differenceBrain,brain); 
        //RNN.BrainSimilarityScore(brain,differenceBrain, 1);
    }

    public void SetSpeciesList(List<Species> speciesList) {
        this.speciesList = speciesList;
    }
    /*public void AddDifferenceNetwork(NEATBrain differenceBrain)
    {
        this.differenceNetwork = differenceBrain.GetNetworkArray();
        numberOfHiddenNeurons = differenceBrain.GetNumberOfUsedHiddenNeurons();
    }*/
}



