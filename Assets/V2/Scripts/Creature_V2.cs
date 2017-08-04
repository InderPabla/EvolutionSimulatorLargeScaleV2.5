using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;

public class Creature_V2 : CustomCircleCollider, IEquatable<Creature_V2>, IComparable
{
    public Transform trans = null; //Transform of this object
    private MeshRenderer creatureMeshRen = null;
    private TextMesh textMesh = null;
    private MeshRenderer textMeshRen = null;

    private Transform fightTrans = null; //the circle around the creature that truns red when in fight mode
    private LineRenderer leftLine = null;
    private LineRenderer rightLine = null;
    private LineRenderer spikeLine = null;
    private LineRenderer[] sensorLine;

    //private NEATBrain brain = null;
    private RNN brain = null;

    private HSBColor bodyColor;
    private HSBColor mouthColor;

    private Vector3 leftPos;
    private Vector3 rightPos;
    private Vector3[] sensorPos;
    private float[] sensorValue;
    private float spikeLength = 0f;
    private Vector3 spikePos;

    private int[] tileDetail = new int[2];

    private float initialRadius;
    //private float currentRadius = 0.04f;

    private int ID = -1;
    private int generation;
    //private float worldDeltaTime = 0.001f;
    private float timeLived = 0f;

    private TileMap_V2 map;
    private Material bodyMaterial;
    private Material mouthMaterial;

    private float sensorSize;
    private Energy energy;
    private WolrdManager_V2 world;

    private List<Creature_V2> children;
    private int childCount = 0;
    private bool isNode = false;
    private string parentNames = "";
    private string creatureName = "";

    //private HashSet<int> populationID;
    private Species species;
    private BehaviourGenome behaviourGenome;

    Creature_V2 spikeCreature = null;
    List<Creature_V2> creaturesInBirthRange = null;
    HSBColor bodyTileColor;

    private float speciesSimilarityScore;
    private int threadID;
    private Color[] threadColors;

    private float[] output;
    private int numberOfEyeSensors;
    private bool fastCompute;
    private int numberOfInputNeurons;

    private bool renderObjectsMade = false;

    public Creature_V2(
                        int ID, int generation,

                        RNN brain /*NEATBrain brain*/, HSBColor bodyColor, Vector3 bodyPos, Vector3 leftPos, Vector3 rightPos,
                        float sensorSize, float angle, float worldDeltaTime, float initialRadius,
                        float initialEnergy, float currentEnergy, float life, float minLife, float lifeDecerase,
                        float eatDamage, float veloDamage, float angDamage, float fightDamage, float veloForward, float veloAngular,
                        TileMap_V2 map, WolrdManager_V2 world, String parentNames, float speciesSimilarityScore, Color[] threadColors,
                        float minBirthEnergy, float minFightMaturity, float birthEnergyCost, float birthLifeCost, float fightEnergyCost, float fightLifeCost, float maturity,
                        float eggLife, float currentEggLife, float eggFactor, float eggPrematureBirthDamage, float waterDamage, bool fastCompute
                        ) : base(initialRadius, bodyPos, angle, veloForward, veloAngular, 1f, worldDeltaTime)
    {

        this.ID = ID;
        this.generation = generation;

        /*this.trans = trans;
        
        this.leftLine = leftLine;
        this.rightLine = rightLine;
        this.sensorLine = sensorLine;
        this.spikeLine = spikeLine;*/

        this.brain = brain;
        this.bodyColor = bodyColor;
        this.leftPos = leftPos;
        this.rightPos = rightPos;
        this.sensorSize = sensorSize;
        base.worldDeltaTime = worldDeltaTime;
        this.initialRadius = initialRadius;
        this.map = map;
        this.world = world;
        this.parentNames = parentNames;


        this.speciesSimilarityScore = speciesSimilarityScore;

        this.spikeLength = 0f;
        this.spikePos = Vector3.zero;

        this.children = new List<Creature_V2>();
        //energyDensity = 1f/(Mathf.PI * initialRadius * initialRadius);



        //this.bodyColor = new HSBColor();//new HSBColor(UnityEngine.Random.Range(0f,1f),1f,1f);
        this.mouthColor = new HSBColor(1, 1, 1);//new HSBColor(UnityEngine.Random.Range(0f, 1f), 1f, 1f);

        this.creatureName = brain.GetName();
        this.behaviourGenome = brain.GetBehaviourGenome();

        numberOfEyeSensors = behaviourGenome.numberOfEyeSensors;
        sensorPos = new Vector3[numberOfEyeSensors];
        sensorValue = new float[numberOfEyeSensors];
        for (int i = 0; i < numberOfEyeSensors; i++)
        {
            sensorPos[i] = Vector3.zero;
        }

        this.timeLived = maturity;
        this.energy = new Energy(initialEnergy, currentEnergy, life, map, worldDeltaTime,
            minLife, lifeDecerase, eatDamage, veloDamage, angDamage, fightDamage,
            behaviourGenome.eggBirthTimer, behaviourGenome.predLevel, this.brain,
            minBirthEnergy, minFightMaturity, birthEnergyCost, birthLifeCost,
            fightEnergyCost, fightLifeCost, maturity, eggLife, currentEggLife, eggFactor,
            eggPrematureBirthDamage, waterDamage);

        if (this.energy.GetEnergy() < 1f)
            base.radius = initialRadius;
        else
            base.radius = initialRadius + ((this.energy.GetEnergy()) * 0.001f);

        /*this.energy.SetEnergy(currentEnergy);
        this.energy.SetLife(life);*/

        this.threadColors = threadColors;
        this.creaturesInBirthRange = new List<Creature_V2>();
        this.fastCompute = fastCompute;
        this.numberOfInputNeurons = brain.GetNumberOfInputNeurons();
        output = brain.GetOutput();

    }

