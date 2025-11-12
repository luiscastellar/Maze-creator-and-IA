using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using TMPro; 
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Slider = UnityEngine.UI.Slider;
using Toggle = UnityEngine.UI.Toggle;

namespace _Scripts
{
    public class MazeGenerate : MonoBehaviour
    {
        #region Variables
        
        private Vector2Int mazeSize = new Vector2Int(30, 30);
        
        private bool seeProcess;
        
        private float heuristicWeight = 0.0f;
        
        List<MazeNode> nodes = new List<MazeNode>();
        
        private MazeNode startNode;
        private MazeNode endNode;

        private bool _endPathReached;
        private bool _isSearching;

        [Header("Prefabs")]
        [SerializeField] MazeNode nodePrefab;
        
        [Header("UI References")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private TMP_InputField colsInput;
        [SerializeField] private TMP_InputField rowsInput;
        [SerializeField] private Slider weightSlider;
        [SerializeField] private Toggle seeProcessToggle;
        [SerializeField] private Button generateButton;

        [Header("Action Buttons")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button configurationButton;

        #endregion

        #region UI

        public void StartGenerationFromUI()
        {
            ClearMaze();
            
            int cols = int.TryParse(colsInput.text, out int c) ? c : mazeSize.x;
            int rows = int.TryParse(rowsInput.text, out int r) ? r : mazeSize.y;
            mazeSize = new Vector2Int(cols, rows);

            heuristicWeight = weightSlider.value;
            seeProcess = seeProcessToggle.isOn;

            settingsPanel.SetActive(false);
            
            startButton.gameObject.SetActive(true);   
            restartButton.gameObject.SetActive(true);

            if (seeProcess)
                StartCoroutine(GenerateMaze(mazeSize));
            else
                GenerateMazeInstant(mazeSize);
        }

        private void PopulateUIWithDefaults()
        {
            colsInput.text = mazeSize.x.ToString();
            rowsInput.text = mazeSize.y.ToString();
            
            weightSlider.minValue = 1f;
            weightSlider.maxValue = 10f;
            weightSlider.value = heuristicWeight;

            seeProcessToggle.isOn = seeProcess;
        }

        #endregion

        #region Start
        
        private void Start()
        {
            settingsPanel.SetActive(true);
            PopulateUIWithDefaults();

            generateButton.onClick.AddListener(StartGenerationFromUI);
            startButton.onClick.AddListener(OnStartPathfindingClicked); 
            restartButton.onClick.AddListener(Regenerate);
            configurationButton.onClick.AddListener(OpenConfigurationPanel);

            startButton.gameObject.SetActive(false); 
            restartButton.gameObject.SetActive(false);
        }

        private void OnStartPathfindingClicked()
        {
            if (!settingsPanel.activeSelf && startNode != null && endNode != null && !_endPathReached && !_isSearching)
            {
                StartCoroutine(Pathfinding(mazeSize));
            }
        }
        
        #endregion

        #region GenerateMazeProcess
        IEnumerator GenerateMaze(Vector2Int size)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    Vector3 nodePos = new Vector3(x - (size.x / 2f), 0, y - (size.y / 2f));
                    MazeNode newNode = Instantiate(nodePrefab, nodePos, Quaternion.identity, transform);
                    
                    newNode.x = x;
                    newNode.y = y;
                    
                    nodes.Add(newNode);
                    
                    yield return null;
                }
            }
        
            List<MazeNode> currentPath = new List<MazeNode>();
            List<MazeNode> completedNodes = new List<MazeNode>();
        
            currentPath.Add(nodes[Random.Range(0, nodes.Count)]);

