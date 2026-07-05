using System.Collections.Generic;
using UnityEngine;

public class QuadTree : MonoBehaviour
{
    // Boundaries
    public Rectangle boundary;
    // Capacity
    public int _maxCapacity;
    // List of circles
    public List<Circle> _circlesList = new();
    // List of sub-quadtrees
    public List<QuadTree> _subQuadTrees = new();
    // Subdivide flag
    private bool _isSubdivided = false;

    public void Initiate(Rectangle rectangle, int capacity)
    {
        boundary = rectangle;
        _maxCapacity = capacity;
        _circlesList = new();
        _subQuadTrees = new();
    }

    public bool Insert(GameObject gameObject)
    {
        Circle circle = gameObject.GetComponent<Circle>();
        if (circle != null)
        {
            return Insert(circle);
        }
        Debug.Log("GameObject does not have a Circle component.");
        return false;
    }

    /// <summary>
    /// Inserts a circle into the quadtree. If the circle is out of bounds, it will not be added. 
    /// If the quadrant has reached its capacity and is not yet subdivided, it will be subdivided 
    /// and the existing circles will be redistributed into the appropriate sub-quadrants. 
    /// The new circle will then be inserted into the appropriate sub-quadrant.
    /// </summary>
    /// <param name="circle">The circle to insert.</param>
    /// <returns>True if the circle was successfully inserted, false otherwise.</returns>
    public bool Insert(Circle circle)
    {
        // Check if the circle is within the bounds of this quadrant
        if (!this.boundary.Contains(circle.transform))
        {
            return false; // Circle is out of bounds
        }

        // Check if quadrant has reached capacity and is not yet subdivided
        if (_circlesList.Count < _maxCapacity && !_isSubdivided)
        {
            // If not, add the circle to the list
            _circlesList.Add(circle);
            return true; // Circle added successfully
        }
        else // If capacity is reached, subdivide and add the circle to the appropriate sub-quadrant
        {
            // If not yet Subdivided, Subdivide and redistribute existing circles
            if (!_isSubdivided)
            {
                Subdivide();

                // Redistribute existing circles into the appropriate sub-quadrants
                foreach (var existingCircle in _circlesList)
                {
                    foreach (var subQuadTree in _subQuadTrees)
                    {
                        if (subQuadTree.Insert(existingCircle))
                        {
                            break; // Existing circle inserted into a sub-quadrant
                        }
                    }
                }
                _circlesList.Clear(); // Clear the list after redistributing circles
            }

            foreach (var subQuadTree in _subQuadTrees)
            {
                if (subQuadTree.Insert(circle))
                {
                    return true; // Circle inserted into a sub-quadrant
                }
            }
            return true; // Quadtree subdivided and circle inserted
        }
    }

    public void Subdivide()
    {
        // Create four child quadrants
        // A B
        // C D
        for (int i = 0; i < 4; i++)
        {
            GameObject newGameObject = new GameObject("QuadTree_" + i);
            newGameObject.transform.parent = this.transform;

            QuadTree newTree = newGameObject.AddComponent<QuadTree>();
            Rectangle rectangle = new Rectangle(boundary.X + (i % 2 == 0 ? -boundary.Width / 4 : boundary.Width / 4),
                                                boundary.Y + (i < 2 ? boundary.Height / 4 : -boundary.Height / 4),
                                                boundary.Width / 2,
                                                boundary.Height / 2);

            newTree.Initiate(rectangle, _maxCapacity);
            _subQuadTrees.Add(newTree);
        }
        _isSubdivided = true;
    }

    public List<Circle> Query(Rectangle otherBoundary)
    {
        List<Circle> foundCircles = new List<Circle>();

        if (!this.boundary.Intersects(otherBoundary)) // if boundary does not intersect, return an empty list
        {
            return foundCircles;
        }
        else if (_isSubdivided) // if subdivided, add all the circles from the sub trees
        {
            foreach (QuadTree subTree in _subQuadTrees)
            {
                foundCircles.AddRange(subTree.Query(otherBoundary));
            }
        }
        else // if not subdivided, add all the circles from this tree
        {
            foreach (Circle circle in _circlesList)
            {
                foundCircles.Add(circle);
            }
        }

        return foundCircles;
    }

    public List<GameObject> QueryGameObject(Rectangle otherBoundary)
    {
        List<GameObject> gameObjects = new List<GameObject>();
        List<Circle> circles = Query(otherBoundary);

        foreach (Circle circle in circles)
        {
            gameObjects.Add(circle.gameObject);
        }

        return gameObjects;
    }

    //public void Clear()
    //{
    //    _circlesList.Clear();
    //    if (_isSubdivided)
    //    {
    //        foreach (var subQuadTree in _subQuadTrees)
    //        {
    //            subQuadTree.Clear();
    //            Destroy(subQuadTree.gameObject); // Destroy the child GameObject
    //        }
    //        _subQuadTrees.Clear();
    //        _isSubdivided = false;
    //    }
    //}

    private void OnDrawGizmos()
    {
        // Draw the boundaries of the quadrant
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(boundary.X - boundary.Width / 2, boundary.Y - boundary.Height / 2, 0), new Vector3(boundary.X + boundary.Width / 2, boundary.Y - boundary.Height / 2, 0));
        Gizmos.DrawLine(new Vector3(boundary.X + boundary.Width / 2, boundary.Y - boundary.Height / 2, 0), new Vector3(boundary.X + boundary.Width / 2, boundary.Y + boundary.Height / 2, 0));
        Gizmos.DrawLine(new Vector3(boundary.X + boundary.Width / 2, boundary.Y + boundary.Height / 2, 0), new Vector3(boundary.X - boundary.Width / 2, boundary.Y + boundary.Height / 2, 0));
        Gizmos.DrawLine(new Vector3(boundary.X - boundary.Width / 2, boundary.Y + boundary.Height / 2, 0), new Vector3(boundary.X - boundary.Width / 2, boundary.Y - boundary.Height / 2, 0));
    }
}