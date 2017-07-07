using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {
    public GameObject level;
    public float blockDroppingTime = 1f;
    public GameObject mostLeft;
    public GameObject mostRight;
    [HideInInspector] public int mostLeftInt;
    [HideInInspector] public int mostRightInt;
    protected Dictionary<int, List<GameObject>> spawnOrder = new Dictionary<int, List<GameObject>>(); // can be seen as a multidimensional array with some additional functionality
    protected int currentNum = 0;
    protected GameObject currentPiece = null;
    protected Arduino arduino;
	// Use this for initialization
	void Start () {
        CreateSpawnOrder();
        StartCoroutine(LateStart(1f));
        string[] joysticks = Input.GetJoystickNames();
        string controller = "keyboard";
        GameObject gArduino = GameObject.Find("Arduino");
        if (gArduino)
        {
            arduino = gArduino.GetComponent<Arduino>();
            controller = "arduino";
        }
        if (joysticks.Length > 0 && joysticks[0].Length > 0)
        {
            controller = "gamepad";
        }
        mostLeftInt = Mathf.RoundToInt(mostLeft.transform.position.x);
        mostRightInt = Mathf.RoundToInt(mostRight.transform.position.x);
        // TODO: Arduino, put in seperate class / config scene :p
        PlayerPrefs.SetString("control", controller);
    }

    IEnumerator LateStart(float waitTime)
    {
        yield return new WaitForSeconds(waitTime); // waiting a bit so all pieces are created
        Grid.CreateCrid();
    }
	
	// Update is called once per frame
	void Update () {
		if (!currentPiece)
        {
            GameObject newPiece = GetNextPiece();
            if (!newPiece)
            {
                Debug.Log("COMPLETE");
                return;
            }
            currentPiece = Object.Instantiate(newPiece);
            currentPiece.transform.position = transform.position;
            PlayerControl pc = currentPiece.AddComponent<PlayerControl>();
            pc.fittingPiece = newPiece.GetComponent<Piece>();
            pc.spawner = this;
        }
	}

    protected void CreateSpawnOrder()
    {
        // find and loop through all pieces
        Piece[] pieces = level.GetComponentsInChildren<Piece>();
        for (int i = 0; i < pieces.Length; i++)
        {
            Piece piece = pieces[i];
            int num = piece.order;
            if (!spawnOrder.ContainsKey(num)) // no other gameobject at this order exists yet, create a new list on that number
            {
                spawnOrder[num] = new List<GameObject>();
            }
            spawnOrder[num].Add(piece.gameObject); // add gameobject to the list
        }
    }

    protected GameObject GetNextPiece(int loop=0)
    {
        if (loop > 100) // recursive loop goes on for too long, let's stop here!
        {
            Debug.LogError("GetNextPiece recursion error");
            return null;
        }
        if (spawnOrder.Count == 0) // no more pieces to be found
        {
            return null;
        }
        if (!spawnOrder.ContainsKey(currentNum)) // current number (no longer) has a list, find the next one
        {
            currentNum++;
            return GetNextPiece(loop + 1);
        }
        List<GameObject> currentList = spawnOrder[currentNum];
        // taking piece at index 0 for now, we could change this into random...
        GameObject piece = currentList[0];
        currentList.RemoveAt(0);
        if (currentList.Count == 0) // no more items in current list, delete the list from the dictionary
        {
            spawnOrder.Remove(currentNum);
            currentNum++;
        }
        return piece;
    }
}
