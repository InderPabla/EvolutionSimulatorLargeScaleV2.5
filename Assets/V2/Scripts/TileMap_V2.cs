using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

public class TileMap_V2
{
    private Tile_V2[,] tiles;
    private Texture2D texture;
    private int sizeX;
    private int sizeY;
    float worldDeltaTime = 0.001f; //each year last
    private float maxEnergyGrownOnTile = 0.8f;
    private float climate = 3f; //1 is excellent climate for growth, 0 means nothing will grow, and below zero, vegetation starts to die
    private List<int[]> floorTiles = new List<int[]>();

    private float seedSoilFracture = 1f;
    private float seedSoilColor = 0.5f;
    private float seedWater = 1425f;
    private float seedSoil = 134f;
    private float seedSoilPower = 1.25f;
    private float seedFirt = 8925;
    private float seedSoilFirt = 5f;
    private float seedSoilFirtPower = 0.3f;

    //private Color[] speciesColor = new Color[250];
    private bool heat = true;
    private bool drawHeat = false;
    private float nutritionAmplitude = 0.001f;
    private int nutritionPower = 4;

    //private HSBColor water = new HSBColor(-1,-1,-1);
    //private HSBColor mutate = new HSBColor(-1, 1, -1);
    //private HSBColor water = new HSBColor(0,0,0);
    //private HSBColor mutate = new HSBColor(0,0,1);

