using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DynamicHyperParameter : MonoBehaviour {

    public RectTransform dynamicSetting;
    public Dictionary<string, Text> sliderText;

    [NonSerialized]
    public float mutationNumber = 1000;
    [NonSerialized]
    public float mutationSign = 1;
    [NonSerialized]
    public float mutationRandom = 1;
    [NonSerialized]
    public float mutationIncrease = 1;
    [NonSerialized]
    public float mutationDecrease = 1;
    [NonSerialized]
    public float mutationWeakerParentFactor = 1;

    [NonSerialized]
    public float climate = 3f;
    [NonSerialized]
    public float minLife = 5.5f;
    [NonSerialized]
    public float lifeDecrease = 0.5f;
    [NonSerialized]
    public float eatDamage = 1f;
    [NonSerialized]
    public float veloDamage = 3f;
    [NonSerialized]
    public float angDamage = 0.5f;
    [NonSerialized]
    public float fightDamage = 0.1f;
    [NonSerialized]
    public float nutritionAmplitude = 0.001f;
    [NonSerialized]
    public int nutritionPower = 4;

    [NonSerialized]
    public float seedSoilFracture = 1f;
    [NonSerialized]
    public float seedSoilColor = 0.5f;
    [NonSerialized]
    public float seedWater = 1425f;
    [NonSerialized]
    public float seedSoil = 134f;
    [NonSerialized]
    public float seedSoilPower = 1.25f;
    [NonSerialized]
    public float seedFirt = 8925;
    [NonSerialized]
    public float seedSoilFirt = 5f;
    [NonSerialized]
    public float seedSoilFirtPower = 0.3f;

    [NonSerialized]
    public bool useCustomMap = false;
    [NonSerialized]
    public bool speciesTileVisual = false;
    [NonSerialized]
    public bool drawThreadColor = false;

    [NonSerialized]
    public float speciesSimilarityScore = 0.95f;
    [NonSerialized]
    public int[] layer;  // test with 30 middle
    [NonSerialized]
    public float[] initialVisionAngles = new float[] { 16.875f, 11.25f, 11.25f, 11.25f };

    [NonSerialized]
    public float minVisionLength = 0;

    [NonSerialized]
    public int numberOfTheads = 4;

    [NonSerialized]
    public float minBirthEnergy = 2f;
    [NonSerialized]
    public float minFightMaturity = 0.25f;
    [NonSerialized]
    public float birthEnergyCost = 1.5f;
    [NonSerialized]
    public float birthLifeCost = 0.25f;
    [NonSerialized]
    public float fightEnergyCost = 0.5f;
    [NonSerialized]
    public float fightLifeCost = 0;
    [NonSerialized]
    public float creatureSize = 0.2f;
    [NonSerialized]
    public int minCreatureCount = 60;
    [NonSerialized]
    public int playSpeedVisual = 11;
    [NonSerialized]
    public float worldDeltaTime = 0.002f;
    [NonSerialized]
    public float speciesGroupingAlgorithm = 1;

    [NonSerialized]
    public float eggLife = 0.01f;
    [NonSerialized]
    public float eggFactor = 0f;
    [NonSerialized]
    public float eggPrematureBirthDamage = 0.45f;

    [NonSerialized]
    public float waterDamage = 20f;
    [NonSerialized]
    public bool fastCompute = false;

    private string[] parameterNames;

    private float[,] bounding;
   
    private WolrdManager_V2 manager;

    [NonSerialized]
    public int numberOfEyeSensors;

    public void OnChanged(int index)
    {
        
    }

    public void LoadHyperParameter(WolrdManager_V2 manager)
    {
        this.manager = manager;
        string filename = "world_hyper_parameters.txt";

        if (!File.Exists(filename))
        {
            StreamWriter writer = new StreamWriter(filename);
            Debug.Log("Saving File");
            writer.WriteLine("neural_layer: 20 11 11");
            writer.WriteLine("min_creature_count: 60                    //Minimum amount of creatures to spawn");
            writer.WriteLine("max_visual_speed: 17                  //Play speed at which rendering is stopped");
            writer.WriteLine("climate: 3                   //Controls rate at which food grows. Ex: 1 is super low, and 10 is super high");
            writer.WriteLine("min_life: 5.5                 //Minimum years creature gets to live (depends on other variables, such as food etc)");
            writer.WriteLine("life_decrease: 0.5                    //Rate at which creature looses life due to lack of energy. >>>Formula: life = life - ((worldDeltaTime / minLife) / Mathf.Pow(Mathf.Max(currentEnergy, 1f), lifeDecerase))<<< A creature with energy of 3 will loose life 40% slower than a creature with energy of 1. ");
            writer.WriteLine("eat_damage: 0                 //Damage a creature takes based on different between mouth and ground hue. >>>Formula: damage = (colorDifferance * worldDeltaTime * eatDamage)<<<");
            writer.WriteLine("velo_damage: 3                    //Damage a creature takes for moving. >>>Formula: damage= (Mathf.Abs(velocityForward) * worldDeltaTime) / veloDamage<<<");
            writer.WriteLine("ang_damage: 1                   //Damage a creature takes for rotating. >>>Formula: damage= (Mathf.Abs(angularVelocity) * worldDeltaTime) / angDamage<<<");
            writer.WriteLine("fight_damage: 0.1                 //Damage factor a creature can inflict onto another creature >>>Formula: energyTaken = fightDamage * (Mathf.Max(currentEnergy, 0f))<<<");
            writer.WriteLine("food_nutrition_power: 1                   //Rate at which food on title decays. Keep between 1 to 5 >>>Formula: energy = energy - (1f-currentTileEnergy)^nutritionPower) * nutritionAmplitude<<<");
            writer.WriteLine("food_nutrition_amplitude: 0.01                   //Max food loss. >>>Formula: energy = energy - (1f-currentTileEnergy)^nutritionPower) * nutritionAmplitude<<<");
            writer.WriteLine("min_energy_to_birth: 2                    //Minimum energy creature must have before it can give birth.");
            writer.WriteLine("min_fight_maturity: 0.25                  //Minimum years for creature to have been alive before it's allowed to be able to fight.");
            writer.WriteLine("birth_energy_cost: 1.5                    //Cost of energy to give birth. >>>Formula: energy = energy - birthEnergyCost<<<");
            writer.WriteLine("birth_life_cost: 0.25                 //Cost of life to give birth. >>>Formula: life = life - birthLifeCost<<<");
            writer.WriteLine("fight_energy_cost: 0.5                    //Cost of energy to have spike out >>>Formula: energy = energy - (worldDeltaTime * fightEnergyCost)<<<");
            writer.WriteLine("fight_life_cost: 0                    //Cost of life to have spike out >>>Formula: life = life - (worldDeltaTime * fightLifeCost)<<<");
            writer.WriteLine("egg_life: 0.01                    //Life an egg has when in the egg state. If a creature is an egg and it's life is dropped below egg life limit, it will die. If it lives, the damage taken is then subtracted from the overall life of the creature.");
            writer.WriteLine("egg_timer: 0                 //Time factor in scaling egg hatching. If set to 0, that mean creatures do not spawn as eggs.  If (eggCurrentTimer*egg_timer)<=0 = Then egg hatched is true");
            writer.WriteLine("egg_premature_birth_damage: 0.45                  //Damage taken by an egg if it hatches too early. >>>Formula: life = life - ((eggPrematureBirthDamage - (eggBirthTimer* eggPrematureBirthDamage))* eggFactor)<<<. Note: Egg birth timer is the mutatable part of the genome.");
            writer.WriteLine("water_damage: 20                  //Damage a creature takes when on water. >>>Formula: energy = energy - (worldDeltaTime*waterDamage)<<<");
            writer.WriteLine("initial_vision_angles: 4 16.875 11.25 11.25 11.25                   //Initial vision angles all creatures start from. Structure: Initial Angle, Offset 1, Offset 2, Offset 3. Ex: 16.875 11.25 11.25 11.25 gives the cone vision structure");
            writer.WriteLine("min_vision_length: 0                  //Minimum size of the vision. >>>Formula: visionLenght = minVisionLength. if visionLength <minVisionLength, make visionLength = minVisionLength <<<");
            writer.WriteLine("creature_size: 0.2                   //Size of creature in meters (0.2 recommended)");
            writer.WriteLine("@world_delta_time: 0.004                  //Time step for this work. 0.0033 is recommened. Keep betweem 0.001 - 0.1.");
            writer.WriteLine("@mutation_number: 1000                    //Keep it 1000. Only applies to mutation_sign, random, increase and decrease");
            writer.WriteLine("@mutation_weaker_parent_factor: 333                 //Chance of mutation_weaker_parent_factor/1000 PER neural connection for weaker parents's connection weights to be applied and switched from stronger parent");
            writer.WriteLine("@mutation_sign: 0.8                 //Chance of mutation out of mutation_number. Ex: mutation_random = 0.2, mutation_number = 1000, then the chance of this mutation will be (0.2/1000)*100% = 0.02%. This seems like a lower number, BUT each neural connection will have this chance to get a mutation. Thus in a neural network with 2000 connection the probability rises drastically.");
            writer.WriteLine("@mutation_random: 0.2                   //Same as mutation_sign");
            writer.WriteLine("@mutation_increase: 0.2             //Same as mutation_sign");
            writer.WriteLine("@mutation_decrease: 0.8                 //Same as mutation_sign");
            writer.WriteLine("@seed_soil_fracture: 2.5                    //Pick any number between 0.1 and 10. Try it out :D");
            writer.WriteLine("@seed_soil_color: 0.65                 //Makes tighter grouping of colors. Keep between 0.5 to 3");
            writer.WriteLine("@seed_disp_water: 1425                    //Water displacement for perlin noise");
            writer.WriteLine("@seed_disp_soil: 134                  //Soil displacement for perlin noise");
            writer.WriteLine("@seed_soil_power: 1.4                    //Adds more reddness to the soil.");
            writer.WriteLine("@seed_disp_fertility: 8925                    //Fertility displacement for perlin noise");
            writer.WriteLine("@seed_soil_fertility: 5                   //I have no idea what this does or why I added it. ");
            writer.WriteLine("@seed_soil_fertility_power: 0.3                  //Ground fertility. Keep between 0 and 1");
            writer.WriteLine("use_custom_map: 0                 // 0 = create procedural map. 1 = create map from png file. If you set it to 1, make sure you have map.png and map_spawn.png files in same folder as the game.");
            writer.WriteLine("species_similarity_score: 0.945                  //Species score of greater than or equal to this means creatures are in same species.");
            writer.WriteLine("species_grouping_algorithm: 0                 //0 = Random check from all species (less computationaly costly but may on occasion group two creatures from different species into the same species). Greater than 0 = creature must match at least (species_grouping_algorithm*100%) from any species");
            writer.WriteLine("use_fast_less_precise_tanh: 0                 //0 = slow more perfect tanh caluclated with double size. 1 = much fast tanh calculated without use of exp function and clipping range of -3 and 3. Note that fast tanh is vastly less precise!!!");
            writer.Write("number_of_threads: 4                  //Number of threads for multithreading. 4 is HIGHLY HIGHLY RECOMMENDED. 1 = no multithreading OBVIOUSLY. Keep under 10.");

            writer.Close();
        }

        StreamReader reader = new StreamReader(filename);

        string[] neuralNetwork = reader.ReadLine().Split(' ');
        string[] minCreatureCountLine = reader.ReadLine().Split(' ');
        string[] maxVisualSpeedLine = reader.ReadLine().Split(' ');
        string[] climateLine = reader.ReadLine().Split(' ');
        string[] minLifeLine = reader.ReadLine().Split(' ');
        string[] lifeDecreaseLine = reader.ReadLine().Split(' ');
        string[] eatDamageLine = reader.ReadLine().Split(' ');
        string[] veloDamageLine = reader.ReadLine().Split(' ');
        string[] angDamageLine = reader.ReadLine().Split(' ');
        string[] fightDamageLine = reader.ReadLine().Split(' ');
        string[] foodNutritionPowerLine = reader.ReadLine().Split(' ');
        string[] foodNutritionAmplitudeLine = reader.ReadLine().Split(' ');
        string[] minEnergyToBirthLine = reader.ReadLine().Split(' ');
        string[] minFightMaturityLine = reader.ReadLine().Split(' ');
        string[] birthEnergyCostLine = reader.ReadLine().Split(' ');
        string[] birthLifeCostLine = reader.ReadLine().Split(' ');
        string[] fightEnergyCostLine = reader.ReadLine().Split(' ');
        string[] fightLifeCostLine = reader.ReadLine().Split(' ');
        string[] eggLifeLine = reader.ReadLine().Split(' ');
        string[] eggfactorLine = reader.ReadLine().Split(' ');
        string[] eggPrematureBirthDamageLine = reader.ReadLine().Split(' ');
        string[] waterDamageLine = reader.ReadLine().Split(' ');
        string[] initialVisionAnglesLine = reader.ReadLine().Split(' ');
        string[] minVisionLengthLine = reader.ReadLine().Split(' ');
        string[] creatureSizeLine = reader.ReadLine().Split(' ');
        string[] worldDeltaTimeLine = reader.ReadLine().Split(' ');
        string[] mutationNumberLine = reader.ReadLine().Split(' ');
        string[] mutationWeakerParentFactorLine = reader.ReadLine().Split(' ');
        string[] mutationSignLine = reader.ReadLine().Split(' ');
        string[] mutationRandomLine = reader.ReadLine().Split(' ');
        string[] mutationIncreaseLine = reader.ReadLine().Split(' ');
        string[] mutationDecreaseLine = reader.ReadLine().Split(' ');
        string[] seedSoilFractureLine = reader.ReadLine().Split(' ');
        string[] seedSoilColorLine = reader.ReadLine().Split(' ');
        string[] seedWaterLine = reader.ReadLine().Split(' ');
        string[] seedSoilLine = reader.ReadLine().Split(' ');
        string[] seedSoilPowerLine = reader.ReadLine().Split(' ');
        string[] seedFirtLine = reader.ReadLine().Split(' ');
        string[] seedSoilFirtLine = reader.ReadLine().Split(' ');
        string[] seedSoilFirtPowerLine = reader.ReadLine().Split(' ');
        string[] useCustomMapLine = reader.ReadLine().Split(' ');
        string[] speciesSimilarityScoreLine = reader.ReadLine().Split(' ');
        string[] speciesGroupingAlgorithmLine = reader.ReadLine().Split(' ');
        string[] fastTanHLine = reader.ReadLine().Split(' ');
        string[] numberOfThreadsLine = reader.ReadLine().Split(' ');


        numberOfEyeSensors = int.Parse(initialVisionAnglesLine[1]);
        int numberOfInputs = 10 + (4 * numberOfEyeSensors);
        int numberOfOutputs = 7;
        //layer = new int[] { numberOfInputs, 20, 11, 11, numberOfOutputs };  
        int numberOfLayers = neuralNetwork.Length - 1;
        layer = new int[numberOfLayers + 2];
        layer[0] = numberOfInputs;
        layer[layer.Length - 1] = numberOfOutputs;
        for (int i = 1; i < neuralNetwork.Length; i++)
            layer[i] = int.Parse(neuralNetwork[i]);
        
        minCreatureCount = int.Parse(minCreatureCountLine[1]);
        playSpeedVisual = int.Parse(maxVisualSpeedLine[1]);
        worldDeltaTime = float.Parse(worldDeltaTimeLine[1]);
        climate = float.Parse(climateLine[1]);
        minLife = float.Parse(minLifeLine[1]);
        lifeDecrease = float.Parse(lifeDecreaseLine[1]);
        eatDamage = float.Parse(eatDamageLine[1]);
        veloDamage = float.Parse(veloDamageLine[1]);
        angDamage = float.Parse(angDamageLine[1]);
        fightDamage = float.Parse(fightDamageLine[1]);
        nutritionPower = (int)(float.Parse(foodNutritionPowerLine[1]));
        nutritionAmplitude = float.Parse(foodNutritionAmplitudeLine[1]);

        minBirthEnergy = float.Parse(minEnergyToBirthLine[1]);
        minFightMaturity = float.Parse(minFightMaturityLine[1]);
        birthEnergyCost = float.Parse(birthEnergyCostLine[1]);
        birthLifeCost = float.Parse(birthLifeCostLine[1]);
        fightEnergyCost = float.Parse(fightEnergyCostLine[1]);
        fightLifeCost = float.Parse(fightLifeCostLine[1]);

        eggLife = float.Parse(eggLifeLine[1]);
        eggFactor = float.Parse(eggfactorLine[1]);
        eggPrematureBirthDamage = float.Parse(eggPrematureBirthDamageLine[1]);

        waterDamage = float.Parse(waterDamageLine[1]);

        initialVisionAngles = new float[numberOfEyeSensors];
        for (int i = 0; i < numberOfEyeSensors; i++) {
            initialVisionAngles[i] = float.Parse(initialVisionAnglesLine[i+2]);
        }
        
        minVisionLength = float.Parse(minVisionLengthLine[1]);
        //initialVisionAngles = new float[] { float.Parse(initialVisionAnglesLine[1]), float.Parse(initialVisionAnglesLine[2]), float.Parse(initialVisionAnglesLine[3]), float.Parse(initialVisionAnglesLine[4])};
        creatureSize = float.Parse(creatureSizeLine[1]);

      
        mutationNumber = float.Parse(mutationNumberLine[1]);
        mutationWeakerParentFactor = float.Parse(mutationWeakerParentFactorLine[1]);
        mutationSign = float.Parse(mutationSignLine[1]);
        mutationRandom = float.Parse(mutationRandomLine[1]);
        mutationIncrease = float.Parse(mutationIncreaseLine[1]);
        mutationDecrease = float.Parse(mutationDecreaseLine[1]);
        seedSoilFracture = float.Parse(seedSoilFractureLine[1]);
        seedSoilColor = float.Parse(seedSoilColorLine[1]);
        seedWater = float.Parse(seedWaterLine[1]);
        seedSoil = float.Parse(seedSoilLine[1]);
        seedSoilPower = float.Parse(seedSoilPowerLine[1]);
        seedFirt = float.Parse(seedFirtLine[1]);
        seedSoilFirt = float.Parse(seedSoilFirtLine[1]);
        seedSoilFirtPower = float.Parse(seedSoilFirtPowerLine[1]);
        useCustomMap = float.Parse(useCustomMapLine[1]) == 0 ? false : true;
        speciesSimilarityScore = float.Parse(speciesSimilarityScoreLine[1]);
        speciesGroupingAlgorithm = float.Parse(speciesGroupingAlgorithmLine[1]);
        fastCompute = float.Parse(fastTanHLine[1]) == 0 ? false : true;
        numberOfTheads = int.Parse(numberOfThreadsLine[1]);
        reader.Close();

        //Debug.Log(minBirthEnergy+" "+ minFightMaturity+" "+ birthEnergyCost + " " + birthLifeCost + " " + fightEnergyCost + " " + fightLifeCost+" "+ initialVisionAngles[0]+" "+ initialVisionAngles[1]+" "+ initialVisionAngles[2]+" "+ initialVisionAngles[3]);
        //Debug.Log(nutritionPower +" "+ nutritionAmplitude);
        Debug.Log(eggLife + " " + eggFactor + " " + eggPrematureBirthDamage + " " + waterDamage + " " + fastCompute);


        if (minCreatureCount > 500)
            minCreatureCount = 500;

        if (speciesSimilarityScore <= 0f)
            speciesSimilarityScore = 0.001f;
        if (speciesSimilarityScore >1f)
            speciesSimilarityScore = 1f;

        sliderText = new Dictionary<string, Text>();
        parameterNames = new string[]
        {
            "min_creature_count","climate_","min_life","life_decrease","eat_damage",
            "velo_damage","ang_damage","fight_damage","food_nutrition_power","food_nutrition_amplitude",
            "min_energy_to_birth","min_fight_maturity","birth_energy_cost","birth_life_cost","fight_energy_cost",
            "fight_life_cost","egg_life","egg_timer","egg_premature_birth_damage","water_damage",
            "creature_size","world_delta_time","mutation_number","mutation_weaker_parent_factor",
            "mutation_sign","mutation_random","mutation_increase","mutation_decrease","species_similarity_score",
            "species_grouping_algorithm", "min_vision_length"
        };

        bounding = new float[,]
        {
            { 1,300, minCreatureCount}, { 0,20, climate}, { 1,15, minLife}, { 0,3, lifeDecrease}, { 0,15,eatDamage}, 
            { 0,3, veloDamage}, { 0,3, angDamage}, { 0,0.5f, fightDamage}, { 1,8, nutritionPower}, { 0,0.5f, nutritionAmplitude},
            { 0,5, minBirthEnergy}, { 0,1, minFightMaturity}, { 0,3, birthEnergyCost}, { 0,1, birthLifeCost}, { 0,5f,fightEnergyCost}, 
            { 0f,5f, fightLifeCost}, {0f,1f,eggLife}, {0f,1f,eggFactor}, {0f,1f,eggPrematureBirthDamage},{0f,20f,waterDamage},
            { 0.1f,0.5f, creatureSize}, { 0.001f,0.1f,worldDeltaTime}, { 0,1000,mutationNumber},{ 0,1000, mutationWeakerParentFactor}, 
            { 0f,2f,mutationSign}, { 0f,2f, mutationRandom},{ 0f,2f,mutationIncrease},{ 0f,2f, mutationDecrease},
            { 0f,1f,speciesSimilarityScore},{ 0f,1f,speciesGroupingAlgorithm}, { 0f,1f,minVisionLength}
        };

































































































































































































































































































































































        UnityAction<float>[] callbacks = 
            {
                manager.min_creature_count,manager.climate_,manager.min_life,manager.life_decrease,manager.eat_damage,
                manager.velo_damage,manager.ang_damage,manager.fight_damage,manager.food_nutrition_power,manager.food_nutrition_amplitude,
                manager.min_energy_to_birth,manager.min_fight_maturity,manager.birth_energy_cost,manager.birth_life_cost,manager.fight_energy_cost,
                manager.fight_life_cost, manager.egg_life,manager.egg_factor,manager.egg_premature_birth_damage,manager.water_damage,
                manager.creature_size,manager.world_delta_time,manager.mutation_number,manager.mutation_weaker_parent_factor,
                manager.mutation_sign,manager.mutation_random,manager.mutation_increase,manager.mutation_decrease,manager.species_similarity_score,
                manager.species_grouping_algorithm, manager.min_vision_length
            };
        for (int i = 0; i < dynamicSetting.childCount; i++)
        {

            Text sliderName = dynamicSetting.GetChild(i).GetComponent<Text>();
            Slider slider = sliderName.GetComponent<RectTransform>().GetChild(0).GetComponent<Slider>();
            Text sliderValue = sliderName.GetComponent<RectTransform>().GetChild(1).GetComponent<Text>();

            sliderName.text = parameterNames[i] + ": {" + bounding[i, 0] + "-" + bounding[i, 1] + "}";
            slider.minValue = bounding[i, 0];
            slider.maxValue = bounding[i, 1];
            slider.value = bounding[i, 2];
            slider.onValueChanged.AddListener(callbacks[i]);
            sliderValue.text = bounding[i, 2]+"";

            sliderText.Add(parameterNames[i], sliderValue);

            /*sliderName.color = Color.black;
            sliderValue.color = Color.black;*/
            
        }
    }

    /*public void min_creature_count(float value)
    {
        minCreatureCount = (int)value;
        sliderText["min_creature_count"].text = (int)value+"";
    }

    public void climate_(float value)
    {
        climate = value;
        sliderText["climate_"].text = value + "";
    }

    public void min_life(float value)
    {
        minLife = value;
        sliderText["min_life"].text = value + "";
    }

    public void life_decrease(float value)
    {
        lifeDecrease = value;
        sliderText["life_decrease"].text = value + "";
    }

    public void eat_damage(float value)
    {
        eatDamage = value;
        sliderText["eat_damage"].text = value + "";
    }

    public void velo_damage(float value)
    {
        veloDamage = value;
        sliderText["velo_damage"].text = value + "";
    }

    public void ang_damage(float value)
    {
        angDamage = value;
        sliderText["ang_damage"].text = value + "";
    }

    public void fight_damage(float value)
    {
        fightDamage = value;
        sliderText["fight_damage"].text = value + "";
    }

    public void food_nutrition_power(float value)
    {
        nutritionPower = (int)value;
        sliderText["food_nutrition_power"].text = value + "";
    }

    public void food_nutrition_amplitude(float value)
    {
        nutritionAmplitude = value;
        sliderText["food_nutrition_amplitude"].text = value + "";
    }

   
    public void min_energy_to_birth(float value)
    {
        minBirthEnergy = value;
        sliderText["min_energy_to_birth"].text = value + "";
    }

    public void min_fight_maturity(float value)
    {
        minFightMaturity = value;
        sliderText["min_fight_maturity"].text = value + "";
    }

    public void birth_energy_cost(float value)
    {
        birthEnergyCost = value;
        sliderText["birth_energy_cost"].text = value + "";
    }

    public void birth_life_cost(float value)
    {
        birthLifeCost = value;
        sliderText["birth_life_cost"].text = value + "";
    }

    public void fight_energy_cost(float value)
    {
        fightEnergyCost = value;
        sliderText["fight_energy_cost"].text = value + "";
    }

    
    public void fight_life_cost(float value)
    {
        fightLifeCost = value;
        sliderText["fight_life_cost"].text = value + "";
    }

    public void creature_size(float value)
    {
        creatureSize = value;
        sliderText["creature_size"].text = value + "";
    }

    public void world_delta_time(float value)
    {
        worldDeltaTime = value;
        sliderText["world_delta_time"].text = value + "";
    }

    public void mutation_number(float value)
    {
        mutationNumber = value;
        sliderText["mutation_number"].text = value + "";
    }

    public void mutation_weaker_parent_factor(float value)
    {
        mutationWeakerParentFactor = value;
        sliderText["mutation_weaker_parent_factor"].text = value + "";
    }

    public void mutation_sign(float value)
    {
        mutationSign = value;
        sliderText["mutation_sign"].text = value + "";
    }

    public void mutation_random(float value)
    {
        mutationRandom = value;
        sliderText["mutation_random"].text = value + "";
    }

    public void mutation_increase(float value)
    {
        mutationIncrease = value;
        sliderText["mutation_increase"].text = value + "";
    }

    public void mutation_decrease(float value)
    {
        mutationDecrease = value;
        sliderText["mutation_decrease"].text = value + "";
    }

    public void species_similarity_score(float value)
    {
        speciesSimilarityScore = value;
        sliderText["species_similarity_score"].text = value + "";
    }

    public void species_grouping_algorithm(float value)
    {
        speciesGroupingAlgorithm = value;
        sliderText["species_grouping_algorithm"].text = value + "";
    }
    */
    
}