            while (completedNodes.Count < nodes.Count)
            {
                List<int> posibleNextNodes = new List<int>();
                List<int> posibleDirections = new List<int>();

                int currentNodeIndex = nodes.IndexOf(currentPath[currentPath.Count - 1]);
                int currentNodeX = currentNodeIndex / size.y;
                int currentNodeY = currentNodeIndex % size.y;

                if (currentNodeX < size.x - 1)
                {
                    if (!completedNodes.Contains(nodes[currentNodeIndex + size.y]) && !currentPath.Contains(nodes[currentNodeIndex + size.y]))
                    {
                        posibleDirections.Add(1);
                        posibleNextNodes.Add(currentNodeIndex + size.y);
                    }
                }
            
                if (currentNodeX > 0)
                {
                    if (!completedNodes.Contains(nodes[currentNodeIndex - size.y]) && !currentPath.Contains(nodes[currentNodeIndex - size.y]))
                    {
                        posibleDirections.Add(2);
                        posibleNextNodes.Add(currentNodeIndex - size.y);
                    }
                }
            
                if (currentNodeY < size.y - 1)
                {               
                    if (!completedNodes.Contains(nodes[currentNodeIndex + 1]) && !currentPath.Contains(nodes[currentNodeIndex + 1]))
                    {
                        posibleDirections.Add(3);
                        posibleNextNodes.Add(currentNodeIndex + 1);
                    }
                }

                if (currentNodeY > 0)
                {             
                    if (!completedNodes.Contains(nodes[currentNodeIndex - 1]) && !currentPath.Contains(nodes[currentNodeIndex - 1]))
                    {
                        posibleDirections.Add(4);
                        posibleNextNodes.Add(currentNodeIndex - 1);
                    }
                }

                if (posibleDirections.Count > 0)
                {
                    int chosenDirection = Random.Range(0, posibleDirections.Count);
                    MazeNode chosenNode = nodes[posibleNextNodes[chosenDirection]];
                
                    switch (posibleDirections[chosenDirection])
                    {
                        case 1:
                            chosenNode.RemoveWalls(1);
                            currentPath[currentPath.Count - 1].RemoveWalls(0);
                            break;
                        case 2:
                            chosenNode.RemoveWalls(0);
                            currentPath[currentPath.Count - 1].RemoveWalls(1);
                            break;
                        case 3:
                            chosenNode.RemoveWalls(3);
                            currentPath[currentPath.Count - 1].RemoveWalls(2);
                            break;
                        case 4:
                            chosenNode.RemoveWalls(2);
                            currentPath[currentPath.Count - 1].RemoveWalls(3);
                            break;
                    }
                
                    currentPath.Add(chosenNode);
                }
                else
                {
                    completedNodes.Add(currentPath[currentPath.Count - 1]);
                
                    currentPath.RemoveAt(currentPath.Count -1);
                }

                    yield return null;
            }
            
            startNode = nodes[Random.Range(0, mazeSize.y)];
            startNode.SetState(NodeState.Start);
            
            int lastIndex = nodes.Count - 1;
            endNode = nodes[Random.Range(lastIndex, lastIndex - mazeSize.y)];
            endNode.SetState(NodeState.End);
            
            AsignHCosts();
        }
        #endregion
        
        #region GenerateMazeInstant
        void GenerateMazeInstant(Vector2Int size)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    Vector3 nodePos = new Vector3(x - (size.x / 2f), 0, y - (size.y / 2f));
                    MazeNode newNode = Instantiate(nodePrefab, nodePos, Quaternion.identity, transform);

                    newNode.x = x;
                    newNode.y = y;
                    
