using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Grid
{
    public static bool started = false;
    private static Piece[,] grid;
    private static List<Piece> pieces = new List<Piece>();
    private static int lowX = 0;
    private static int lowY = 0;
    private static int highX = 0;
    private static int highY = 0;
    public static void RegisterPiece(Piece piece) // call on start of piece
    {
        pieces.Add(piece);
        foreach (Transform child in piece.gameObject.transform) // loop through each piece-block.
        {
            Vector3 pos = child.transform.position;
            // registering the lowest + highest x and y position of the pieces, to know how big the grid should be.
            if (pos.x < lowX)
            {
                lowX = Mathf.RoundToInt(pos.x);
            }
            else if (pos.x > highX)
            {
                highX = Mathf.RoundToInt(pos.x) + 1;
            }
            if (pos.y < lowY)
            {
                lowY = Mathf.RoundToInt(pos.y);
            }
            else if (pos.y > highY)
            {
                highY = Mathf.RoundToInt(pos.y) + 1;
            }
        }
    }
    public static void CreateCrid() // call after some interval
    {
        int gridWidth = -lowX + highX;
        int gridHeight = -lowY + highY;
        grid = new Piece[gridWidth, gridHeight];
        pieces.ForEach(delegate (Piece piece) // loop through all pieces, similar to a for loop
        {
            foreach (Transform child in piece.gameObject.transform) // loop through piece-childs (which is every block)
            {
                Vector3 pos = child.gameObject.transform.position;
                int x = Mathf.RoundToInt(pos.x) - lowX;
                int y = Mathf.RoundToInt(pos.y) - lowY;
                grid[x, y] = piece; // register piece to every block position
            }
        });
        started = true;
    }
    // x and y in worldposition! Will be converted to grid position
    public static Piece GetPiece(int x, int y)
    {
        if (x >= highX || y >= highY || x < lowX || y < lowY) { return null;  } // out of grid boundary, so no game pieces there
        return grid[x - lowX, y - lowY]; // lowX is the 0-position of the grid, so this is substracted to get the grid x/y position. (if low is -30 and x = -30, you get -30--30 = 0). 
    }
}

public class Piece : MonoBehaviour {
    public Color color = new Color(1, 1, 1);
    public int order = 0;
    public string pTag = ""; // if no tag is set, only 1 piece fits here. If tag = set, any piece with the same tag fits.
    [HideInInspector] public bool colorized = true;
    private SpriteRenderer[] childRenderers;
    // Use this for initialization
    void Start ()
    {
        childRenderers = GetComponentsInChildren<SpriteRenderer>();
        MakeGray();
        if (transform.parent && transform.parent.tag == "Level") // register piece if we have a parent and the parent has the "Level" tag,
        {                                                        // used so we don't register pieces controlled by the player
            Grid.RegisterPiece(this);
        }
    }

    private void OnValidate() // called in edit-mode, to set color of child-pieces when color has changed
    {
        Colorize();
    }

    public void MakeGray()
    {
        colorized = false;
        float grayScale = color.grayscale;
        for (int i = 0; i < childRenderers.Length; i++)
        {
            SpriteRenderer childRenderer = childRenderers[i];
            childRenderer.color = new Color(grayScale, grayScale, grayScale); 
        }
    }

    public void Colorize()
    {
        colorized = true;
        if (childRenderers == null) {
            return;
        }
        for (int i = 0; i < childRenderers.Length; i++)
        {
            SpriteRenderer childRenderer = childRenderers[i];
            childRenderer.color = new Color(color.r, color.g, color.b);
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