    public bool Equals(Creature_V2 other)
    {
        if (other == null)
            return false;

        return (other.ID == this.ID);
    }

    public void UpdateCreature(int threadID)
    {
        this.threadID = threadID;
        if (energy.EggHatched() == false)
        {
            energy.UpdateEggTimer();
        }
        else
        {
            UpdatePhysicsBeforeNetwork();
        }
    }


    public void UpdatePhysicsBeforeNetwork()
    {
        float closestCreatureDistance = float.MaxValue;
        float closestCreatureHalfRadian = 0;
        float closestCreatureSpecies = 0;

        bodyTileColor = map.GetColor((int)base.position.x, (int)base.position.y);
        HSBColor leftTileColor = map.GetColor((int)leftPos.x, (int)leftPos.y);
        HSBColor rightTileColor = map.GetColor((int)rightPos.x, (int)rightPos.y);

        bool spikeTargetFound = false;
        float spikeValue = -1;
        spikeCreature = null;
        List<Creature_V2> creatureListAtTile = map.ExistCreaturesNearTile((int)base.position.x, (int)base.position.y);
        float[] sensorDistance = new float[numberOfEyeSensors];
        float[] sameSpecies = new float[numberOfEyeSensors];
        float[] danger = new float[numberOfEyeSensors];
        float[] egg = new float[numberOfEyeSensors];
        for (int i = 0; i < numberOfEyeSensors; i++)
        {
            sensorValue[i] = -1f;
            sensorDistance[i] = -1f;

            if (creatureListAtTile != null)
            {
                for (int j = 0; j < creatureListAtTile.Count; j++)
                {
                    Creature_V2 creature = creatureListAtTile[j];
                    if (!creature.Equals(this))
                    {

                        float distanceFromThisCreature = Vector2.Distance(creature.position, position);
                        if (distanceFromThisCreature < closestCreatureDistance)
                        {
                            Vector2 deltaVector = (creature.position - position).normalized;
                            closestCreatureDistance = distanceFromThisCreature;
                            //closestCreatureColor = creature.bodyColor.h;
                            if (species.Equals(creature.species))
                            {
                                closestCreatureSpecies = 1f;
                            }
                            else
                            {
                                closestCreatureSpecies = -1f;
                            }


                            float rad = Mathf.Atan2(deltaVector.y, deltaVector.x);
                            rad *= Mathf.Rad2Deg;

                            rad = rad % 360;
                            if (rad < 0)
                            {
                                rad = 360 - rad;
                            }

                            rad = 90f - rad;
                            if (rad < 0f)
                            {
                                rad += 360f;
                            }
                            rad = 360 - rad;
                            rad -= base.rotation;
                            if (rad < 0)
                                rad = 360 + rad;
                            if (rad >= 180f)
                            {
                                rad = 360 - rad;
                                rad *= -1f;
                            }
                            rad *= Mathf.Deg2Rad;

                            closestCreatureHalfRadian = rad / (Mathf.PI);
                        }

                        float distance;
                        if (spikeTargetFound == false)
                        {
                            distance = creature.IsLineIntersectingWithCircle(base.position, spikePos);
                            if (distance != -1f)
                            {
                                spikeCreature = creature;
                                //spikeValue = spikeCreature.bodyColor.h;

                                if (/*species.GetPopulationHashID().Contains(creature.ID) == true*/ species.Equals(creature.species))
                                {
                                    spikeValue = 1f;
                                    if (creature.sameSpeciesDanger <= sameSpeciesDanger)
                                        sameSpeciesDanger += base.worldDeltaTime;
                                }
                                else
                                {
                                    spikeValue = -1f;

                                }

                                spikeTargetFound = true;
                            }
                        }

                        distance = creature.IsLineIntersectingWithCircle(base.position, sensorPos[i]);

                        if (distance != -1f)
                        {
                            sensorValue[i] = creature.bodyColor.h;
                            sensorDistance[i] = distance / (sensorSize * 2.5f)/*behaviourGenome.visionDistances[i]*/;
                            egg[i] = creature.energy.EggHatched() == true ? 1 : -1;
                            //if (isNode)
                            //Debug.Log(species.Equals(creature.species) + " "+creature.ID);

                            if (/*species.GetPopulationHashID().Contains(creature.ID) == true*/ species.Equals(creature.species))
                            {
                                sameSpecies[i] = 1f;
                                danger[i] = creature.sameSpeciesDanger;
                            }
                            else
                            {
                                sameSpecies[i] = -1f;
                            }

                            break;
                        }
                    }
                }
            }
        }

        if (isNode)
        {
            //Debug.Log(egg[0] + " " + egg[1] + " " + egg[2] + " " + egg[3]);
            //Debug.Log(leftTileColor + "  %  " + rightTileColor + "  %  " +bodyTileColor);
        }

        List<Creature_V2> creatureListAtBodyTile = map.ExistCreaturesNearPrecisionTile(base.position.x, base.position.y, base.radius);
        if (creaturesInBirthRange.Count != 0)
            creaturesInBirthRange = new List<Creature_V2>();
        float hueAverage = -1f;
        float isCollision = -1f;

        //check for left sensor collision
        if (creatureListAtBodyTile != null)
        {
            for (int i = 0; i < creatureListAtBodyTile.Count; i++)
            {
                Creature_V2 creature = creatureListAtBodyTile[i];
                if (!creature.Equals(this))
                {

                    float distanceFromThisCreature = Vector2.Distance(creature.position, position);
                    if (distanceFromThisCreature < closestCreatureDistance)
                    {
                        Vector2 deltaVector = (creature.position - position).normalized;
                        closestCreatureDistance = distanceFromThisCreature;
                        //closestCreatureColor = creature.bodyColor.h;
                        if (species.Equals(creature.species))
                        {
                            closestCreatureSpecies = 1f;
                        }
                        else
                        {
                            closestCreatureSpecies = -1f;
                        }

                        float rad = Mathf.Atan2(deltaVector.y, deltaVector.x);
                        rad *= Mathf.Rad2Deg;

                        rad = rad % 360;
                        if (rad < 0)
                        {
                            rad = 360 - rad;
                        }

                        rad = 90f - rad;
                        if (rad < 0f)
                        {
                            rad += 360f;
                        }
                        rad = 360 - rad;
                        rad -= base.rotation;
                        if (rad < 0)
                            rad = 360 + rad;
                        if (rad >= 180f)
                        {
                            rad = 360 - rad;
                            rad *= -1f;
                        }
                        rad *= Mathf.Deg2Rad;

                        closestCreatureHalfRadian = rad / (Mathf.PI);
                    }


                    if (creature.CollisionCheckWithCircle(radius, base.position) == true)
                    {
                        hueAverage += creature.bodyColor.h;
                        isCollision++;
                        creaturesInBirthRange.Add(creature);
                    }
                    else if (creature.CollisionCheckWithCircle(radius * 2f, base.position))
                    {
                        creaturesInBirthRange.Add(creature);
                    }
                }
            }
        }

        if (isCollision >= 0)
        {
            isCollision++;
            hueAverage = hueAverage / isCollision;
            isCollision = 1f;
        }

        /*brain.SetInputs(new float[] {sensorDistance[0],sensorDistance[1], sensorDistance[2], sensorDistance[3], 
            sensorValue[0]*sameSpecies[0], sensorValue[1]*sameSpecies[1], sensorValue[2]*sameSpecies[2], sensorValue[3]*sameSpecies[3],  spikeValue,
            hueAverage, bodyTileColor.h, bodyTileColor.s, leftTileColor.h, leftTileColor.s, rightTileColor.h, rightTileColor.s,
            energy.GetLife()/2f, closestCreatureSpecies,closestCreatureHalfRadian, previousOutput[7], previousOutput[8],mouthColor.h,  bodyColor.h});*/

        /*brain.FeedForward(new float[] {sensorDistance[0],sensorDistance[1], sensorDistance[2], sensorDistance[3],
            sensorValue[0], sensorValue[1], sensorValue[2], sensorValue[3],  spikeValue,
            hueAverage, bodyTileColor.h, bodyTileColor.s, leftTileColor.h, leftTileColor.s, rightTileColor.h, rightTileColor.s,
            energy.GetLife()/2f, closestCreatureSpecies,closestCreatureHalfRadian, sameSpecies[0], sameSpecies[1], sameSpecies[2], sameSpecies[3],mouthColor.h,  bodyColor.h
            ,egg[0],egg[1],egg[2],egg[3]
       });*/

        /*brain.FeedForward(new float[] {sensorDistance[0],sensorDistance[1], sensorDistance[2], sensorDistance[3],
            sensorValue[0], sensorValue[1], sensorValue[2], sensorValue[3],  spikeValue,
            hueAverage, bodyTileColor.h, bodyTileColor.s, leftTileColor.h, leftTileColor.s,  rightTileColor.h, rightTileColor.s, 
            energy.GetLife()/2f, closestCreatureHalfRadian, sameSpecies[0], sameSpecies[1], sameSpecies[2], sameSpecies[3], egg[0],egg[1],egg[2],egg[3]
       });*/

        float[] inputs = new float[numberOfInputNeurons];

        Buffer.BlockCopy(sensorDistance, 0, inputs, 0, 4 * numberOfEyeSensors);
        Buffer.BlockCopy(sensorValue, 0, inputs, 4 * numberOfEyeSensors, 4 * numberOfEyeSensors);
        Buffer.BlockCopy(sameSpecies, 0, inputs, 4 * numberOfEyeSensors * 2, 4 * numberOfEyeSensors);
        Buffer.BlockCopy(egg, 0, inputs, 4 * numberOfEyeSensors * 3, 4 * numberOfEyeSensors);
        int endBlockIndex = numberOfEyeSensors * 4;
        inputs[endBlockIndex] = spikeValue;
        inputs[endBlockIndex + 1] = hueAverage;
        inputs[endBlockIndex + 2] = bodyTileColor.h;
        inputs[endBlockIndex + 3] = bodyTileColor.s;
        inputs[endBlockIndex + 4] = leftTileColor.h;
        inputs[endBlockIndex + 5] = leftTileColor.s;
        inputs[endBlockIndex + 6] = rightTileColor.h;
        inputs[endBlockIndex + 7] = rightTileColor.s;
        inputs[endBlockIndex + 8] = energy.GetLife() / 2f;
        inputs[endBlockIndex + 9] = closestCreatureHalfRadian;

        if (fastCompute == true)
        {

            brain.FeedForward_Fast_Tanh(inputs);

            /*brain.FeedForward_Fast_Tanh(new float[] {sensorDistance[0],sensorDistance[1], sensorDistance[2], sensorDistance[3],
            sensorValue[0], sensorValue[1], sensorValue[2], sensorValue[3],  spikeValue,
            hueAverage, bodyTileColor.h, bodyTileColor.s, leftTileColor.h, leftTileColor.s,  rightTileColor.h, rightTileColor.s,
            energy.GetLife()/2f, closestCreatureHalfRadian, sameSpecies[0], sameSpecies[1], sameSpecies[2], sameSpecies[3], egg[0],egg[1],egg[2],egg[3]
             });*/
        }
        else
        {


            brain.FeedForward_Slow_Tanh(inputs);


            /*brain.FeedForward_Slow_Tanh(new float[] {sensorDistance[0],sensorDistance[1], sensorDistance[2], sensorDistance[3],
            sensorValue[0], sensorValue[1], sensorValue[2], sensorValue[3],  spikeValue,
            hueAverage, bodyTileColor.h, bodyTileColor.s, leftTileColor.h, leftTileColor.s,  rightTileColor.h, rightTileColor.s,
            energy.GetLife()/2f, closestCreatureHalfRadian, sameSpecies[0], sameSpecies[1], sameSpecies[2], sameSpecies[3], egg[0],egg[1],egg[2],egg[3]
             });*/

        }

        float accelForward = output[0] * -1f;
        float accelAngular = output[1];
        float bodyHue = output[2];
        float mouthHue = output[3];

        if (output[6] >= 0)
            spikeLength = output[6];
        else
            spikeLength = 0;

        if (bodyHue >= 0)
            this.bodyColor.h = bodyHue;

        if (mouthHue >= 0)
            this.mouthColor.h = mouthHue;

        base.UpdateColliderPhysics(/*Mathf.Abs(accelForward)>0.5f? accelForward*4f: accelForward*2f*/accelForward, accelAngular); //CHANGED TO HAVE 4x speed

        base.rotation = base.rotation % 360;
        if (base.rotation < 0)
        {
            base.rotation = 360 + base.rotation;
        }

        UpdateSensors();

        if (energy.GetEnergy() < 1f)
            //base.radius = initialRadius - ((1f - energy.GetEnergy()) * (initialRadius / 2));
            base.radius = initialRadius;
        else
            //base.radius = initialRadius +((energy.GetEnergy()/4f) * 0.0025f);
            base.radius = initialRadius + ((energy.GetEnergy()) * 0.005f);

    }

