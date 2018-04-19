using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UI;

/// <summary>
/// Board controller - manages the game logic and the overall board state & display
/// </summary>
public class BoardController : SingletonMonoBehaviour<BoardController>
{

    public int width = 5;
    public int height = 5;

    public GameObject[] dotPrefabs = null;

    public LineRenderer currentPathDisplay = null;

    private Dot[,] dotsCollection;

    void Start()
    {
        Setup();
    }

    void Update()
    {

    }

    void Setup()
    {
        dotsCollection = new Dot[width, height];
        for (int col = 0; col < width; col++)
        {
            for (int row = 0; row < height; row++)
            {
                dotsCollection[row, col] = CreateDot(col, row);
            }
        }

        currentPathDisplay.enabled = false;
        currentPathDisplay.positionCount = 0;
    }

    private string GenerateDotName(int col, int row)
    {
        return string.Format("Dot ({0},{1})", col, row);
    }

    private Dot CreateDot(int col, int row)
    {
        // select a type of dot at random from our dot prefabs
        int typeOfDot = UnityEngine.Random.Range(0, dotPrefabs.Length);
        Vector2 tmpPos = new Vector2(col, row);
        GameObject newDot = Instantiate<GameObject>(dotPrefabs[typeOfDot], tmpPos, Quaternion.identity);
        newDot.gameObject.name = GenerateDotName(col, row);
        newDot.transform.parent = this.transform;
        Dot dotCtl = newDot.GetComponent<Dot>();
        // setup the dot properties
        dotCtl.Setup(typeOfDot, row, col);
        return dotCtl;
    }

    public void UpdateDotSelection(Dot dot, List<Dot> currentDotPath)
    {
        if (currentDotPath.Count == 0)
        {
            // highlight a new dot
            dot.Activate();
            currentDotPath.Add(dot);
        }
        else
        {
            // highlight a new dot on the path if the dot is selectable according to the game rules
            Dot lastDot = currentDotPath[currentDotPath.Count - 1];
            int lastDotX = lastDot.Column;
            int lastDotY = lastDot.Row;
            int newDotX = dot.Column;
            int newDotY = dot.Row;
            if (
                lastDot != dot &&                                   // not same as last dot
                lastDot.DotType == dot.DotType &&                   // same type of dot
                !currentDotPath.Contains(dot) &&                    // not already in current path
                Mathf.Abs(newDotX - lastDotX) <= 1 &&               // only one step away on the board horizontally
                Mathf.Abs(newDotY - lastDotY) <= 1 &&               // only one step away on the board vertically
                (newDotX == lastDotX || newDotY == lastDotY)        // only horizontal/vertical dot connections are allowed
               )
            {
                dot.Activate();
                currentDotPath.Add(dot);
            }
        }

        // display the path as connected lines
        currentPathDisplay.positionCount = currentDotPath.Count;
        currentPathDisplay.enabled = currentPathDisplay.positionCount > 0;
        currentPathDisplay.SetPositions(currentDotPath.Select(d => d.transform.position).ToArray());
    }

    public void FinalizeDotSelection(List<Dot> currentDotPath)
    {
        if (currentDotPath.Count > 1)
        {
            bool hasTransitioningDots = false;
            foreach (var dot in currentDotPath.OrderBy(d => d.Row))
            {
                // remove each cleared dot from the board's dot collection
                dotsCollection[dot.Row, dot.Column] = null;
                for (int i = dot.Row; i >= 0; i--)
                {
                    // each dot below the cleared dot will be shifted into the cleared up space
                    var adjcantDot = dotsCollection[i, dot.Column];
                    if (adjcantDot && !currentDotPath.Contains(adjcantDot))
                    {
                        dotsCollection[adjcantDot.Row, adjcantDot.Column] = null;
                        adjcantDot.Row += 1;
                        dotsCollection[adjcantDot.Row, adjcantDot.Column] = adjcantDot;
                        adjcantDot.gameObject.name = GenerateDotName(adjcantDot.Column, adjcantDot.Row);

                        // invoke the shift movement as a neat smooth transition for each moved dot
                        StartCoroutine(UIHelpers.TweenPosition(adjcantDot.transform, new Vector2(adjcantDot.Column, adjcantDot.Row), 0.3f));
                        hasTransitioningDots = true;
                    }
                }
            }
            // create new dots _after_ all current dots have shifted into the cleared spaces
            StartCoroutine(CreateAndShowNewDots(hasTransitioningDots ? 0.3f : 0f));

            // destroy all the cleared up dots
            currentDotPath.ForEach(d => Destroy(d.gameObject));
        }
        else
        {
            // in case there was only one dot on the path, just deactivate the highlighting
            currentDotPath.ForEach(d => d.Deactivate());
        }

        currentPathDisplay.enabled = false;
        currentPathDisplay.positionCount = 0;
        currentDotPath.Clear();
    }

    IEnumerator CreateAndShowNewDots(float delay)
    {
        yield return new WaitForSeconds(delay);
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                if (dotsCollection[row, col] == null)
                {
                    var newDot = CreateDot(col, row);
                    dotsCollection[row, col] = newDot;
                    // spawn and transition the new dot a bit off-screen
                    newDot.transform.position = new Vector3(newDot.Column, -width / 2);
                    StartCoroutine(UIHelpers.TweenPosition(newDot.transform, new Vector3(newDot.Column, newDot.Row), 0.2f));
                }
            }
        }
        yield return true;
    }
}