    public TileMap_V2(Texture2D tex, int sizeX, int sizeY, float climate, float worldDeltaTime, float seedSoilFracture, float seedSoilColor, float seedWater, float seedSoil, float seedSoilPower, float seedFirt, float seedSoilFirt, float seedSoilFirtPower, bool useCustomMap, int nutritionPower, float nutritionAmplitude)
    {
        this.texture = tex;
        this.sizeX = sizeX;
        this.sizeY = sizeY;
        this.climate = climate;
        this.worldDeltaTime = worldDeltaTime;
        this.nutritionAmplitude = nutritionAmplitude;
        this.nutritionPower = nutritionPower;

        tiles = new Tile_V2[sizeY, sizeX];

        this.seedSoilFracture = seedSoilFracture;
        this.seedSoilColor = seedSoilColor;
        this.seedWater = seedWater;
        this.seedSoil = seedSoil;
        this.seedSoilPower = seedSoilPower;
        this.seedFirt = seedFirt;
        this.seedSoilFirt = seedSoilFirt;
        this.seedSoilFirtPower = seedSoilFirtPower;

        if (useCustomMap == false)
        {
            GenerateTile(seedSoilFracture, seedSoilColor, seedWater, seedSoil, seedSoilPower, seedFirt, seedSoilFirt, seedSoilFirtPower);
        }
        else
        {
            string mapFile = "map.lsesmap";
            if (System.IO.File.Exists(mapFile) == true)
            {
                StreamReader reader = new StreamReader(mapFile);
                string[] readAll = reader.ReadToEnd().Split(' ');
                reader.Close();
                int index = 0;

                for (int y = 0; y < sizeY; y++)
                {
                    for (int x = 0; x < sizeX; x++)
                    {
                        tiles[y,x] = new Tile_V2();
                        float type = float.Parse(readAll[index]);
                        index++;
                        if (type == Tile_V2.TILE_FERT)
                        {
 
                            float hue = float.Parse(readAll[index]);
                            index++;
                            float maxEnergy = float.Parse(readAll[index]);
                            index++;
                            float restrict = float.Parse(readAll[index]);
                            index++;

                            tiles[y,x].type = Tile_V2.TILE_FERT;
                            tiles[y,x].currentEnergy = maxEnergy;
                            tiles[y, x].maxEnergy = maxEnergy;

                            tiles[y, x].detail = new HSBColor(hue, maxEnergy, 1f - (0.25f - (maxEnergy * 0.25f)));
                            //texture.SetPixel(y, x, tiles[y, x].detail.ToColor());
                            if (restrict == 0)
                                floorTiles.Add(new int[] { x, y });
                        }
                        else if(type == Tile_V2.TILE_INFERT)
                        {
                            float value = float.Parse(readAll[index]);
                            index++;
                            float restrict = float.Parse(readAll[index]);
                            index++;

                            tiles[y, x].type = Tile_V2.TILE_INFERT;
                            tiles[y, x].currentEnergy = 0f;
                            tiles[y, x].detail = new HSBColor(-1, 1, -1);
                            texture.SetPixel(x,y, Color.white);

                            if(restrict == 0)
                                floorTiles.Add(new int[] { x,y});
                        }
                        else if (type == Tile_V2.TILE_WATER)
                        {
                            float value = float.Parse(readAll[index]);
                            index++;
                            float restrict = float.Parse(readAll[index]);
                            index++;

                            tiles[y, x].type = Tile_V2.TILE_WATER;
                            tiles[y,x].currentEnergy = 0f;
                            tiles[y, x].detail = new HSBColor(-1, -1, -1);
                            texture.SetPixel(x,y, Color.black);

                        }
                    }
                }
            }
            else
            {
                GenerateTile(seedSoilFracture, seedSoilColor, seedWater, seedSoil, seedSoilPower, seedFirt, seedSoilFirt, seedSoilFirtPower);
            }
            /*byte[] bytes_map = System.IO.File.ReadAllBytes("map.png");
            byte[] bytes_map_spawn = System.IO.File.ReadAllBytes("map_spawn.png");

            Texture2D mapImage = new Texture2D((int)sizeX, sizeY, TextureFormat.RGB24, false);
            Texture2D mapSpawnImage = new Texture2D((int)sizeX, sizeY, TextureFormat.RGB24, false);

            mapImage.LoadImage(bytes_map);
            mapImage.Apply();
            mapSpawnImage.LoadImage(bytes_map_spawn);
            mapSpawnImage.Apply();

            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    tiles[x, y] = new Tile_V2();
                    Color mapColor = mapImage.GetPixel(y, x);
                    Color mapSpawnColor = mapSpawnImage.GetPixel(y, x);

                    HSBColor color = HSBColor.FromColor(mapColor);
                    if (mapColor.r == 0 && mapColor.g == 0 && mapColor.b == 0)
                    {
                        tiles[x, y].currentEnergy = 0f;
                        tiles[x, y].type = Tile_V2.TILE_WATER;
                        color = new HSBColor(-1, -1, -1);
                        //color = new HSBColor(Color.black); 
                        texture.SetPixel(y, x, Color.black);


                    }
                    else if (mapColor.r == 1 && mapColor.g == 1 && mapColor.b == 1)
                    {
                        tiles[x, y].currentEnergy = 0f; //10f
                        tiles[x, y].maxEnergy = 0f; //10f?
                        tiles[x, y].type = Tile_V2.TILE_INFERT;
                        color = new HSBColor(-1, 1, -1);
                        //color = new HSBColor(Color.white);
                        texture.SetPixel(y, x, Color.white);
                    }
                    else
                    {
                        tiles[x, y].type = Tile_V2.TILE_FERT;
                        tiles[x, y].currentEnergy = color.s;
                        tiles[x, y].maxEnergy = color.s;

                        color.b = 1f - (0.25f - (color.s * 0.25f));

                        if (mapColor.r == mapSpawnColor.r && mapColor.g == mapSpawnColor.g && mapColor.b == mapSpawnColor.b)
                        {
                            floorTiles.Add(new int[] { y, x });
                        }
                    }

                    tiles[x, y].detail = color;
                }
            }*/
        }

        texture.Apply();
        //Debug.Log("MAP MADE: "+ floorTiles.Count);
    }

