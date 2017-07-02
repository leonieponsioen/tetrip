using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour {

    public Piece fittingPiece;      // set by Spawner
    public Spawner spawner;         // set by Spawner
    private Piece controllingPiece;
    private string control;
    private float timer;            // timer used to countDown
    public float time;              // amount of time timer should countDown (retrieved from spawner (blockDroppingTime))
    private int mostLeft;       // used for arduino/xboxtrigger, retrieved from spawner
    private int mostRight;      // used for arduino/xboxtrigger, retrieved from spawner
    private int gridDiff;
	// Use this for initialization
	void Start () {
        control = PlayerPrefs.GetString("control");
        Debug.Log(control);
        controllingPiece = GetComponent<Piece>();
        controllingPiece.Colorize();
        timer = time;
        gameObject.AddComponent<MeshRenderer>(); // add meshrenderer to enable "OnBecameInvinsible" function on this gameobject
        mostLeft = spawner.mostLeftInt;
        mostRight = spawner.mostRightInt;
        gridDiff = mostRight - mostLeft;
        time = spawner.blockDroppingTime;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (!Grid.started) { return; } // wait for grid-start
        Vector3 position = transform.position;
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            if (CanMoveDown(position))
            {
                timer = time;
                position.y--;
            } else // can't move further, last collision check or RESET
            {
                if (!DetectCollision(position))
                {
                    Reset();
                }
                return;
            }
        }

        switch (control)
        {
            case "gamepad":
                position = XboxControl(position);
                break;
            default:
                position = KeyboardControl(position);
                break;
        }
        
        if (position != transform.position) { // if position has changed
            DetectCollision(position);
        }
        transform.position = position;
    }

    // acts on collision with fittingPiece, or a piece with the same "pTag" as fittingPiece
    private bool DetectCollision(Vector3 position) // TODO: Check world boundaries
    {
        Piece piece = null;
        foreach (Transform child in transform) // check collision for each child-block, every block should return the same piece for full collision
        {
            Piece collisionPiece = Grid.GetPiece(Mathf.RoundToInt(child.position.x), Mathf.RoundToInt(child.position.y));
            if (!collisionPiece) { return false; }
            if (!piece)
            {
                piece = collisionPiece;
            } else if (piece != collisionPiece)
            {
                Debug.Log("not the same piece", piece);
                if (collisionPiece)
                {
                    Debug.Log(piece);
                }
                Piece collisionP2iece = Grid.GetPiece(Mathf.RoundToInt(child.position.x), Mathf.RoundToInt(child.position.y));
                return false; // not the same piece, so no proper collision
            }
        }
        if (!piece) { return false;  } // no collision, so get out this function...
        // below here a collision with a piece has been found, now we check if it's a fitting one.
        if (piece == fittingPiece || (fittingPiece.pTag != "" && piece.pTag == fittingPiece.pTag))
        {
            Destroy(gameObject);
            piece.Colorize();
            return true;
        }
        return false;
    }

    private bool CanMoveDown(Vector3 position)
    {
        return CanMoveTo(position, 0, -1);
    }

    private bool CanMoveTo(Vector3 position, int xDiff, int yDiff=0)
    {
        foreach (Transform child in transform) // looping through all our child-blocks to get collision for each one of them
        {
            Piece nextCollisionPiece = Grid.GetPiece(Mathf.RoundToInt(position.x) + xDiff, Mathf.RoundToInt(position.y) + yDiff);
            if (nextCollisionPiece != null && nextCollisionPiece.colorized) { return false; } // if piece is found, and colorized, we can't move down
        }
        return true;
    }

    private void OnBecameInvisible()
    {
        Reset();
    }

    private void Reset()
    {
        transform.position = spawner.transform.position;
    }

    private Vector3 KeyboardControl(Vector3 position)
    {
        if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.LeftArrow))
        {
            if (CanMoveTo(position, -1)) position.x--;
        }
        if (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.RightArrow))
        {
            if (CanMoveTo(position, 1)) position.x++;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            if (CanMoveDown(position)) position.y--;
        }
        return position;
    }

    private Vector3 XboxControl(Vector3 position)
    {
        position = ToPosition(position, Mathf.Abs(Input.GetAxis("RightTrigger") * 100));
        return position;
    }

    private Vector3 ToPosition(Vector3 position, float sensorPosition) // sensorPosition = 0-100
    {
        int gridPosition = Mathf.RoundToInt(sensorPosition / 100 * gridDiff);
        position.x = gridPosition + mostLeft; // add mostLeft to convert to worldposition
        Debug.Log(gridPosition +" , "+ gridDiff);
        return position;
    }

    private void ArduinoSingleControl()
    {

    }

    private void ArduinoDoubleControl()
    {

    }
}
