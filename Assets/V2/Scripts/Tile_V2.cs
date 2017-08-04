using System.Collections.Generic;

public class Tile_V2
{

    public const int TILE_FERT = 0;
    public const int TILE_INFERT = 1;
    public const int TILE_WATER = -1;

    public int type;
    public HSBColor detail;
    public float maxEnergy;
    public float currentEnergy;

    public List<Creature_V2> creatureListOnTile = new List<Creature_V2>();

    public float lastUpdated = 0;
    public bool selected = false;

    public int numberOfCreatures = 0;
}