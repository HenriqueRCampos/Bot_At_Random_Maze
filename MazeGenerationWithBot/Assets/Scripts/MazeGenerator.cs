using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [SerializeField] private MazeNode nodePrefab;
    [SerializeField] private GameObject bot;
    [SerializeField] private GameObject key;
    [SerializeField] private GameObject mazeExit;
    [SerializeField] private Vector2Int mazeSize;
    [SerializeField] private int coinsCount;

    private List<MazeNode> currentPath = new List<MazeNode>();
    private List<MazeNode> completedNodes = new List<MazeNode>();

    private bool continuosLockedPath = true;
    public int CoinsCount { get => coinsCount; set => coinsCount = value; }

    private void Start()
    {
        GenerateMazeNodes(mazeSize);
    }
    private void GenerateMazeNodes(Vector2Int mazeSize)
    {
        List<MazeNode> nodes = new List<MazeNode>();

        for (int x = 0; x < mazeSize.x; x++)
        {
            for (int y = 0; y < mazeSize.y; y++)
            {
                Vector3 nodePos = new Vector3(x - (mazeSize.x / 2f), 0, y - (mazeSize.y / 2f));
                MazeNode newNode = Instantiate(nodePrefab, nodePos, Quaternion.identity, transform);
                nodes.Add(newNode);
            }
        }
        SetInicialPoint(nodes);
        GenerateMazePath(nodes);
    }
    private void SetInicialPoint(List<MazeNode> nodes)
    {
        currentPath.Add(nodes[Random.Range(0, nodes.Count)]);
        Instantiate(bot, currentPath[0].transform.position, bot.transform.rotation);
    }

    private void GenerateMazePath(List<MazeNode> nodes)
    {
        while (completedNodes.Count < nodes.Count)
        {
            List<int> nextNode = new List<int>();
            List<int> directions = new List<int>();

            int currentNodeIndex = nodes.IndexOf(currentPath[currentPath.Count - 1]);
            int currentNodeX = currentNodeIndex / mazeSize.y;
            int currentNodeY = currentNodeIndex % mazeSize.y;

            if (currentNodeX < mazeSize.x - 1)
            {
                PossibleDirections(new DirectionValidation(nodes, directions, nextNode, currentNodeIndex, mazeSize.y, 1));
            }
            if (currentNodeX > 0)
            {
                PossibleDirections(new DirectionValidation(nodes, directions, nextNode, currentNodeIndex, -mazeSize.y, 2));
            }
            if (currentNodeY < mazeSize.y - 1)
            {
                PossibleDirections(new DirectionValidation(nodes, directions, nextNode, currentNodeIndex, 1, 3));
            }
            if (currentNodeY > 0)
            {
                PossibleDirections(new DirectionValidation(nodes, directions, nextNode, currentNodeIndex, -1, 4));
            }

            if (directions.Count > 0)
            {
                int chosenDirection = Random.Range(0, directions.Count);
                MazeNode choseNode = nodes[nextNode[chosenDirection]];

                switch (directions[chosenDirection])
                {
                    case 1:
                        choseNode.RemoveWall(1);
                        currentPath[currentPath.Count - 1].RemoveWall(0);
                        break;
                    case 2:
                        choseNode.RemoveWall(0);
                        currentPath[currentPath.Count - 1].RemoveWall(1);
                        break;
                    case 3:
                        choseNode.RemoveWall(3);
                        currentPath[currentPath.Count - 1].RemoveWall(2);
                        break;
                    case 4:
                        choseNode.RemoveWall(2);
                        currentPath[currentPath.Count - 1].RemoveWall(3);
                        break;
                }

                continuosLockedPath = false;
                currentPath.Add(choseNode);
            }
            else
            {
                completedNodes.Add(currentPath[currentPath.Count - 1]);

                if (!continuosLockedPath)
                {
                    if (CoinsCount == 4)
                    {
                        Transform exitPath = currentPath[currentPath.Count - 1].transform;
                        Instantiate(mazeExit, exitPath.position, mazeExit.transform.rotation);
                        CoinsCount++;
                    }
                    else
                    {
                        Transform blockedPath = currentPath[currentPath.Count - 1].transform;
                        Instantiate(key, blockedPath.position, key.transform.rotation);
                        CoinsCount++;
                    }
                }

                continuosLockedPath = true;
                currentPath.RemoveAt(currentPath.Count - 1);
            }
        }
        if (completedNodes.Count == nodes.Count)
        {
            CoinsCount--;
        }
    }

    public struct DirectionValidation
    {
        public List<MazeNode> nodes;
        public List<int> directions;
        public List<int> nextNode;
        public int currentNodeIndex;
        public int addNodeIndex;
        public int value;

        public DirectionValidation(List<MazeNode> nodes, List<int> directions, List<int> nextNode, int currentNodeIndex, int value, int addNodeIndex)
        {
            this.nodes = nodes;
            this.directions = directions;
            this.nextNode = nextNode;
            this.currentNodeIndex = currentNodeIndex;
            this.addNodeIndex = addNodeIndex;
            this.value = value;
        }
    }

    private void PossibleDirections(DirectionValidation validation)
    {
        if (!completedNodes.Contains(validation.nodes[validation.currentNodeIndex + validation.value]) && !currentPath.Contains(validation.nodes[validation.currentNodeIndex + validation.value]))
        {
            validation.directions.Add(validation.addNodeIndex);
            validation.nextNode.Add(validation.currentNodeIndex + validation.value);
        }
    }
}