    private float sameSpeciesDanger = 0;
    private float differentSpeciesDanger = 0;



    public void UpdateAfterNetwork()
    {
        map.RemoveCreatureFromTileList(tileDetail[0], tileDetail[1], this);

        //copy tile detail
        tileDetail[0] = (int)base.position.x;
        tileDetail[1] = (int)base.position.y;

        if (base.position.x < 0)
            tileDetail[0] = -1;
        if (base.position.y < 0)
            tileDetail[1] = -1;

        map.AddCreatureToTileList(tileDetail[0], tileDetail[1], this);

        if (energy.EggHatched())
        {
            //float[] output = brain.GetOutput();



            energy.UpdateCreatureEnergy(tileDetail[0], tileDetail[1], output, bodyTileColor.h, mouthColor.h, spikeCreature);
        }

        // Creature is dead ;( D: :( -_-  ;_;
        if (energy.IsAlive() == false)
        {
            /*if (energy.GetDeathByWater() == true)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    Creature_V2 child = children[i];
                    child.KillWithEnergy();
                }
            }
            else
            {
                for (int i = 0; i < children.Count; i++)
                {
                    Creature_V2 child = children[i];
                    if (child.IsAlive())
                    {
                        if (child.childCount == 0)
                        {
                            child.RemoveEnergy(1f);
                            child.RemoveLife(0.1f);
                        }
                    }
                }
            }*/


            Kill();
        }
        else if (energy.GiveBirth() == true)
        {
            energy.DoneBirth();
            world.CreateCreature(this);
        }
        else if (energy.IsAbleToGiveBirth() == true)
        {
            for (int i = 0; i < creaturesInBirthRange.Count; i++)
            {
                Creature_V2 creature = creaturesInBirthRange[i];
                if (creature.species.GetID() == species.GetID())
                {
                    if (creature.energy.IsAbleToGiveBirth())
                    {
                        energy.AffectEnergy(-0.75f);
                        creature.energy.AffectEnergy(-0.75f);
                        energy.DoneBirth();
                        creature.energy.DoneBirth();

                        world.CreateCreature(this, creature);
                        break;
                    }
                    else
                    {
                        energy.AffectEnergy(-1.25f);
                        creature.energy.AffectEnergy(-0.25f);

                        energy.DoneBirth();
                        creature.energy.DoneBirth();

                        world.CreateCreature(this, creature);
                        break;
                    }
                }
            }
        }

        timeLived += base.worldDeltaTime;
    }


