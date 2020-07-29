using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseMapGeneration : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public float frequency;
        public float amplitude;
    }

    /*
     * Generate a noisemap using perlin noise
     * <param name="mapSize">Size of the terrain</param>
     * <param name="offsetX">Terrain position X</param>
     * <param name="offsetZ">Terrain position Z</param>
     * <param name="seed">Allow to create different world with different seed</param>
     * <param name="waves">Influence how the noisemap is created</param>
     */
    public float[,] GeneratePerlinNoiseMap(int mapSize, float offsetX, float offsetZ, int seed, Wave[] waves)
    {
        var noiseMap = new float[mapSize, mapSize];

        float sampleX, sampleZ;
        for (int z = 0; z < mapSize; z++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                sampleX = (x + offsetX) / mapSize;
                sampleZ = (z + offsetZ) / mapSize;

                float noise = 0f;
                float normalization = 0f;
                foreach (Wave wave in waves)
                {
                    noise += wave.amplitude * Mathf.PerlinNoise(sampleX * wave.frequency + seed, sampleZ * wave.frequency + seed);
                    normalization += wave.amplitude;
                }
                noise /= normalization;
                noiseMap[z, x] = noise;
            }
        }
        return noiseMap;
    }

    [Obsolete("Used for Tile Generation which have been made obsoleted, using for reference")]
    public float[,] GeneratePerlinNoiseMap(int mapDepth, int mapWidth, float scale, float offsetX, float offsetZ, int seed, Wave[] waves)
    {
        var noiseMap = new float[mapDepth, mapWidth];

        float sampleX = 0;
        float sampleZ = 0;
        for (int z = 0; z < mapDepth; z++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                sampleX = (x + offsetX) / scale;
                sampleZ = (z + offsetZ) / scale;

                float noise = 0f;
                float normalization = 0f;
                noise = Mathf.PerlinNoise(sampleX, sampleZ);
                foreach (Wave wave in waves)
                {
                    noise += wave.amplitude * Mathf.PerlinNoise(sampleX * wave.frequency + seed, sampleZ * wave.frequency + seed);
                    normalization += wave.amplitude;
                }
                noise /= normalization;
                noiseMap[z, x]= noise;
            }
        }
        return noiseMap;
    }

    /*
     * Generate a noisemap using perlin noise
     * <param name="mapSize">Size of the terrain</param>
     * <param name="centerVertexZ">Center position of the noisemap</param>
     * <param name="maxDistanceZ">Max distance away from the center</param>
     * <param name="offsetZ">Terrain position Z away from origin</param>
     */
    public float[,] GenerateUniformNoiseMap(int mapDepth, int mapWidth, float centerVertexZ, float maxDistanceZ, float offsetZ)
    {
        var noiseMap = new float[mapDepth, mapWidth];

        for (int zIndex = 0; zIndex < mapDepth; zIndex++)
        {
            float sampleZ = zIndex + offsetZ;
            float noise = Mathf.Abs(sampleZ - centerVertexZ) / maxDistanceZ;
            for (int xIndex = 0; xIndex < mapWidth; xIndex++)
            {
                noiseMap[mapDepth - zIndex - 1, xIndex] = noise;
            }
        }
        return noiseMap;
    }
}
