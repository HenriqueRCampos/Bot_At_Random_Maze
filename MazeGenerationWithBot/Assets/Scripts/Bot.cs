using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Bot : MonoBehaviour
{
    [Range(0.05f, 1.5f)]
    [SerializeField] private float delayToMovement = 0.05f;
    [SerializeField] private GameObject completedTrackPath;
    [SerializeField] private GameObject exitTrackPath;

    private List<Vector3> pathWalked = new List<Vector3>();
    private List<Vector3> pathCompleted = new List<Vector3>();
    private List<Vector3> pathExit = new List<Vector3>();
    private List<GameObject> exitTrackInstances = new List<GameObject>();
    private List<Vector3> botAxisDirections = new List<Vector3>();
    private RaycastHit[] raycastHits = new RaycastHit[4];

    private MazeGenerator mazeGeneratorScript;
    private Vector3 movDirection;
    private Vector3 exitMazePosition;
    private Vector3 botSideToExitPosition;
    private bool afterCrossingPath;
    private bool isInExitPath;

    private void Start()    
    {
        botAxisDirections.Add(transform.TransformDirection(Vector3.forward));
        botAxisDirections.Add(transform.TransformDirection(Vector3.right));
        botAxisDirections.Add(transform.TransformDirection(Vector3.back));
        botAxisDirections.Add(transform.TransformDirection(Vector3.left));

        mazeGeneratorScript = GameObject.FindGameObjectWithTag("Generator").GetComponent<MazeGenerator>();
    }

    public void StartBot()
    {
        StartCoroutine(MoveBot());
        exitMazePosition = GameObject.FindGameObjectWithTag("Exit").transform.position;
    }

    private IEnumerator MoveBot()
    {
        yield return new WaitForSeconds(delayToMovement);
        Vector3 movDirection = DirectionRaycastHits();

        gameObject.transform.position = movDirection;
        yield return MoveBot();
    }

    private Vector3 DirectionRaycastHits()
    {
        List<Vector3> freeDirections = new List<Vector3>();
        CollidingObjects collidingObjects = new CollidingObjects();
        collidingObjects.wall = new List<Vector3>();
        collidingObjects.coin = new List<Vector3>();
        Vector3 direction;

        for (int i = 0; i < botAxisDirections.Count; i++)
        {
            if (!Physics.Raycast(transform.position, botAxisDirections[i], out raycastHits[i], 1))
            {
                freeDirections.Add(botAxisDirections[i]);
            }
            if (Physics.Raycast(transform.position, botAxisDirections[i], out raycastHits[i], 1))
            {
                switch (raycastHits[i].collider.tag)
                {
                    case "Wall":
                        collidingObjects.wall.Add(botAxisDirections[i]);
                        break;
                    case "Coin":
                        collidingObjects.coin.Add(botAxisDirections[i]);
                        break;
                    case "Exit":
                        collidingObjects.exit = botAxisDirections[i];
                        botSideToExitPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
                        break;
                }
            }
        }
        direction = DirectionToMove(freeDirections, collidingObjects);
        return direction;
    }

    private Vector3 DirectionToMove(List<Vector3> freeDirection, CollidingObjects collidingObjects)
    {
        if (collidingObjects.coin.Count > 0)
        {
            movDirection = new Vector3(transform.position.x, transform.position.y, transform.position.z) + collidingObjects.coin[0];
        }
        else if (collidingObjects.exit != Vector3.zero && mazeGeneratorScript.CoinsCount <= 0)
        {
            movDirection = new Vector3(transform.position.x, transform.position.y, transform.position.z) + collidingObjects.exit;
        }
        else if (botSideToExitPosition + collidingObjects.exit == exitMazePosition && mazeGeneratorScript.CoinsCount > 0)
        {
            movDirection = FindedExitButDoesntCollectedAllKeys(freeDirection);
        }
        else if (collidingObjects.wall.Count == 3)
        {
            movDirection = LockedBetweenWalls(freeDirection);
        }
        else if (freeDirection.Count == 2)
        {
            movDirection = FowardAndBackwardsDirections(freeDirection);
        }
        else if (freeDirection.Count == 3)
        {
            movDirection = CrossingPathDirections(freeDirection, 2, 3, 2);
        }
        else if (freeDirection.Count == 4)
        {
            movDirection = CrossingPathDirections(freeDirection, 3, 4, 3);
        }
        else
        {
            movDirection = new Vector3(transform.position.x, transform.position.y, transform.position.z) + freeDirection[0];
        }

        pathWalked.Add(movDirection);
        return movDirection;
    }

    private Vector3 FindedExitButDoesntCollectedAllKeys(List<Vector3> freeDirection)
    {
        Vector3 finalDirection = Vector3.zero;
        afterCrossingPath = true;
        isInExitPath = true;
        pathWalked.Clear();
        pathWalked.Add(transform.position);
        pathExit.Add(transform.position);
        CreatePathTrack(transform.position, exitTrackPath);

        for (int i = 0; i < freeDirection.Count; i++)
        {
            Vector3 direction = new Vector3(transform.position.x, transform.position.y, transform.position.z) + freeDirection[i];
            if (!pathCompleted.Contains(direction))
            {
                finalDirection = new Vector3(transform.position.x, transform.position.y, transform.position.z) + freeDirection[i];
            }
        }
        return finalDirection;
    }

    private Vector3 LockedBetweenWalls(List<Vector3> freeDirection)
    {
        Vector3 finalDirection = new Vector3(transform.position.x, transform.position.y, transform.position.z) + freeDirection[0];
        pathWalked.Clear();
        pathWalked.Add(transform.position);
        pathCompleted.Add(transform.position);
        CreatePathTrack(transform.position, completedTrackPath);

        afterCrossingPath = false;
        return finalDirection;
    }

    private Vector3 FowardAndBackwardsDirections(List<Vector3> freeDirection)
    {
        Vector3 finalDirection = Vector3.zero;
        for (int i = 0; i < freeDirection.Count; i++)
        {
            Vector3 nextPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z) + freeDirection[i];
            if (!pathWalked.Contains(nextPosition) && !pathCompleted.Contains(nextPosition))
            {
                finalDirection = nextPosition;
            }
            if (isInExitPath && mazeGeneratorScript.CoinsCount > 0)
            {
                pathExit.Add(transform.position);
                CreatePathTrack(transform.position, exitTrackPath);
            }
            if (!afterCrossingPath)
            {
                pathCompleted.Add(transform.position);
                CreatePathTrack(transform.position, completedTrackPath);
            }
        }
        return finalDirection;
    }

    private Vector3 CrossingPathDirections(List<Vector3> freeDirection, int blockeDirectionCompare, int blockedPathCompare, int completedQuantityCompare)
    {
        List<Vector3> currentFreeDirections = new List<Vector3>();
        Vector3 finalDirection = Vector3.zero;
        Vector3 exitDirection = Vector3.zero;

        int blockedPath = 0;
        int blockeDirection = 0;
        int completedQuantity = 0;
        int exitQuantity = 0;

        for (int i = 0; i < freeDirection.Count; i++)
        {
            Vector3 nextDirection = new Vector3(transform.position.x, transform.position.y, transform.position.z) + freeDirection[i];

            if (pathCompleted.Contains(nextDirection))
            {
                blockeDirection++;
            }
            if (blockeDirection >= blockeDirectionCompare)
            {
                afterCrossingPath = false;
                pathCompleted.Add(transform.position);
                CreatePathTrack(transform.position, completedTrackPath);
            }
            else
            {
                afterCrossingPath = true;
                isInExitPath = false;
            }

            if (!pathWalked.Contains(nextDirection) && !pathCompleted.Contains(nextDirection) && !pathExit.Contains(nextDirection))
            {
                currentFreeDirections.Add(nextDirection);
                finalDirection = currentFreeDirections[Random.Range(0, currentFreeDirections.Count)];
            }

            if (pathWalked.Contains(nextDirection) || pathCompleted.Contains(nextDirection) || pathExit.Contains(nextDirection))
            {
                if (pathCompleted.Contains(nextDirection))
                {
                    completedQuantity++;
                }
                else if (pathExit.Contains(nextDirection))
                {
                    exitQuantity++;
                    exitDirection = nextDirection;
                }
                blockedPath++;
            }
            if (blockedPath >= blockedPathCompare)
            {
                if (completedQuantity == completedQuantityCompare && exitQuantity == 1)
                {
                    pathExit.Clear();
                    finalDirection = exitDirection;
                    if (mazeGeneratorScript.CoinsCount <= 0)
                    {
                        foreach (GameObject obj in exitTrackInstances)
                        {
                            obj.SetActive(false);
                        }
                    }
                }
                else
                {
                    if (transform.position == pathWalked[pathWalked.Count - 1])
                    {
                        finalDirection = pathWalked[pathWalked.Count - 2];
                    }
                    else
                    {
                        finalDirection = pathWalked[pathWalked.Count - 1];
                    }

                    isInExitPath = true;
                    pathWalked.Clear();
                    pathWalked.Add(transform.position);
                    pathExit.Add(transform.position);
                    CreatePathTrack(transform.position, exitTrackPath);
                }
            }
            else
            {
                if (completedQuantity == completedQuantityCompare - 1 && exitQuantity == 1)
                {
                    isInExitPath = true;
                    pathExit.Add(transform.position);
                    CreatePathTrack(transform.position, exitTrackPath);
                }
            }
        }
        return finalDirection;
    }

    private void CreatePathTrack(Vector3 instancePosition, GameObject trackInstanceObj)
    {
        if (trackInstanceObj == exitTrackPath)
        {
            exitTrackInstances.Add(Instantiate(trackInstanceObj, instancePosition + Vector3.up * 2, trackInstanceObj.transform.rotation));
        }
        else
        {
            Instantiate(trackInstanceObj, instancePosition + Vector3.up * 2, trackInstanceObj.transform.rotation);
        }
    }

    struct CollidingObjects
    {
        public List<Vector3> wall;
        public List<Vector3> coin;
        public Vector3 exit;

        public CollidingObjects(List<Vector3> wall, List<Vector3> coin, Vector3 exit)
        {
            this.wall = new List<Vector3>();
            this.coin = new List<Vector3>();
            this.wall = wall;
            this.coin = coin;
            this.exit = exit;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Coin"))
        {
            other.gameObject.SetActive(false);
            mazeGeneratorScript.CoinsCount--;
        }
        if (other.gameObject.CompareTag("Exit"))
        {
            other.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}

[CustomEditor(typeof(Bot))]
public class MazeBotControlEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Bot mazeBotControl = (Bot)target;

        if (GUILayout.Button("Awake Bot"))
        {
            mazeBotControl.StartBot();
        }
    }
}
