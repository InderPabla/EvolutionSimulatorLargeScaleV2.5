
using System;
using System.Collections.Generic;
using UnityEngine;

public class Species : IEquatable<Species> {

    List<Creature_V2> population;
    Color color;
    //HashSet<int> populationID;
    int randomSpeciesNumber;

    public Species(Color color, int speciesNumber)
    {
        population = new List<Creature_V2>();
        this.color = color;
        this.randomSpeciesNumber = speciesNumber;
        //this.randomSpeciesNumber = UnityEngine.Random.Range(0,10000); //random number
        // populationID = new HashSet<int>();
    }

    public void Add(Creature_V2 creature)
    {
        population.Add(creature);
        
        //populationID.Add(creature.GetID());
    }

    public bool Remove(Creature_V2 creature)
    {

        int i = creature.GetID();
        int low = 0, high = population.Count - 1, mid;
        int index = -1;
        while (low <= high)
        {
            mid = (low + high) / 2;
            if (i < population[mid].GetID())
                high = mid - 1;

            else if (i > population[mid].GetID())
                low = mid + 1;

            else {
                index = mid;
                break;
            }
        }

        if (index != -1) {
            population.RemoveAt(index);
            return true;
        }


        return false;


       /* if (population.Remove(creature) == true)
        {
            //populationID.Remove(creature.GetID());
            return true;
        }

        return false;*/
    }

    

    public bool IsEmpty()
    {
        return population.Count == 0;
    }

    

    /*public bool HasID(int ID)
    {
        return populationID.Contains(ID);
    }*/

    public List<Creature_V2> GetPopulation()
    {
        return population;
    }

    public Color GetColor()
    {
        return color;
    }

    /*public HashSet<int> GetPopulationHashID()
    {
        return populationID;
    }*/

    /// <summary>
    /// MUAHAHHAHAHAHAH MUUUUHAHAHAHAHAHAHA okay okay clam down. Take a chill pill. 
    /// </summary>
    public void KillSpecies()
    {
        for (int i = 0; i < population.Count; i++)
        {
            population[i].KillWithEnergy();
        }
    }

    public bool Equals(Species other)
    {
        return other.randomSpeciesNumber == randomSpeciesNumber;
    }

    public int GetID() {
        return randomSpeciesNumber;
    }

    /*public void PrintID()
    {
        IEnumerator<int> e = populationID.GetEnumerator();
        string s = "";
        s += e.Current + " ";
        while (e..MoveNext() == true)
        {
            s += e.Current + " ";
        }
        Debug.Log(s);
    }*/
}
