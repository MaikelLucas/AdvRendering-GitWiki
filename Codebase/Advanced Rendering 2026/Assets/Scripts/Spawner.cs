using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(100)] // Ensures Spawner runs after the Circle components
public class Spawner : MonoBehaviour
{
    [Header("Optimization Settings")]
    public OptimizationMethod optimizationMethod;

    [Header("Prefab Settings")]
    public bool spawnEvenlyDistributed = true;
    public GameObject prefabToSpawn;
    [Min(0)] public int numberOfPrefabs = 1;
    public float prefabSpeed = 1f;
    public float prefabSize = 0.1f;
    public float worldSize = 10f;

    [Header("Spatial Partitioning Settings")]
    public float gridSize = 0.1f;
    public int gridWidth, gridHeight;

    [Header("QuadTree Settings")]
    public QuadTree quadTreePrefab;
    [HideInInspector] public QuadTree currentQuadTree;
    [HideInInspector] public QuadTree2 quadTree2;
    public Rectangle quadTreeBoundary = new Rectangle(0f, 0f, 10f, 10f);
    public int maxCapacity = 4;

    [Header("Lists")]
    public List<GameObject> spawnedPrefabs = new();
    public List<List<List<GameObject>>> spatialGrid = new();

    public enum OptimizationMethod
    {
        None,
        SpatialPartitioning,
        QuadTree,
        QuadTree2
    }

    private void Start()
    {
        // Initialize spatial grid if enabled
        if (optimizationMethod == OptimizationMethod.SpatialPartitioning)
        {
            gridSize = prefabSize;
            gridWidth = Mathf.CeilToInt(worldSize / gridSize) + 2;
            gridHeight = Mathf.CeilToInt(worldSize / gridSize) + 2;

            for (int i = 0; i < gridWidth; i++)
            {
                spatialGrid.Add(new List<List<GameObject>>(gridHeight));
                for (int j = 0; j < gridHeight; j++)
                {
                    spatialGrid[i].Add(new List<GameObject>());
                }
            }
        }

        SpawnPrefabs();
    }

    private void Update()
    {
        if (optimizationMethod == OptimizationMethod.QuadTree)
        {
            // Rebuild QuadTree each frame
            if (currentQuadTree != null)
            {
                Destroy(currentQuadTree.gameObject);
            }

            currentQuadTree = Instantiate(quadTreePrefab).GetComponent<QuadTree>();
            currentQuadTree.Initiate(quadTreeBoundary, maxCapacity);

            foreach (GameObject prefab in spawnedPrefabs)
            {
                currentQuadTree.Insert(prefab);
            }
        }
        else if (optimizationMethod == OptimizationMethod.QuadTree2)
        {
            // Rebuild QuadTree2 each frame
            quadTree2 = new QuadTree2(quadTreeBoundary, maxCapacity);

            foreach (GameObject prefab in spawnedPrefabs)
            {
                quadTree2.Insert(prefab);
            }
        }
    }

    private void LateUpdate()
    {
        // Clear spatial grid each frame if enabled
        // Because of [DefaultExecutionOrder(100)], this will happen AFTER all Circles have checked collisions
        if (optimizationMethod == OptimizationMethod.SpatialPartitioning)
        {
            for (int i = 0; i < gridWidth; i++)
            {
                for (int j = 0; j < gridHeight; j++)
                {
                    spatialGrid[i][j].Clear();
                }
            }
        }
    }