                    nodes.Add(newNode);
                }
            }
        
            List<MazeNode> currentParth = new List<MazeNode>();
            List<MazeNode> completedNodes = new List<MazeNode>();
        
            currentParth.Add(nodes[Random.Range(0, nodes.Count)]);

            while (completedNodes.Count < nodes.Count)
            {
                List<int> posibleNextNodes = new List<int>();
                List<int> posibleDirections = new List<int>();

                int currentNodeIndex = nodes.IndexOf(currentParth[currentParth.Count - 1]);
                int currentNodeX = currentNodeIndex / size.y;
                int currentNodeY = currentNodeIndex % size.y;
            
                if (currentNodeX < size.x - 1)
                {
                    if (!completedNodes.Contains(nodes[currentNodeIndex + size.y]) && !currentParth.Contains(nodes[currentNodeIndex + size.y]))
                    {
                        posibleDirections.Add(1);
                        posibleNextNodes.Add(currentNodeIndex + size.y);
                    }
                }
            
                if (currentNodeX > 0)
                {
                    if (!completedNodes.Contains(nodes[currentNodeIndex - size.y]) && !currentParth.Contains(nodes[currentNodeIndex - size.y]))
                    {
                        posibleDirections.Add(2);
                        posibleNextNodes.Add(currentNodeIndex - size.y);
                    }
                }
            
                if (currentNodeY < size.y - 1)
                {
                    if (!completedNodes.Contains(nodes[currentNodeIndex + 1]) && !currentParth.Contains(nodes[currentNodeIndex + 1]))
                    {
                        posibleDirections.Add(3);
                        posibleNextNodes.Add(currentNodeIndex + 1);
                    }
                }

                if (currentNodeY > 0)
                {
                    if (!completedNodes.Contains(nodes[currentNodeIndex - 1]) && !currentParth.Contains(nodes[currentNodeIndex - 1]))
                    {
                        posibleDirections.Add(4);
                        posibleNextNodes.Add(currentNodeIndex - 1);
                    }
                }

                if (posibleDirections.Count > 0)
                {
                    int chosenDirection = Random.Range(0, posibleDirections.Count);
                    MazeNode chosenNode = nodes[posibleNextNodes[chosenDirection]];

                    switch (posibleDirections[chosenDirection])
                    {
                        case 1:
                            chosenNode.RemoveWalls(1);
                            currentParth[currentParth.Count - 1].RemoveWalls(0);
                            break;
                        case 2:
                            chosenNode.RemoveWalls(0);
                            currentParth[currentParth.Count - 1].RemoveWalls(1);
                            break;
                        case 3:
                            chosenNode.RemoveWalls(3);
                            currentParth[currentParth.Count - 1].RemoveWalls(2);
                            break;
                        case 4:
                            chosenNode.RemoveWalls(2);
                            currentParth[currentParth.Count - 1].RemoveWalls(3);
                            break;
                    }
                
                    currentParth.Add(chosenNode);
                }
                else
                {
                    completedNodes.Add(currentParth[currentParth.Count - 1]);
                
                    currentParth.RemoveAt(currentParth.Count -1);
                }
            }
            
            startNode = nodes[Random.Range(0, mazeSize.y)];
            startNode.SetState(NodeState.Start);
            
            int lastIndex = nodes.Count - 1;
            endNode = nodes[Random.Range(lastIndex, lastIndex - mazeSize.y)];
            endNode.SetState(NodeState.End);

            AsignHCosts();
        }
        #endregion

        #region HCosts
        void AsignHCosts()
        {
            foreach (var node in nodes)
            {
                node.hCost = CalculateDistance(node, endNode);
            }
        }
        #endregion
        
        #region PathFinding
        
        private const int MoveCost = 10;

        IEnumerator Pathfinding(Vector2Int size)
        {
            _isSearching = true; 

            List<MazeNode> openList = new List<MazeNode>();
            HashSet<MazeNode> closedList = new HashSet<MazeNode>();

            foreach (var node in nodes)
            {
                node.gCost = int.MaxValue; 
                node.cameFromNode = null;
            }

            startNode.gCost = 0; 
            
            openList.Add(startNode);

            while (openList.Count > 0)
            {
                MazeNode currentNode = GetLowestFCostNode(openList);

                if (currentNode == endNode)
                {
                    _endPathReached = true;
                    StartCoroutine(CalculatePath(endNode)); 
                    _isSearching = false; 
                    yield break;
                }

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                if (currentNode != startNode)
                    currentNode.SetState(NodeState.AlreadyInvestigated);

                foreach (MazeNode neighbour in GetNeighbours(currentNode, size))
                {
                    if (closedList.Contains(neighbour))
                    {
                        continue;
                    }

                    int tentativeGCost = currentNode.gCost + MoveCost;

                    if (tentativeGCost < neighbour.gCost)
                    {
                        neighbour.cameFromNode = currentNode;
                        neighbour.gCost = tentativeGCost;

                        if (!openList.Contains(neighbour))
                        {
                            openList.Add(neighbour);
                            
                            if (neighbour != endNode)
                                neighbour.SetState(NodeState.Investigated);
                        }
                    }
                }
                
                yield return null; 
            }
            
            _isSearching = false;
        }

        private List<MazeNode> GetNeighbours(MazeNode currentNode, Vector2Int size)
        {
            List<MazeNode> neighbours = new List<MazeNode>();

            int currentNodeIndex = nodes.IndexOf(currentNode);
            int currentNodeX = currentNode.x; 
            int currentNodeY = currentNode.y; 
            
            if (currentNodeX < size.x - 1 && !currentNode.GetWalls(0))
            {
                neighbours.Add(nodes[currentNodeIndex + size.y]);
            }
            
            if (currentNodeX > 0 && !currentNode.GetWalls(1))
            {
                neighbours.Add(nodes[currentNodeIndex - size.y]);
            }

            if (currentNodeY < size.y - 1 && !currentNode.GetWalls(2))
            {
                neighbours.Add(nodes[currentNodeIndex + 1]);
            }

            if (currentNodeY > 0 && !currentNode.GetWalls(3))
            {
                neighbours.Add(nodes[currentNodeIndex - 1]);
            }

            return neighbours;
        }

        private MazeNode GetLowestFCostNode(List<MazeNode> mazeNodeList)
        {
            MazeNode lowestFCostNode = mazeNodeList[0];
            
            float lowestFCost = lowestFCostNode.gCost + (lowestFCostNode.hCost * heuristicWeight);

            for (int i = 1; i < mazeNodeList.Count; i++)
            {
                MazeNode currentNode = mazeNodeList[i];
                float currentFCost = currentNode.gCost + (currentNode.hCost * heuristicWeight);

                if (currentFCost < lowestFCost)
                {
                    lowestFCost = currentFCost;
                    lowestFCostNode = currentNode;
                }
                else if (currentFCost == lowestFCost && currentNode.hCost < lowestFCostNode.hCost)
                {
                    lowestFCostNode = currentNode;
                }
            }
            return lowestFCostNode;
        }

        private int CalculateDistance(MazeNode a, MazeNode b)
        {
            int xDistance = Mathf.Abs(a.x - b.x);
            int yDistance = Mathf.Abs(a.y - b.y);
            
            return MoveCost * (xDistance + yDistance);
        }
                
        IEnumerator CalculatePath(MazeNode endNode) 
        {
            List<MazeNode> path = new List<MazeNode>();
            
            path.Add(endNode);
            MazeNode currentNode = endNode;

            while (currentNode.cameFromNode != null && currentNode.cameFromNode != startNode)
            {
                path.Add(currentNode.cameFromNode);
                currentNode = currentNode.cameFromNode;
                currentNode.SetState(NodeState.PerfectSearch);
                
                yield return null;
            }
            
            path.Reverse();

            yield return null;
        }
        #endregion

        #region RebuildMaze

        void ClearMaze()
        {
            StopAllCoroutines();

            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            nodes.Clear();

            startNode = null;
            endNode = null;
            _endPathReached = false;
            _isSearching = false;
        }

        private void OpenConfigurationPanel()
        {
            PopulateUIWithDefaults();
            settingsPanel.SetActive(true);
        }

        void Regenerate()
        {
            ClearMaze();

            int cols = int.TryParse(colsInput.text, out int c) ? c : mazeSize.x;
            int rows = int.TryParse(rowsInput.text, out int r) ? r : mazeSize.y;
            mazeSize = new Vector2Int(cols, rows);

            heuristicWeight = weightSlider.value;
            seeProcess = seeProcessToggle.isOn;

            if (seeProcess)
                StartCoroutine(GenerateMaze(mazeSize));
            else
                GenerateMazeInstant(mazeSize);
        }
            
        #endregion
    }
}