    private void GenerateTile(float seedSoilFracture, float seedSoilColor, float seedWater, float seedSoil, float seedSoilPower, float seedFertil, float seedSoilFirt, float seedSoilFirtPower)
    {
        for (int x = 0; x < sizeY; x++)
        {
            for (int y = 0; y < sizeX; y++)
            {
                tiles[y, x] = new Tile_V2();
                float climateType = 0.6f;
                float type = 1f;
                float firt = 0.8f;

                HSBColor color = new HSBColor();

                float climateX = seedSoil + x / 100f * 10f;
                float climateY = seedSoil + y / 100f * 10f;
                float waterX = seedWater + x / 100f * 15f;
                float waterY = seedWater + y / 100f * 15f;
                float fertX = seedFertil + x / 100f * 13f;
                float fertY = seedFertil + y / 100f * 13f;

                climateType = Mathf.Pow(Mathf.Min(Mathf.Abs(Mathf.PerlinNoise(climateX * seedSoilColor, climateY * seedSoilColor)),1f), seedSoilPower);
                type = Mathf.Min(Mathf.Abs(Mathf.PerlinNoise(waterX * seedSoilFracture, waterY * seedSoilFracture )), 1f);
                firt = Mathf.Pow(Mathf.Min(Mathf.Abs(Mathf.PerlinNoise(fertX * seedSoilFirt, fertY * seedSoilFirt)), 1f), seedSoilFirtPower);

                if (type > 0.4f)
                {
                    tiles[y, x].currentEnergy = firt;
                    tiles[y, x].type = Tile_V2.TILE_FERT;
                }
                else
                {
                    tiles[y, x].currentEnergy = 0f;
                    tiles[y, x].type = Tile_V2.TILE_WATER;
                    texture.SetPixel(x,y, Color.black);
                }

                if (tiles[y, x].type == Tile_V2.TILE_FERT)
                {
                    color.h = Mathf.Max(0f, climateType);
                    color.s = tiles[y, x].currentEnergy;
                    color.b = 1f - (0.25f - (color.s * 0.25f));
                    tiles[y, x].maxEnergy = tiles[y, x].currentEnergy;
                    floorTiles.Add(new int[] { x, y });

                }
                else if (tiles[y, x].type == Tile_V2.TILE_INFERT)
                {
                    color = new HSBColor(-1, 1, -1);
                    tiles[y, x].currentEnergy = 0f;
                    tiles[y, x].maxEnergy = 0f;
                }
                else if (tiles[y, x].type == Tile_V2.TILE_WATER)
                {
                    color = new HSBColor(-1, -1, -1);
                    tiles[y, x].currentEnergy = 0f;
                    tiles[y, x].maxEnergy = 0f;
                }

                tiles[y, x].detail = color;
            }
        }
    }