    private void SpawnPrefabs()
    {
        for (int i = 0; i < numberOfPrefabs; i++)
        {
            Vector3 spawnPosition;
            if (spawnEvenlyDistributed)
            {
                spawnPosition = new(Random.Range(-worldSize / 2f, worldSize / 2f), Random.Range(-worldSize / 2f, worldSize / 2f), 0f);
            }
            else
            {
                spawnPosition = new(Random.Range(-worldSize / 10f, worldSize / 10f), Random.Range(-worldSize / 10f, worldSize / 10f), 0f);
            }
            Circle spawnedPrefab = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity).GetComponent<Circle>();
            spawnedPrefab.speed = prefabSpeed;
            spawnedPrefab.size = prefabSize;
            spawnedPrefab.spawner = this;
            spawnedPrefab.worldSize = worldSize;

            if (optimizationMethod == OptimizationMethod.SpatialPartitioning)
            {
                // Shift indices by +1 to leave room for the extra boundary cell on the lower end
                int gridX = (int)((spawnPosition.x + worldSize / 2f) / gridSize) + 1;
                int gridY = (int)((spawnPosition.y + worldSize / 2f) / gridSize) + 1;
                spatialGrid[gridX][gridY].Add(spawnedPrefab.gameObject);
            }
            else if (optimizationMethod == OptimizationMethod.QuadTree)
            {
                spawnedPrefabs.Add(spawnedPrefab.gameObject);
            }
            else if (optimizationMethod == OptimizationMethod.QuadTree2)
            {
                spawnedPrefabs.Add(spawnedPrefab.gameObject);
            }
            else
            {
                spawnedPrefabs.Add(spawnedPrefab.gameObject);
            }
        }
    }

    //private void OnDrawGizmos()
    //{
    //    if (optimizationMethod == OptimizationMethod.QuadTree2)
    //    {
    //        QuadTree2 quadTree = quadTree2;
    //        // Draw the boundaries of the quadrant
    //        Gizmos.color = Color.green;
    //        Gizmos.DrawLine(new Vector3(quadTree._x - quadTree._width / 2, quadTree._y - quadTree._height / 2, 0), new Vector3(quadTree._x + quadTree._width / 2, quadTree._y - quadTree._height / 2, 0));
    //        Gizmos.DrawLine(new Vector3(quadTree._x + quadTree._width / 2, quadTree._y - quadTree._height / 2, 0), new Vector3(quadTree._x + quadTree._width / 2, quadTree._y + quadTree._height / 2, 0));
    //        Gizmos.DrawLine(new Vector3(quadTree._x + quadTree._width / 2, quadTree._y + quadTree._height / 2, 0), new Vector3(quadTree._x - quadTree._width / 2, quadTree._y + quadTree._height / 2, 0));
    //        Gizmos.DrawLine(new Vector3(quadTree._x - quadTree._width / 2, quadTree._y + quadTree._height / 2, 0), new Vector3(quadTree._x - quadTree._width / 2, quadTree._y - quadTree._height / 2, 0));
    //        if (quadTree2._isSubdivided)
    //        {
    //            foreach (var subQuadTree in quadTree2._subQuadTrees)
    //            {
    //                // Draw the boundaries of the quadrant
    //                Gizmos.color = Color.green;
    //                Gizmos.DrawLine(new Vector3(subQuadTree._x - subQuadTree._width / 2, subQuadTree._y - subQuadTree._height / 2, 0), new Vector3(subQuadTree._x + subQuadTree._width / 2, subQuadTree._y - subQuadTree._height / 2, 0));
    //                Gizmos.DrawLine(new Vector3(subQuadTree._x + subQuadTree._width / 2, subQuadTree._y - subQuadTree._height / 2, 0), new Vector3(subQuadTree._x + subQuadTree._width / 2, subQuadTree._y + subQuadTree._height / 2, 0));
    //                Gizmos.DrawLine(new Vector3(subQuadTree._x + subQuadTree._width / 2, subQuadTree._y + subQuadTree._height / 2, 0), new Vector3(subQuadTree._x - subQuadTree._width / 2, subQuadTree._y + subQuadTree._height / 2, 0));
    //                Gizmos.DrawLine(new Vector3(subQuadTree._x - subQuadTree._width / 2, subQuadTree._y + subQuadTree._height / 2, 0), new Vector3(subQuadTree._x - subQuadTree._width / 2, subQuadTree._y - subQuadTree._height / 2, 0));
    //            }
    //        }
    //    }
    //}
}