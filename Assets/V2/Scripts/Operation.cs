
using UnityEngine;

public class Operation {

    public enum Op { NEW, SEXUAL, ASEXUAL, LOAD };
    public Op type;

    public Creature_V2 parent1;
    public Creature_V2 parent2;

    public int ID;
    public float timeLived;
    public int generation;
    public string creatureName;
    public string parentNames;
    public Vector2 position;
    public float[][][] weights;
    public BehaviourGenome genome;
    public float energy;
    public float life;
    public float rotation;
    public float veloForward;
    public float veloAngular;

    public Operation(Op type) {
        this.type = type;
    }

    public Operation(Op type, Creature_V2 parent)
    {
        this.type = type;

        this.parent1 = parent;
    }

    public Operation(Op type,Creature_V2 parent1, Creature_V2 parent2)
    {
        this.type = type;

        this.parent1 = parent1;
        this.parent2 = parent2;
    }

    public Operation(Op type, int ID, float timeLived, int generation, string creatureName, string parentNames, Vector2 position, float[][][] weights, BehaviourGenome genome, float energy, float life, float rotation, float veloForward, float veloAngular)
    {
        this.type = type;

        this.ID = ID;
        this.timeLived = timeLived;
        this.generation = generation;
        this.creatureName = creatureName;
        this.parentNames = parentNames;
        this.position = position;
        this.weights = weights;
        this.genome = genome;
        this.energy = energy;
        this.life = life;
        this.rotation = rotation;
        this.veloAngular = veloAngular;
        this.veloForward = veloForward;
    }

}