    public void Apply(float playSpeed, bool visual)
    {

        if (drawHeat == false)
        {
            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {

                    if (tiles[y, x].type == Tile_V2.TILE_FERT)
                    {

                        float missedFramed = (WolrdManager_V2.WORLD_CLOCK - tiles[y, x].lastUpdated) / worldDeltaTime;

                        if (climate > 0)
                        {
                            if (tiles[y, x].currentEnergy < tiles[y, x].maxEnergy)
                            {
                                tiles[y, x].currentEnergy += climate * worldDeltaTime * missedFramed/*playSpeed*/;
                                if (tiles[y, x].currentEnergy > tiles[y, x].maxEnergy)
                                {
                                    tiles[y, x].currentEnergy = tiles[y, x].maxEnergy;
                                }
                            }
                        }
                        else if (climate < 0)
                        {
                            if (tiles[y, x].currentEnergy > 0f)
                            {
                                tiles[y, x].currentEnergy += climate * worldDeltaTime * missedFramed /*playSpeed*/;

                                if (tiles[y, x].currentEnergy < 0)
                                {
                                    tiles[y, x].currentEnergy = 0f;
                                }
                            }
                        }

                        if (tiles[y, x].currentEnergy < 0f)
                            tiles[y, x].currentEnergy = 0f;
                        else if (tiles[y, x].currentEnergy > 1f)
                            tiles[y, x].currentEnergy = 1f;

                        float saturationToEnergyRatio = tiles[y, x].currentEnergy;
                        tiles[y, x].detail.s = saturationToEnergyRatio;
                        tiles[y, x].detail.b = 1f - (0.25f - (saturationToEnergyRatio * 0.25f));


                        //<<
                        if (tiles[y, x].selected == false)
                            texture.SetPixel(x, y, tiles[y, x].detail.ToColor());
                        else
                            texture.SetPixel(x, y, Color.grey);

                        /*if (tiles[y, x].selected == false)
                            texture.SetPixel(x, y, Color.grey);
                        else
                            texture.SetPixel(x, y, Color.grey);*/

                        tiles[y, x].lastUpdated = WolrdManager_V2.WORLD_CLOCK;
                    }
                    else if (tiles[y, x].type == Tile_V2.TILE_INFERT)
                    {
                        texture.SetPixel(x, y, Color.white); //<<
                    }
                    else
                    {
                        //texture.SetPixel(x, y, tiles[y, x].detail.ToColor());
                        texture.SetPixel(x, y, Color.black);
                    }
                }
            }
        }
        else
        {
            float[,] intensity = new float[sizeY, sizeX];

            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {

                    if (tiles[y, x].type == Tile_V2.TILE_FERT)
                    {

                        float missedFramed = (WolrdManager_V2.WORLD_CLOCK - tiles[y, x].lastUpdated) / worldDeltaTime;

                        if (climate > 0)
                        {
                            if (tiles[y, x].currentEnergy < tiles[y, x].maxEnergy)
                            {
                                tiles[y, x].currentEnergy += climate * worldDeltaTime * missedFramed/*playSpeed*/;
                                if (tiles[y, x].currentEnergy > tiles[y, x].maxEnergy)
                                {
                                    tiles[y, x].currentEnergy = tiles[y, x].maxEnergy;
                                }
                            }
                        }
                        else if (climate < 0)
                        {
                            if (tiles[y, x].currentEnergy > 0f)
                            {
                                tiles[y, x].currentEnergy += climate * worldDeltaTime * missedFramed /*playSpeed*/;

                                if (tiles[y, x].currentEnergy < 0)
                                {
                                    tiles[y, x].currentEnergy = 0f;
                                }
                            }
                        }

                        if (tiles[y, x].currentEnergy < 0f)
                            tiles[y, x].currentEnergy = 0f;
                        else if (tiles[y, x].currentEnergy > 1f)
                            tiles[y, x].currentEnergy = 1f;

                        float saturationToEnergyRatio = tiles[y, x].currentEnergy;
                        tiles[y, x].detail.s = saturationToEnergyRatio;
                        tiles[y, x].detail.b = 1f - (0.25f - (saturationToEnergyRatio * 0.25f));

                        tiles[y, x].lastUpdated = WolrdManager_V2.WORLD_CLOCK;
                    }


                    if (tiles[y,x].numberOfCreatures > 0)
                    {
                        int minY = y - 1;
                        int maxY = y +1;
                        int minX = x - 1;
                        int maxX = x + 1;

                        if (minY < 0)
                            minY = 0;
                        if (maxY >= sizeY)
                            maxY = sizeY - 1;
                        if (minX < 0)
                            minX = 0;
                        if (maxX >= sizeX)
                            maxX = sizeX - 1;

                        for (int col = minY; col <= maxY; col++)
                        {
                            for (int row = minX; row <= maxX; row++)
                            {
                                intensity[col, row]+=2;

                            }
                        }
                    }

                }
            }

           

            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {

                    float it = intensity[y, x] / 16f;
                    if (it > 1)
                        it = 1f;

                    texture.SetPixel(x,y, HeatMapColor(it));
                }
            }
        }







        /*if (visual == true)
        {
            texture.Apply();
        }*/
    }

    public void ColorSpeciesTerritory(List<Species> speciesList, bool visual, bool draw)
    {
        if (visual == true)
        {
            /*if (draw == true)
            {
                for (int i = 0; i < speciesList.Count; i++)
                {
                    List<Creature_V2> population = speciesList[i].GetPopulation();
                    for (int j = 0; j < population.Count; j++)
                    {
                        Vector2 pos = population[j].position;
                        if (pos.x < 0)
                            pos.x = 0;
                        else if (pos.x > sizeX)
                            pos.x = sizeX;

                        if (pos.y < 0)
                            pos.y = 0;
                        else if (pos.y > sizeY)
                            pos.y = sizeY;

                        texture.SetPixel((int)pos.x, (int)pos.y, speciesList[i].GetColor());
                    }
                }
            }*/

            texture.Apply();
        }
    }

    public float GetWorldDeltaTime()
    {
        return worldDeltaTime;
    }

    public HSBColor GetColor(int x, int y)
    {
        if (IsValidLocation(x, y) == true)
            return tiles[y, x].detail;

        return /*HSBColor.FromColor(Color.black);*/ new HSBColor(-1,-1,-1);

    }

    public float Eat(int x, int y)
    {
        float energy = 0;

        if (IsValidLocation(x, y) == true)
        {
            if (tiles[y, x].type == Tile_V2.TILE_FERT)
            {
                float missedFramed = (WolrdManager_V2.WORLD_CLOCK - tiles[y, x].lastUpdated) / worldDeltaTime;

                if (tiles[y, x].currentEnergy < tiles[y, x].maxEnergy)
                {
                    tiles[y, x].currentEnergy += climate * worldDeltaTime * missedFramed;
                    if (tiles[y, x].currentEnergy > tiles[y, x].maxEnergy)
                    {
                        tiles[y, x].currentEnergy = tiles[y, x].maxEnergy;
                    }
                }

                float currEnergy = tiles[y, x].currentEnergy;
                energy = worldDeltaTime * 5f;

                tiles[y, x].currentEnergy -= energy;

                if (tiles[y, x].currentEnergy < 0)
                {
                    energy += tiles[y, x].currentEnergy;
                    tiles[y, x].currentEnergy = 0;
                }

                if (currEnergy > 0)
                {
                    energy = energy - MyPow((1f-currEnergy), nutritionPower) * nutritionAmplitude;
                }

                tiles[y, x].lastUpdated = WolrdManager_V2.WORLD_CLOCK;
            }
            else if(tiles[y, x].type == Tile_V2.TILE_INFERT)
            {
                energy = worldDeltaTime * 5f;
            }
        }

        return energy ;
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

    public bool IsValidLocation(int x, int y)
    {
        if (x < 0 || x > sizeX - 1 || y < 0 || y > sizeY - 1)
            return false;

        return true;
    }

    System.Random random = new System.Random();
    public int[] RandomFloorTile()
    {
        return floorTiles[(int)(random.NextDouble()*floorTiles.Count) /*UnityEngine.Random.Range(0, floorTiles.Count)*/];
        //return new int[] {UnityEngine.Random.Range(0,sizeY), UnityEngine.Random.Range(0, sizeX) };
    }

    public int GetTileType(int x, int y)
    {
        if (IsValidLocation(x, y) == true)
        {
            return tiles[y, x].type;
        }
        return Tile_V2.TILE_WATER;
    }

    /*public float GetTileEnergy(int x, int y)
    {
        if (IsValidLocation(x, y) == true)
        {
            return tiles[y, x].currentEnergy;
        }
        return 0f;
    }*/
    
    public void RemoveCreatureFromTileList(int x, int y, Creature_V2 creature)
    {
        if (IsValidLocation(x, y) == true)
        {
            int index = tiles[y, x].creatureListOnTile.IndexOf(creature);
            if (index != -1)
            {
                tiles[y, x].creatureListOnTile.RemoveAt(index);
                tiles[y, x].numberOfCreatures--;
            }
        }
    }

    public void AddCreatureToTileList(int x, int y, Creature_V2 creature)
    {
        if (IsValidLocation(x, y) == true)
        {
            tiles[y, x].creatureListOnTile.Add(creature);
            tiles[y, x].numberOfCreatures++;
        }
    }

    // search in a 3x by 3x grid, (9 searches)
    public List<Creature_V2> ExistCreaturesNearTile(int x, int y)
    {
        List<Creature_V2> creatureIndexList = new List<Creature_V2>();
        if (IsValidLocation(x, y) == true)
        {
            for (int i = y - 1; i <= y + 1; i++)
            {
                for (int j = x - 1; j <= x + 1; j++)
                {
                    if (IsValidLocation(j, i) == true)
                    {
                        creatureIndexList.AddRange(tiles[i, j].creatureListOnTile);
                    }
                }
            }
        }

        return creatureIndexList;
    }

    // search in based on float coords
    public List<Creature_V2> ExistCreaturesNearPrecisionTile(float x, float y, float radius)
    {
        List<Creature_V2> creatureIndexList = new List<Creature_V2>();


        if (IsValidLocation((int)x, (int)y) == true)
        {
            int x1 = (int)(x - radius);
            int x2 = (int)(x + radius);
            int y1 = (int)(y - radius);
            int y2 = (int)(y + radius);

            for (int i = y1; i < y2+1; i++)
            {
                for (int j = x1; j < x2 + 1; j++)
                {
                    if (IsValidLocation(j, i) == true)
                    {
                        List<Creature_V2> list = tiles[i, j].creatureListOnTile;

                        creatureIndexList.AddRange(tiles[i, j].creatureListOnTile);
                    }
                }
            }
        }

        return creatureIndexList;
    }

    public List<Creature_V2> ExistCreaturesBetweenTiles(int x1, int y1, int x2, int y2)
    {
        List<Creature_V2> creatureIndexList = new List<Creature_V2>();


        if (IsValidLocation((int)x1, (int)y1) == true && IsValidLocation((int)x2, (int)y2) == true)
        {
            if (x1 > x2)
            {
                int temp = x2;
                x2 = x1;
                x1 = temp;
            }

            if (y1 > y2)
            {
                int temp = y2;
                y2 = y1;
                y1 = temp;
            }

            for (int i = y1; i < y2 + 1; i++)
            {
                for (int j = x1; j < x2 + 1; j++)
                {
                    if (IsValidLocation(j, i) == true)
                    {
                        List<Creature_V2> list = tiles[i, j].creatureListOnTile;

                        creatureIndexList.AddRange(tiles[i, j].creatureListOnTile);
                    }
                }
            }

        }

        return creatureIndexList;
    }



    //search in only the given grid, (1 search)
    public List<Creature_V2> ExistCreatureAtTile(int x, int y)
    {
        if (IsValidLocation(x, y) == true)
        {
            return tiles[y, x].creatureListOnTile;
        }
        return null;
    }

    public string TileToString(int x, int y)
    {
        return x + "," + y + "\nE: " + String.Format("{0:###.00}", tiles[y, x].currentEnergy*100f) + "\nC: " + String.Format("{0:###.00}", climate + "\nT: "+tiles[y,x].type);
    }

    public bool IsSelected(int x, int y)
    {
        return tiles[y, x].selected;
    }

    public void SetSelected(int x, int y)
    {
        if(IsValidLocation(x,y) == true)
            tiles[y, x].selected = true ;
    }

    public void RemoveSelected(int x, int y)
    {
        if (IsValidLocation(x, y) == true)
            tiles[y, x].selected = false;
    }

    public void DeleteAllBodiesOnSelected()
    {
        for (int x = 0; x < sizeY; x++)
        {
            for (int y = 0; y < sizeX; y++)
            {
                if (tiles[y, x].selected == true)
                {
                    tiles[y, x].selected = false;

                    for (int i = 0; i < tiles[y, x].numberOfCreatures; i++)
                    {
                        tiles[y, x].creatureListOnTile[i].KillWithEnergy();
                        //tiles[y, x].creatureListOnTile.RemoveAt(i);
                    }
                }
            }
        }
    }

    public List<Creature_V2> GetAllBodiesOnSelected()
    {
        List<Creature_V2> selectedCreatures = new List<Creature_V2>();
        for (int x = 0; x < sizeY; x++)
        {
            for (int y = 0; y < sizeX; y++)
            {
                if (tiles[y, x].selected == true)
                {
                    tiles[y, x].selected = false;
                    for (int i = 0; i < tiles[y, x].numberOfCreatures; i++)
                    {
                        selectedCreatures.Add(tiles[y, x].creatureListOnTile[i]);
                    }
                }
            }
        }
        return selectedCreatures;
    }

    public void AddEnergyToTile(int x, int y, float energy)
    {
        if (IsValidLocation(x, y) == true && tiles[y,x].type == Tile_V2.TILE_FERT)
        {
            tiles[y, x].currentEnergy += energy;
            if (tiles[y, x].currentEnergy > 1f)
                tiles[y, x].currentEnergy = 1f;
        }
    }

    public Tile_V2[,] GetTilesArray()
    {
        return tiles;
    }

    public void SetCurrentEnergy(float[,] currentEnergyArray )
    {
        Debug.Log("Setting Initial Energy Array From File ");
        for (int i = 0; i < 100; i++)
        {
            for (int j = 0; j <100; j++)
            {
                if (tiles[i, j].type == Tile_V2.TILE_FERT)
                {
                    tiles[i, j].currentEnergy = currentEnergyArray[i, j];
                    tiles[i, j].lastUpdated = WolrdManager_V2.WORLD_CLOCK;
                }
            }
        }
    }

    public Color GetColorBetweenTwoColor(float value, Color c1, Color c2)
    {
        return new Color((((c2.r - c1.r) * value) + c1.r), (((c2.g - c1.g) * value) + c1.g), (((c2.b - c1.b) * value) + c1.b));
    }

    public Color HeatMapColor(float it)
    {
        if (heat == true)
        {
            //Heated Metal
            if (it >= 0 && it < 0.4f)
            {
                Color low = Color.black;
                Color high = new Color(128f / 255f, 0, 128f / 255f);
                return GetColorBetweenTwoColor((it) / 0.4f, low, high);
            }
            else if (it >= 0.4 && it < 0.6f)
            {
                Color low = new Color(128f / 255f, 0, 128f / 255f);
                Color high = Color.red;
                return GetColorBetweenTwoColor((0.2f - (0.6f - it)) / 0.2f, low, high);
            }
            else if (it >= 0.6 && it < 0.75)
            {
                Color low = Color.red;
                Color high = Color.yellow;
                return GetColorBetweenTwoColor((0.15f - (0.75f - it)) / 0.15f, low, high);
            }
            else if (it >= 0.75f && it <= 1f)
            {
                Color low = Color.yellow;
                Color high = Color.white;
                return GetColorBetweenTwoColor((0.25f - (1f - it)) / 0.25f, low, high);
            }
        }
        else
        {
            //Incandescent
            if (it >= 0 && it < 0.33f)
            {
                Color low = Color.black;
                Color high = new Color(139f / 255f, 0f, 0f);
                return GetColorBetweenTwoColor(it / 0.33f, low, high);
            }
            else if (it >= 0.33 && it < 0.66f)
            {
                Color low = new Color(139f / 255f, 0f, 0f);
                Color high = Color.yellow;
                return GetColorBetweenTwoColor((0.33f - (0.66f - it)) / 0.33f, low, high);
            }
            else if (it >= 0.66f && it <= 1f)
            {
                Color low = Color.yellow;
                Color high = Color.white;
                return GetColorBetweenTwoColor((0.33f - (1f - it)) / 0.33f, low, high);
            }
        }




        return Color.black;
    }

    public void ToggleHeatType() {
        heat = !heat;
    }

    public void ToggleDrawHeat()
    {
        drawHeat = !drawHeat;
    }

    public void SetClimate(float climate)
    {
        this.climate = climate;
    }

    public void SetWorldDeltaTime(float worldDeltaTime)
    {
        this.worldDeltaTime = worldDeltaTime;
    }

    public void SetNutritionPower(int power)
    {
        this.nutritionPower = power;
    }

    public void SetNutritionAmplitude(float amplitude)
    {
        this.nutritionAmplitude = amplitude;
    }

    public void NukeTiles(int nukeStrength, int x, int y)
    {
        if (IsValidLocation(x, y))
        {
            for(int i = y-nukeStrength; i <= y+nukeStrength; i++)
            {
                for (int j = x - nukeStrength; j <= x + nukeStrength; j++)
                {
                    if (IsValidLocation(j, i) == true)
                    {
                        

                        if (Mathf.Sqrt(Mathf.Pow(j-x,2f) + Mathf.Pow(i - y, 2f)) <= nukeStrength)
                        {
                            if (tiles[i, j].type == Tile_V2.TILE_FERT)
                            {
                                tiles[i, j].currentEnergy = 0f;
                                tiles[i, j].lastUpdated = WolrdManager_V2.WORLD_CLOCK;
                               
                            }

                            List<Creature_V2> creatures = tiles[i, j].creatureListOnTile;
                            for (int k = 0; k < creatures.Count; k++)
                            {
                                creatures[k].AffectEnergy(-25f);
                                creatures[k].AffectLife(-0.75f);

                                
                            }
                        }
                    }
                }
            }
        }
    }
}
