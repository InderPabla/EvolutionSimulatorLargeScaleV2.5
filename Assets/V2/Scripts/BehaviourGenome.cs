
using System;
using System.IO;

public class BehaviourGenome {
    public float[] visionAngles;
    public float[] visionDistances;
    public float bodyHue;
    public float predLevel;
    public float eggBirthTimer; //creature not born yet
    public static Random random = new Random(DateTime.Now.Millisecond);
    public int numberOfEyeSensors;

    private float minVisionLength = 1f;
    public BehaviourGenome(float[] vision, float minVisionLength)
    {
        //this.visionAngles = new float[] { vision[0], vision[1], vision[2], vision[3] };//new float[] { 16.875f, 11.25f, 11.25f, 11.25f };
        this.visionAngles = new float[vision.Length];
        this.visionDistances = new float[vision.Length];
        this.minVisionLength = minVisionLength;

        for (int i = 0; i < visionAngles.Length; i++)
        {
            this.visionAngles[i] = vision[i];
        }

        for (int i = 0; i < visionDistances.Length; i++)
        {
            this.visionDistances[i] = (float)random.NextDouble();
            if (this.visionDistances[i] < minVisionLength)
                this.visionDistances[i] = minVisionLength;
        }

        numberOfEyeSensors = this.visionAngles.Length;

        predLevel = (float)random.NextDouble();
        bodyHue = (float)random.NextDouble();
        eggBirthTimer = (float)random.NextDouble();
        //eggBirthTimer = 0f;

        /*predLevel = 0.5f;
        bodyHue = UnityEngine.Random.Range(0f, 1f);
        eggBirthTimer = 0.25f;*/

        Mutate();
    }

    public BehaviourGenome(float bodyHue, float eggBirthTimer, float predLevel, float[] visionAngles, float[] visionDistances, float minVisionLength)
    {
        this.bodyHue = bodyHue;
        this.eggBirthTimer = eggBirthTimer;
        this.predLevel = predLevel;
        this.visionAngles = visionAngles;
        this.visionDistances = visionDistances;
        this.numberOfEyeSensors = this.visionAngles.Length;
        this.minVisionLength = minVisionLength;
    }

    public BehaviourGenome(BehaviourGenome copy, bool mutate, float minVisionLength)
    {
        this.visionAngles = new float[copy.visionAngles.Length];
        this.minVisionLength = minVisionLength;
        for (int i = 0; i < visionAngles.Length; i++)
        {
            this.visionAngles[i] = copy.visionAngles[i];
        }

        this.visionDistances = new float[copy.visionDistances.Length];

        for (int i = 0; i < visionDistances.Length; i++)
        {
            this.visionDistances[i] = copy.visionDistances[i];
        }

        this.predLevel = copy.predLevel;
        this.bodyHue = copy.bodyHue;
        this.eggBirthTimer = copy.eggBirthTimer;
        this.numberOfEyeSensors = this.visionAngles.Length;
        
        //this.eggBirthTimer = 0f;

        if (mutate == true)
            Mutate();
    }

   

    private void Mutate()
    {
        MutateVision();
        MutateVisionDistances();
        MutateBodyHue();
        MutatePredLevel();
        MutateEggBirthTimer();
    }

    private void MutateVision()
    {
        float randomNumber = (float)random.NextDouble() * 100f;
        for (int i = 0; i < visionAngles.Length; i++)
        {
            randomNumber = (float)random.NextDouble() * 100f;
            if (randomNumber <= 5)
            {
                float factor = ((float)random.NextDouble() + 1f) * 0.1f;
                visionAngles[i] += (visionAngles[i] * factor);
            }
            else if (randomNumber <= 10)
            {
                float factor = ((float)random.NextDouble()) * 0.1f;
                visionAngles[i] -= (visionAngles[i] * factor);
            }

            if (visionAngles[i] < 0f)
            {
                visionAngles[i] = visionAngles[i] + 360f;
            }
            else if (visionAngles[i] > 360f)
            {
                visionAngles[i] = visionAngles[i] - 360f;
            }
        }
        
    }

    private void MutateVisionDistances()
    {
        float randomNumber = (float)random.NextDouble() * 100f;
        for (int i = 0; i < visionDistances.Length; i++)
        {
            randomNumber = (float)random.NextDouble() * 100f;
            if (randomNumber <= 5)
            {
                float factor = ((float)random.NextDouble() + 1f) * 0.1f;
                visionDistances[i] += (visionDistances[i] * factor);
            }
            else if (randomNumber <= 10)
            {
                float factor = ((float)random.NextDouble()) * 0.1f;
                visionDistances[i] -= (visionDistances[i] * factor);
            }

            if (visionDistances[i] < minVisionLength)
            {
                visionDistances[i] = minVisionLength;
            }
            else if (visionDistances[i] > 1f)
            {
                visionDistances[i] = 1f;
            }
        }

    }

    private void MutateBodyHue()
    {
        float randomNumber = (float)random.NextDouble() * 100f;
        if (randomNumber <= 3)
        {
            float factor = ((float)random.NextDouble() + 1f) * 0.3f;
            bodyHue += (bodyHue * factor);
        }
        else if (randomNumber <= 6)
        {
            float factor = ((float)random.NextDouble()) * 0.3f;
            bodyHue -= (bodyHue * factor);
        }

        if (bodyHue < 0f)
        {
            bodyHue = 0f;
        }
        else if (bodyHue > 1f)
        {
            bodyHue = 1f;
        }
    }

    private void MutatePredLevel()
    {
        float randomNumber = (float)random.NextDouble() * 100f;
        if (randomNumber <= 5)
        {
            float factor = ((float)random.NextDouble() + 1f) * 0.1f;
            predLevel += (predLevel * factor);
        }
        else if (randomNumber <= 10)
        {
            float factor = ((float)random.NextDouble()) * 0.1f;
            predLevel -= (predLevel * factor);
        }

        if (predLevel < 0f)
        {
            predLevel = 0f;
        }
        else if (predLevel > 1f)
        {
            predLevel = 1f;
        }
    }

    private void MutateEggBirthTimer()
    {
        float randomNumber = (float)random.NextDouble() * 100f;
        if (randomNumber <= 5)
        {
            float factor = ((float)random.NextDouble() + 1f) * 0.1f;
            eggBirthTimer += (eggBirthTimer * factor);
        }
        else if (randomNumber <= 10)
        {
            float factor = ((float)random.NextDouble()) * 0.1f;
            eggBirthTimer -= (eggBirthTimer * factor);
        }

        if (eggBirthTimer < 0f)
        {
            eggBirthTimer = 0f;
        }
        else if (eggBirthTimer > 1f)
        {
            eggBirthTimer = 1f;
        }
    }

    public void StreamWriteBehaviourGenome(StreamWriter writer)
    {
        writer.Write(bodyHue + " " + eggBirthTimer + " " + predLevel + " "+ visionAngles.Length+" ");
        for (int i = 0; i < visionAngles.Length; i++)
        {
            writer.Write(visionAngles[i]+" ");
        }

        for (int i = 0; i < visionDistances.Length; i++)
        {
            writer.Write(visionDistances[i] + " ");
        }
    }

}