    private void UpdateSensors()
    {
        float fixedRotation = base.rotation + 90f; //base rotation  
        float leftAngle = (((fixedRotation) + 25f) * Mathf.Deg2Rad);
        float rightAngle = (((fixedRotation) - 25f) * Mathf.Deg2Rad);

        //left and right position calculation
        leftPos = base.position + new Vector3(sensorSize * Mathf.Cos(leftAngle), sensorSize * Mathf.Sin(leftAngle), 0f);
        rightPos = base.position + new Vector3(sensorSize * Mathf.Cos(rightAngle), sensorSize * Mathf.Sin(rightAngle), 0f);

        /*float startRotation = fixedRotation - 16.875f;
        float addRotation = 11.25f;
        for (int i = 0; i < sensorPos.Length; i++)
        {
            sensorPos[i] = base.position + new Vector3(sensorSize * Mathf.Cos(startRotation* Mathf.Deg2Rad) *2.5f, sensorSize * Mathf.Sin(startRotation* Mathf.Deg2Rad) * 2.5f, 0f);
            startRotation += addRotation;
        }*/

        float startRotation = fixedRotation - behaviourGenome.visionAngles[0];
        for (int i = 0; i < sensorPos.Length; i++)
        {
            if (i > 0)
                startRotation += behaviourGenome.visionAngles[i];

            float sensorFactor = behaviourGenome.visionDistances[i]* 2.5f* sensorSize;
            sensorPos[i] = base.position + new Vector3(sensorFactor * Mathf.Cos(startRotation * Mathf.Deg2Rad), sensorFactor * Mathf.Sin(startRotation * Mathf.Deg2Rad), 0f);

        }

        spikePos = base.position + new Vector3(sensorSize * Mathf.Cos(fixedRotation * Mathf.Deg2Rad) * spikeLength * 2.5f, sensorSize * Mathf.Sin(fixedRotation * Mathf.Deg2Rad) * spikeLength * 2.5f, 0f);

    }

