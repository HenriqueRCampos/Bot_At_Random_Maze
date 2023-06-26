using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

/// <summary>
/// O bot nao consegue percorrer em mapas com tamanho impar, e nao consegue capturar descobrir onde estao as chaves para a saida, mas consegue eliminiar os caminhos que ja foram totalmente percorridos.
/// </summary>
public class MazeBotControl : MonoBehaviour
{
    [SerializeField] private GameObject completedTrackPath;
    [SerializeField] private GameObject exitTrackPath;
    [Range(0, 1.5f)]
    [SerializeField] private float movSpeed = 1;
    [SerializeField] private List<Vector3> pathWalked = new List<Vector3>();
    [SerializeField] private List<Vector3> pathCompleted = new List<Vector3>();
    [SerializeField] private List<Vector3> pathExit = new List<Vector3>();
    [SerializeField] private List<Vector3> directions = new List<Vector3>();
    private RaycastHit[] hit = new RaycastHit[4];
    private bool afterCrossingPath;
    private bool isInExitPath;

    private MazeGenerator mazeGenerator;
    private Vector3 movDirection;
    private Vector3 exitLabPosition;
    private Vector3 botSideToExitPosition;

    private void Start()    
    {
        directions.Add(transform.TransformDirection(Vector3.forward));
        directions.Add(transform.TransformDirection(Vector3.right));
        directions.Add(transform.TransformDirection(Vector3.back));
        directions.Add(transform.TransformDirection(Vector3.left));

        mazeGenerator = GameObject.FindGameObjectWithTag("Generator").GetComponent<MazeGenerator>();
    }

    public void StartBot()
    {
        StartCoroutine(MoveBot());
        exitLabPosition = GameObject.FindGameObjectWithTag("Exit").transform.position;
    }

    private IEnumerator MoveBot()
    {
        yield return new WaitForSeconds(movSpeed);
        Vector3 movDirection = DirectionRay();

        gameObject.transform.position = movDirection;
        yield return MoveBot();
    }

    private Vector3 DirectionRay()
    {
        List<Vector3> freeDirections = new List<Vector3>();
        ObjectsPositions objPosition = new ObjectsPositions();
        objPosition.wall = new List<Vector3>();
        objPosition.coin = new List<Vector3>();
        Vector3 direction;

        for (int i = 0; i < directions.Count; i++)
        {
            if (!Physics.Raycast(transform.position, directions[i], out hit[i], 1))
            {
                freeDirections.Add(directions[i]);
            }
            if (Physics.Raycast(transform.position, directions[i], out hit[i], 1))
            {
                switch (hit[i].collider.tag)
                {
                    case "Wall":
                        objPosition.wall.Add(directions[i]);
                        break;
                    case "Coin":
                        objPosition.coin.Add(directions[i]);
                        break;
                    case "Exit":
                        objPosition.exit = directions[i];
                        botSideToExitPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
                        break;
                }
            }
        }
        direction = MovDirection(freeDirections, objPosition);
        return direction;
    }

    private Vector3 MovDirection(List<Vector3> freeDirection, ObjectsPositions objPos)
    {
        if (objPos.coin.Count > 0)
        {
            movDirection = new Vector3(transform.position.x, transform.position.y, transform.position.z) + objPos.coin[0];
        }
        else if (objPos.exit != Vector3.zero && mazeGenerator.CoinsCount <= 0)
        {
            movDirection = new Vector3(transform.position.x, transform.position.y, transform.position.z) + objPos.exit;
        }
        else if (botSideToExitPosition + objPos.exit == exitLabPosition && mazeGenerator.CoinsCount > 0)
        {
            movDirection = FoundExitButDosntCollectAllKeys(freeDirection);
        }
        else if (objPos.wall.Count == 3)
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

    private Vector3 FoundExitButDosntCollectAllKeys(List<Vector3> freeDirection)
    {
        Vector3 finalDirection = Vector3.zero;
        afterCrossingPath = true;
        isInExitPath = true;
        pathWalked.Clear();
        pathWalked.Add(transform.position);
        pathExit.Add(transform.position);

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
        Vector3 finalDirection;
        finalDirection = new Vector3(transform.position.x, transform.position.y, transform.position.z) + freeDirection[0];
        pathWalked.Clear();
        pathWalked.Add(transform.position);
        pathCompleted.Add(transform.position);

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
            if (isInExitPath)
            {
                pathExit.Add(transform.position);
            }
            if (!afterCrossingPath)
            {
                pathCompleted.Add(transform.position);
                pathExit.Add(transform.position);
            }
        }
        return finalDirection;
    }

    private Vector3 CrossingPathDirections(List<Vector3> freeDirection, int blockeDirectionCompare, int blockedPathCompare, int completedQuantityCompare)
    {
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
                pathExit.Add(transform.position);
            }
            else
            {
                afterCrossingPath = true;
                isInExitPath = false;
            }

            if (!pathWalked.Contains(nextDirection) && !pathCompleted.Contains(nextDirection) && !pathExit.Contains(nextDirection))
            {
                finalDirection = nextDirection;
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
                }
            }
        }
        return finalDirection;
    }

    struct ObjectsPositions
    {
        public List<Vector3> wall;
        public List<Vector3> coin;
        public Vector3 exit;

        public ObjectsPositions(List<Vector3> wall, List<Vector3> coin, Vector3 exit)
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
            mazeGenerator.CoinsCount--;
        }
        if (other.gameObject.CompareTag("Exit"))
        {
            other.gameObject.SetActive(false);
            gameObject.SetActive(false);
            Debug.Log("GG");
        }
    }
}

[CustomEditor(typeof(MazeBotControl))]
public class MazeBotControlEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MazeBotControl mazeBotControl = (MazeBotControl)target;

        if (GUILayout.Button("Awake Bot"))
        {
            mazeBotControl.StartBot();
        }
    }
}
