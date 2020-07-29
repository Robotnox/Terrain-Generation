using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureGeneration : MonoBehaviour
{
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
            for (int i = 0; i < Random.Range(0, 6); i++)
                Instantiate(Resources.Load("AI/Chicken_Bot") as GameObject, new Vector3(position.x + x, y, position.z + z), Quaternion.identity);
        }
    } 
}
