using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Energy
{

    private bool giveBirth = false;
    private int birthFrameCounter = 0;
    private bool isAlive = true;
    private bool deathByWater = false;
    private float initialEnergy;
    private float currentEnergy;
    private float deltaEnergy;
    private float worldDeltaTime;
    private TileMap_V2 map;

    private float minBirthEnergy = 2f;
    private float minFightMaturity = 0.25f;
    private float birthEnergyCost= 1.5f;
    private float birthLifeCost = 0.25f;
    private float fightEnergyCost = 0.5f;
    private float fightLifeCost = 0;

    private float maturity = 0f;
    

    private float life = 1f;
    private float minLife = 5f;
    private float lifeDecerase = 0.7f;
    private float eatDamage = 0f;
    private float veloDamage = 3f;
    private float angDamage = 0.5f;
    private float fightDamage =  0.5f;
    private float colorChangeDamage = 0.5f;
    private float brainCalculations = 100f;

    private float waterDamage = 20f;
    private float eggTimer = 0f;
    private bool eggHatched = false;
    private float predLevel = 0.5f;
    private float preyLevel;
    
    //private NEATBrain brain;
    private RNN brain;
    private float eggLife = 0.01f;
    private float currentEggLife = 0.01f;
    private float eggFactor = 0f;
    private float eggPrematureBirthDamage = 0.45f;
   
    public Energy(float initialEnergy, float currentEnergy, float life, TileMap_V2 map, float worldDeltaTime, float minLife, float lifeDecerase, float eatDamage, float veloDamage, float angDamage, float fightDamage, float eggTimer, float predLevel, RNN brain, float minBirthEnergy, float minFightMaturity, float birthEnergyCost, float birthLifeCost, float fightEnergyCost, float fightLifeCost, float maturity
        ,float eggLife, float currentEggLife, float eggFactor, float eggPrematureBirthDamage, float waterDamage)
    {                                                                                                                                                                                                                                                           
        this.brain = brain;                                                                                                                                                                                                                                     
        this.initialEnergy = initialEnergy;                                                                                                                                                                                                                     
        this.currentEnergy = currentEnergy;                                                                                                                                                                                                                     
        this.life = life;                                                                                                                                                                                                                                       
        this.map = map;
        this.worldDeltaTime = worldDeltaTime;
        this.eatDamage = eatDamage;
        this.veloDamage = veloDamage;
        this.angDamage = angDamage;
        this.fightDamage = fightDamage;

        this.minLife = minLife;
        this.lifeDecerase = lifeDecerase;
        this.brainCalculations = brain.GetCalculations();
        this.eggTimer = eggTimer;
        this.predLevel = predLevel;
        this.preyLevel = 1f - predLevel; 

        this.minBirthEnergy = minBirthEnergy;
        this.minFightMaturity = minFightMaturity;
        this.birthEnergyCost =birthEnergyCost;
        this.birthLifeCost = birthLifeCost;
        this.fightEnergyCost =fightEnergyCost;
        this.fightLifeCost = fightLifeCost;

        this.maturity = maturity;

        //this.eggLife = maxEggLife;
        this.eggLife = eggLife;
        this.currentEggLife = currentEggLife;
        this.eggFactor = eggFactor;
        this.eggPrematureBirthDamage = eggPrematureBirthDamage;

        this.waterDamage = waterDamage;

        if (maturity >= eggTimer)
        {
            eggHatched = true;
            eggTimer = 0f;
        }

    }

    public void UpdateCreatureEnergy(int x, int y, float[] output, float groundHue, float mouthHue, Creature_V2 spikeCreature)
    {
        float accelForward = output[0];
        float accelAngular = output[1];
        float bodyColor = output[2];
        float eatFood = output[4];
        float birth = output[5];
        float fight = output[6];
        float waterLifeLost = 0f;

        deltaEnergy = 0f;

        deltaEnergy -= ((worldDeltaTime / 5f) * (Mathf.Sqrt(Mathf.Max(currentEnergy / initialEnergy, 1f)))); //natural energy loss (creature will die in 5 years)
        //deltaEnergy -= (Mathf.Pow(brainCalculations,1.05f)* worldDeltaTime* (Mathf.Sqrt(Mathf.Max(currentEnergy / initialEnergy, 1f)))) * 0.001f;  
        //deltaEnergy -= (brainCalculations * worldDeltaTime  * 0.0001f);

        predLevel = 1f; 
        deltaEnergy -= (Mathf.Abs(accelForward) * worldDeltaTime) / (veloDamage* predLevel)/*5f3f*/; //if creature keep moving at max speed it will die in 5 years
        deltaEnergy -= (Mathf.Abs(accelAngular) * worldDeltaTime) / (angDamage * predLevel)/*1f0.5f*/; //if creature keeps turing at max acceleration it will die in 2 years
                                                                                                       // At worst if the creatures keep turning and moving at max rate it will die in 1.1 year

        /*if (Mathf.Abs(accelForward) > 0.5f)
        {
            deltaEnergy -= (Mathf.Abs(accelForward*4f) * worldDeltaTime) / (veloDamage * predLevel);
        }*/

        if (bodyColor>=0f) {
            deltaEnergy -= worldDeltaTime / (colorChangeDamage);
        }

        int tileType = map.GetTileType(x, y);

        if (tileType == Tile_V2.TILE_WATER || x<0 || y<0)
        {
            waterLifeLost = worldDeltaTime *waterDamage/** 0.75f*/; 
            deltaEnergy -= waterLifeLost;
        }
        else 
        {
            // 1 (cirmum) = 2*pi*r 
            // 1/(pi*2) = r = 0.15915494309189533576888376337251
            // turn r = 1 will make c = 2*pi
            // lenght of two chords combined = 2*sin(theta/2)
            if (tileType == Tile_V2.TILE_FERT)
            {
                

                /*float mouthX = Mathf.Cos(mouthHue * Mathf.PI * 2);
                float mouthY = Mathf.Sin(mouthHue * Mathf.PI * 2);
                float groundX = Mathf.Cos(groundHue * Mathf.PI * 2);
                float groundY = Mathf.Sin(groundHue * Mathf.PI * 2);
                float dist = Mathf.Pow(Mathf.Pow(mouthX - groundX, 2) + Mathf.Pow(mouthY - groundY, 2), 0.5f);*/

                //deltaEnergy -= (dist * worldDeltaTime * eatDamage); //2 is good
                float dist = 0f;

                if (mouthHue > groundHue) {
                    float diff1 = mouthHue - groundHue;
                    float diff2 = (1f - mouthHue) + groundHue;

                    if (diff1 < diff2)
                        dist = diff1;
                    else
                        dist = diff2;
                }
                else if(mouthHue < groundHue) {
                    float diff1 = groundHue - mouthHue;
                    float diff2 = (1f - groundHue) + mouthHue;

                    if (diff1 < diff2)
                        dist = diff1;
                    else
                        dist = diff2;
                }

                if (eatFood > 0)
                {
                    float eat = (map.Eat(x, y) - Mathf.Abs(accelForward) * 0.001f);

                    //eat *= preyLevel*2f;

                    if (eat < 0)
                        eat = 0;
                    deltaEnergy += (eat - (eat*(dist/0.5f))*eatDamage);
                }

                //deltaEnergy -= (dist * worldDeltaTime * eatDamage); //2 is good
            }
            else
            {
                brain.MutateExistingNetwork();
            }
        }

        if (fight > 0)
        {
            deltaEnergy -= (worldDeltaTime *fightEnergyCost);
            life -= (worldDeltaTime * fightLifeCost);
            if (spikeCreature != null && maturity > minFightMaturity)
            {
                float enemyEnergy = spikeCreature.GetEnergy();
                if (enemyEnergy > 0f)
                {
                    float energySuck = fightDamage * (Mathf.Max(currentEnergy, 0f)) * 0.1f;

                    enemyEnergy -= energySuck;
                    if (enemyEnergy < 0f)
                        energySuck = (energySuck + enemyEnergy) + 0.005f;

                    //energySuck *= (predLevel * 5f); //<<

                    deltaEnergy += (energySuck);
                    spikeCreature.AffectEnergy(-energySuck); 
                }

                float enemyLife = spikeCreature.GetLife();
                if (enemyLife > 0f)
                {
                    float lifeSuck = fightDamage * 0.1f; //* (Mathf.Max(currentEnergy, 0f)) *0.1f;

                    //lifeSuck *= (predLevel * 2f); //<<

                    enemyLife -= lifeSuck;

                    life += (lifeSuck);
                    spikeCreature.AffectLife(-lifeSuck);
                }
            }
        }

        if (currentEnergy > minBirthEnergy && birth > 0f && birthFrameCounter == 0)
        {
            birthFrameCounter++;
        }
        else if (birthFrameCounter <6 && birthFrameCounter>0 && birth > 0f)
        {
            birthFrameCounter++;
        }
        else if(birthFrameCounter == 6 && birth > 0f)
        {
            deltaEnergy -= birthEnergyCost;
            life -= birthLifeCost; 
            giveBirth = true;
            birthFrameCounter = 0;
        }

        currentEnergy += deltaEnergy;
        maturity += worldDeltaTime;

        //life -= ((worldDeltaTime /(minLife + ((Mathf.Pow(predLevel,2f)* minLife * 2f)))) / Mathf.Pow(Mathf.Max(currentEnergy, 1f), lifeDecerase));
        float factor = 1f;
        /*if (currentEnergy < 0f)
            factor = factor + Mathf.Max(Mathf.Abs(currentEnergy), 1f);*/

        life -= factor * ((worldDeltaTime / minLife) / Mathf.Pow(Mathf.Max(currentEnergy, 1f), lifeDecerase));

        if (currentEnergy <= 0f || life <= 0)
        {
            isAlive = false;
            if (waterLifeLost > 0f)
            {
                deathByWater = true;
            }
        }
    }

    public void UpdateEggTimer()
    {
        if (eggHatched == false)
        {
            eggTimer -= worldDeltaTime;
        }

        if ((eggTimer*eggFactor) <= 0f)
        {
            eggHatched = true;

  
            life = life - ((eggPrematureBirthDamage - (brain.GetBehaviourGenome().eggBirthTimer* eggPrematureBirthDamage))* eggFactor);
            life = life - (eggLife-currentEggLife);
        }
    }

    float MyPow(float num, int exp)
    {
        float result = 1.0f;
        while (exp > 0)
        {
            if (exp % 2 == 1)
                result *= num;
            exp >>= 1;
            num *= num;
        }

        return result;
    }

    public bool GetDeathByWater()
    {
        return deathByWater;
    }

    public float GetEnergy()
    {
        return currentEnergy;
    }

    public float GetDeltaEnergy()
    {
        return deltaEnergy;
    }

    public float GetLife()
    {
        return life;
    }

    public void AffectEnergy(float energy)
    {
        currentEnergy += energy;
    }

    public void AffectLife(float life)
    {
        if (eggHatched == true)
            this.life += life;
        else
        {
            currentEggLife += life;
            if (currentEggLife <= 0)
                isAlive = false;
        }

        
    }

    public bool GiveBirth()
    {
        return giveBirth;
    }

    public void DoneBirth()
    {
        giveBirth = false;
        birthFrameCounter = 0;
    }

    public bool IsAbleToGiveBirth()
    {
        return birthFrameCounter > 0;
    }

    public bool IsAlive()
    {
        return isAlive;
    }

    public void SetKillEnergy()
    {
        currentEnergy = -10000f;
        life = -10000f;
        currentEggLife = -10000f;
    }

    public void SetEnergy(float currentEnergy)
    {
        this.currentEnergy = currentEnergy;
    }

    public void SetLife(float life)
    {
        this.life = life;
    }

    public bool EggHatched()
    {
        return eggHatched;
    }

    public void world_delta_time(float value)
    {
        worldDeltaTime = value;
    }

    public void min_life(float value)
    {
        minLife = value;
    }

    public void life_decrease(float value)
    {
        lifeDecerase = value;
    }

    public void eat_damage(float value)
    {
        eatDamage = value;
    }

    public void velo_damage(float value)
    {
        veloDamage = value;
    }

    public void ang_damage(float value)
    {
        angDamage = value;
    }

    public void fight_damage(float value)
    {
        fightDamage = value;
    }

    public void min_energy_to_birth(float value)
    {
         minBirthEnergy= value;
    }

    public void min_fight_maturity(float value)
    {
         minFightMaturity= value;
    }

    public void birth_energy_cost(float value)
    {
         birthEnergyCost= value;
    }

    public void birth_life_cost(float value)
    {
         birthLifeCost= value;
    }

    public void fight_energy_cost(float value)
    {
         fightEnergyCost= value;
    }


    public void fight_life_cost(float value)
    {
         fightLifeCost= value;
    }

    public void water_damage(float value)
    {
        waterDamage = value;
    }
}