    public void SetRenderComponents(bool show, Transform trans, LineRenderer leftLine, LineRenderer rightLine, LineRenderer[] sensorLine, LineRenderer spikeLine)
    {
        this.trans = trans;
        this.leftLine = leftLine;
        this.rightLine = rightLine;
        this.sensorLine = sensorLine;
        this.spikeLine = spikeLine;

        this.textMeshRen = trans.GetChild(1).GetComponent<MeshRenderer>();
        this.creatureMeshRen = trans.GetComponent<MeshRenderer>();

        this.textMesh = trans.GetChild(1).GetComponent<TextMesh>();
        this.fightTrans = trans.GetChild(3);
        this.bodyMaterial = trans.GetComponent<Renderer>().material;
        this.mouthMaterial = trans.GetChild(0).GetComponent<Renderer>().material;

        renderObjectsMade = true;

        if (show == true)
            Show();
        else
            Hide();
    }

    public void UpdateRender(bool visionVisual, Vector3 bottomLeft, Vector3 topLeft, bool threadDraw, bool speciesDraw)
    {
        //try {
        if ((position.x > bottomLeft.x) &&
            (position.x < topLeft.x) &&
            (position.y > bottomLeft.y) &&
            (position.y < topLeft.y) && renderObjectsMade == true)
        {



            if (energy.EggHatched() == true)
            {
                //float[] output = brain.GetOutput();

                leftLine.SetPosition(0, position);
                leftLine.SetPosition(1, leftPos);
                rightLine.SetPosition(0, position);
                rightLine.SetPosition(1, rightPos);

                if (output[4] > 0)
                {
                    leftLine.SetColors(Color.black, Color.black);
                    rightLine.SetColors(Color.black, Color.black);
                }
                else
                {
                    leftLine.SetColors(Color.white, Color.white);
                    rightLine.SetColors(Color.white, Color.white);
                }


                if (visionVisual)
                {
                    for (int i = 0; i < sensorLine.Length; i++)
                    {
                        sensorLine[i].SetPosition(0, position);
                        sensorLine[i].SetPosition(1, sensorPos[i]);
                    }

                    for (int i = 0; i < sensorValue.Length; i++)
                    {
                        if (sensorValue[i] >= 0)
                        {
                            sensorLine[i].SetColors(Color.blue, Color.blue);
                        }
                        else
                        {
                            sensorLine[i].SetColors(Color.white, Color.white);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < sensorLine.Length; i++)
                    {
                        sensorLine[i].SetPosition(0, Vector2.zero);
                        sensorLine[i].SetPosition(1, Vector2.zero);
                    }
                }


                /*if (output[6] > 0)
                {
                    //outlineTrans.localScale = new Vector3(2f, 2f, 1f);
                    fightTrans.localScale = new Vector3(1.6f, 1.6f, 1f);
                    fightTrans.GetComponent<Renderer>().material.color = new Color(1f,0f,0f,0.5f);
                }
                else
                {
                    fightTrans.localScale = new Vector3(1.075f, 1.075f, 1f);
                    fightTrans.GetComponent<Renderer>().material.color = Color.black;
                }*/

                spikeLine.SetPosition(0, position);
                spikeLine.SetPosition(1, spikePos);
                spikeLine.SetColors(Color.red, Color.red);

                if (threadDraw == true)
                {
                    bodyMaterial.color = threadColors[threadID];
                    mouthMaterial.color = threadColors[threadID];
                }
                else if (speciesDraw == true)
                {
                    bodyMaterial.color = species.GetColor();
                    mouthMaterial.color = species.GetColor();
                }
                else
                {
                    bodyMaterial.color = bodyColor.ToColor(); /*new HSBColor(behaviourGenome.bodyHue, 1f, 1f).ToColor()*/
                    mouthMaterial.color = mouthColor.ToColor();

                }


                trans.position = position;

                trans.eulerAngles = new Vector3(0f, 0f, rotation);

                textMesh.transform.eulerAngles = new Vector3(0, 0, 0f);
                textMesh.transform.position = position + new Vector3(0f, 0.4f, 0f);

                if (isNode == true)
                {
                    textMesh.transform.localScale = new Vector3(2f, 2f, 2f);
                    textMesh.color = Color.red;
                }
                else
                {
                    if (radius > 0)
                    {
                        trans.localScale = new Vector3(radius * 2f, radius * 2f, 1f);
                        textMesh.transform.localScale = new Vector3(1f, 1f, 1f);
                        textMesh.color = Color.white;
                    }
                }
            }
            else
            {

                leftLine.SetPosition(0, Vector2.zero);
                leftLine.SetPosition(1, Vector2.zero);
                rightLine.SetPosition(0, Vector2.zero);
                rightLine.SetPosition(1, Vector2.zero);


                for (int i = 0; i < sensorLine.Length; i++)
                {
                    sensorLine[i].SetPosition(0, Vector2.zero);
                    sensorLine[i].SetPosition(1, Vector2.zero);
                }

                spikeLine.SetPosition(0, Vector2.zero);
                spikeLine.SetPosition(1, Vector2.zero);

                if (threadDraw == true)
                {
                    bodyMaterial.color = threadColors[threadID];
                    mouthMaterial.color = threadColors[threadID];
                }
                else if (speciesDraw == true)
                {
                    bodyMaterial.color = species.GetColor();
                    mouthMaterial.color = species.GetColor();
                }
                else
                {
                    bodyMaterial.color = bodyColor.ToColor(); /*new HSBColor(behaviourGenome.bodyHue, 1f, 1f).ToColor()*/
                    mouthMaterial.color = mouthColor.ToColor();

                }

                trans.position = position;

                trans.eulerAngles = new Vector3(0f, 0f, rotation);

                textMesh.transform.eulerAngles = new Vector3(0, 0, 0f);
                textMesh.transform.position = position + new Vector3(0f, 0.4f, 0f);

            }
        }
        /*}
        catch (Exception e)
        {
            Debug.Log(e);

        }*/
    }


    public void Hide()
    {
        if (renderObjectsMade == true)
        {
            textMeshRen.enabled = false;
            creatureMeshRen.enabled = false;
            trans.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
            trans.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().enabled = false;
            trans.GetChild(2).GetComponent<MeshRenderer>().enabled = false;
            trans.GetChild(3).GetComponent<MeshRenderer>().enabled = false;

            for (int i = 4; i < trans.childCount; i++)
                trans.GetChild(i).GetComponent<LineRenderer>().enabled = false;
        }
    }

    public void Show()
    {
        if (renderObjectsMade == true)
        {
            textMeshRen.enabled = true;
            creatureMeshRen.enabled = true;

            trans.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
            trans.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().enabled = true;
            trans.GetChild(2).GetComponent<MeshRenderer>().enabled = true;
            trans.GetChild(3).GetComponent<MeshRenderer>().enabled = true;

            for (int i = 4; i < trans.childCount; i++)
                trans.GetChild(i).GetComponent<LineRenderer>().enabled = true;
        }
    }


    public RNN GetBrain()
    {
        return brain;
    }

    public float GetLife()
    {
        return energy.GetLife();
    }

    public void KillWithEnergy()
    {
        energy.SetKillEnergy();
    }

    public void AffectEnergy(float affect)
    {
        energy.AffectEnergy(affect);
    }

    public void AffectLife(float life)
    {
        energy.AffectLife(life);
    }

    public float GetEnergy()
    {
        return energy.GetEnergy();
    }

    public float GetDeltaEnergy()
    {
        return energy.GetDeltaEnergy();
    }

    public float GetTimeLived()
    {
        return timeLived; //return time lived of this creature
    }

    public bool IsAlive()
    {
        return energy.IsAlive();
    }

    public String GetParentNames()
    {
        return parentNames;
    }

    public void Kill()
    {
        children.Clear();
        //brain = null; 
        map.RemoveCreatureFromTileList(tileDetail[0], tileDetail[1], this);
        map = null;

        //world.RemoveCreature(this);

        //GameObject.Destroy(trans.gameObject);
        if (renderObjectsMade == true)
            UnityEngine.Object.Destroy(trans.gameObject);
    }

    public int GetGeneration()
    {
        return generation;
    }

    public void AddChildren(Creature_V2 child)
    {
        children.Add(child);
        childCount++;
    }

    public int CompareTo(object obj)
    {
        if (obj == null) return 1;

        Creature_V2 otherCreature = obj as Creature_V2;
        if (otherCreature != null)
            return this.ID.CompareTo(otherCreature.ID);
        else
            throw new ArgumentException("Object is not a Temperature");
    }

    public int GetID()
    {
        return ID;
    }

    public List<Creature_V2> GetChildren()
    {
        return children;
    }

    public int GetChildCount()
    {
        return childCount;
    }

    public string GetName()
    {
        return creatureName;
    }

    public void SetIsNode(bool isNode)
    {
        this.isNode = isNode;
    }

    public bool IsNode()
    {
        return this.isNode;
    }

    public bool SameSpecies(Creature_V2 other)
    {
        /*if (RNN.BrainSimilarityScore(brain, other.GetBrain()) < speciesSimilarityScore)  //0.0075
         {
             return true;
         }*/

        if (RNN.BrainSimilarityScore(brain, other.GetBrain()) >= speciesSimilarityScore)   //<= 0.0075
        {
            return true;
        }

        return false;
    }

    /*public bool SameSpecies(RNN other)
    {
        if (RNN.BrainSimilarityScore(brain, other) < speciesSimilarityScore)  //0.0075
        {
            return true;
        }
        return false;
    }*/

    public void SetSpecies(Species species)
    {
        this.species = species;
    }

    public Species GetSpecies()
    {
        return species;
    }

    public void StreamWriteCreatureData(StreamWriter writer)
    {
        writer.Write(
                 ID + " "
               + timeLived + " "
               + generation + " "
               + creatureName + " "
               + parentNames + " "
               + energy.GetEnergy() + " "
               + energy.GetLife() + " "
               + position.x + " "
               + position.y + " "
               + rotation + " "
               + veloForward + " "
               + veloAngular + " "
               );

        behaviourGenome.StreamWriteBehaviourGenome(writer);

        brain.StreamWriterRNNBrain(writer);
        // brain.StreamWriterNEATBrain(writer);  << important!
    }

    public void delta_time(float value)
    {
        base.worldDeltaTime = value;
        energy.world_delta_time(value);
    }

    public void creature_size(float value)
    {
        initialRadius = value / 2f;
    }

    public void min_life(float value)
    {
        energy.min_life(value);
    }

    public void life_decrease(float value)
    {
        energy.life_decrease(value);
    }

    public void eat_damage(float value)
    {
        energy.eat_damage(value);
    }

    public void velo_damage(float value)
    {
        energy.velo_damage(value);
    }

    public void ang_damage(float value)
    {
        energy.ang_damage(value);
    }

    public void fight_damage(float value)
    {
        energy.fight_damage(value);
    }

    public void min_energy_to_birth(float value)
    {
        energy.min_energy_to_birth(value);
    }

    public void min_fight_maturity(float value)
    {
        energy.min_fight_maturity(value);
    }

    public void birth_energy_cost(float value)
    {
        energy.birth_energy_cost(value);
    }

    public void birth_life_cost(float value)
    {
        energy.birth_life_cost(value);
    }

    public void fight_energy_cost(float value)
    {
        energy.fight_energy_cost(value);
    }

    public void fight_life_cost(float value)
    {
        energy.fight_life_cost(value);
    }

    public void water_damage(float value)
    {
        energy.water_damage(value);
    }

    public void mutation_number(float value)
    {
        brain.mutation_number(value);
    }

    public void mutation_weaker_parent_factor(float value)
    {
        brain.mutation_weaker_parent_factor(value);
    }

    public void mutation_sign(float value)
    {
        brain.mutation_sign(value);
    }

    public void mutation_random(float value)
    {
        brain.mutation_random(value);
    }

    public void mutation_increase(float value)
    {
        brain.mutation_increase(value);
    }

    public void mutation_decrease(float value)
    {
        brain.mutation_decrease(value);
    }

    public void species_similarity_score(float value)
    {
        speciesSimilarityScore = value;
        brain.species_similarity_score(value);
    }
}