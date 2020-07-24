using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGeneration : MonoBehaviour
{
    [SerializeField]
    private int mapWidthInTiles, mapDepthInTiles;

    [SerializeField]
    private GameObject tilePrefab;

    [SerializeField]
    private Camera camera;
    private Vector3 cameraPosition;
    private Bounds currentTile;

    private GameObject world;

    private LevelData levelData;

    // Start is called before the first frame update
    void Start()
    {
        world = new GameObject("World");
        camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        cameraPosition = camera.transform.position;

        var tileMeshVertices = tilePrefab.GetComponent<MeshFilter>().sharedMesh.vertices;
        int tileDepthInVertices = (int)Mathf.Sqrt(tileMeshVertices.Length);
        int tileWidthInVertices = tileDepthInVertices;
        levelData = new LevelData(tileDepthInVertices, tileWidthInVertices, 500, 500);

        UpdateMap();
    }

    private void Update()
    {
        var cameraCurrentPosition = camera.transform.position;
        if (!currentTile.Contains(cameraCurrentPosition))
            UpdateMap();
    }

    public void GenerateMap()
    {
        foreach (Transform child in world.transform)
            Destroy(child.gameObject);

        var tileSize = tilePrefab.GetComponent<MeshRenderer>().bounds.size;
        int tileWidth = (int)tileSize.x;
        int tileDepth = (int)tileSize.z;

        for (int zIndex = 0; zIndex < mapDepthInTiles; zIndex++)
        {
            for (int xIndex = 0; xIndex < mapWidthInTiles; xIndex++)
            {
                var tilePosition = new Vector3(this.gameObject.transform.position.x + xIndex * tileWidth,
                    this.gameObject.transform.position.y,
                    this.gameObject.transform.position.z + zIndex * tileDepth);
                var tile = Instantiate(tilePrefab, tilePosition, Quaternion.identity) as GameObject;
                tile.transform.parent = world.transform;
            }
        }
    }

    public void UpdateMap()
    {
        var tileSize = tilePrefab.GetComponent<MeshRenderer>().bounds.size;
        int tileWidth = (int)tileSize.x;
        int tileDepth = (int)tileSize.z;


        var currentCameraPosition = camera.transform.position;
        var x = Mathf.Floor(currentCameraPosition.x / tileWidth) * tileWidth;
        var z = Mathf.Floor(currentCameraPosition.z / tileDepth) * tileDepth;

        int minMax = (int)Mathf.Floor(mapWidthInTiles / 2);
        for (int zIndex = -minMax; zIndex <= minMax; zIndex++)
        {
            for (int xIndex = -minMax; xIndex <= minMax; xIndex++)
            {
                var tilePosition = new Vector3(x + xIndex * tileWidth, 0, z + zIndex * tileDepth);
                if (tilePosition.x >= 0 && tilePosition.z >= 0 && !CheckIfTileExist(tilePosition))
                {
                    var tile = Instantiate(tilePrefab, tilePosition, Quaternion.identity) as GameObject;
                    tile.transform.parent = world.transform;
                    TileData tileData = tile.GetComponent<TileGeneration>().GenerateTile(10, 500);
                    levelData.AddTileData(tileData, (int)tilePosition.z / tileDepth, (int)tilePosition.x / tileWidth);
                }
                currentTile = new Bounds(new Vector3(x + (tileWidth / 2), 0, z + (tileDepth / 2)), new Vector3(tileWidth, 1000, tileDepth));
            }
        }
    }

    private bool CheckIfTileExist(Vector3 tilePosition)
    {
        return Physics.CheckSphere(tilePosition, 4);
    }
}
