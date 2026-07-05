using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]

public class Circle : MonoBehaviour
{
    // parameters
    public Vector3 direction;

    public float speed = 1f;
    public float size = 0.1f;
    public Spawner spawner;
    public float worldSize = 10f;

    // spatial partitioning parameters
    private int gridX, gridY;
    public bool debug = false;

    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        // assign random direction
        direction = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f).normalized;
        transform.localScale = new Vector3(size, size, size);
    }

    private void Update()
    {
        Move();
        Loop();
        
        // reset color and fill grid
        _spriteRenderer.color = Color.white;

        if (spawner.optimizationMethod == Spawner.OptimizationMethod.SpatialPartitioning)
        {
            // calculate grid position
            gridX = (int)((transform.position.x + worldSize / 2f) / spawner.gridSize) + 1;
            gridY = (int)((transform.position.y + worldSize / 2f) / spawner.gridSize) + 1;
            spawner.spatialGrid[gridX][gridY].Add(this.gameObject);
            
            if (debug)
            {
                Debug.Log($"Circle at {transform.position} in grid cell ({gridX}, {gridY})");
            }
        }
    }

    private void LateUpdate()
    {
        // check for collisions after all circles have been added to the grid
        if (spawner.optimizationMethod == Spawner.OptimizationMethod.SpatialPartitioning)
        {
            List<GameObject> adjacentGridCells = new();

            // collision check with nearby circles in the same and adjacent grid cells
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    int checkX = (gridX + i + spawner.gridWidth) % spawner.gridWidth;
                    int checkY = (gridY + j + spawner.gridHeight) % spawner.gridHeight;
                    adjacentGridCells.AddRange(spawner.spatialGrid[checkX][checkY]);
                }
            }
            CollisionCheck(adjacentGridCells);
        }
        else if (spawner.optimizationMethod == Spawner.OptimizationMethod.QuadTree)
        {
            // collision check with nearby circles in the quadtree
            List<GameObject> nearbyCircles = spawner.currentQuadTree.QueryGameObject(new Rectangle(transform.position.x, transform.position.y, size * 2, size * 2));
            CollisionCheck(nearbyCircles);
        }
        else if (spawner.optimizationMethod == Spawner.OptimizationMethod.QuadTree2)
        {
            // collision check with nearby circles in the quadtree
            List<GameObject> nearbyCircles = spawner.quadTree2.QueryGameObject(new Rectangle(transform.position.x, transform.position.y, size * 2, size * 2));
            CollisionCheck(nearbyCircles);
        }
        else
        {
            CollisionCheck(spawner.spawnedPrefabs);
        }
    }

    private void Move()
    {
        // move the circle
        transform.position += speed * Time.deltaTime * direction;
    }

    private void Loop()
    {
        // loop the circles at the edge
        if (transform.position.x > worldSize / 2f + size) transform.position = new Vector3(transform.position.x - worldSize - 2 * size, transform.position.y, 0f);
        else if (transform.position.x < -worldSize / 2f - size) transform.position = new Vector3(transform.position.x + worldSize + 2 * size, transform.position.y, 0f);
        if (transform.position.y > worldSize / 2f + size) transform.position = new Vector3(transform.position.x, transform.position.y - worldSize - 2 * size, 0f);
        else if (transform.position.y < -worldSize / 2f - size) transform.position = new Vector3(transform.position.x, transform.position.y + worldSize + 2 * size, 0f);
    }

    private void CollisionCheck(List<GameObject> list)
    {
        // collision check with other circles
        foreach (GameObject other in list)
        {
            if (other != this.gameObject && Vector3.Distance(transform.position, other.transform.position) < size)
            {
                _spriteRenderer.color = Color.red;
            }
        }
    }
}