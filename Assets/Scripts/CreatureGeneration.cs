using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureGeneration : MonoBehaviour
{
    private GameObject animals;

    public void Awake()
    {
        animals = new GameObject("Animals");
        animals.transform.parent = GameObject.Find("World").transform;
    }

    public void GenerationCreatures(TileData tileData)
    {
        var position = tileData.terrain.GetPosition();
        var terrainData = tileData.terrain.terrainData;
        var size = terrainData.size;
        int x = (int)Random.Range(0f, size.x);
        int z = (int)Random.Range(0f, size.z);
        float y = terrainData.GetHeight(x, z);

        if (tileData.chosenHeightTerrainTypes[z,x].name.Equals("ground"))
        {
            var rotation = Quaternion.identity;
            for (int i = 0; i < Random.Range(0, 6); i++)
            {
                rotation.eulerAngles = new Vector3(0, Random.Range(0, 360), 0);
                var animal = Instantiate(Resources.Load("AI/Chicken_Bot") as GameObject, new Vector3(position.x + x, y, position.z + z), rotation);
                animal.transform.parent = animals.transform;
            }
        }
    } 
}
