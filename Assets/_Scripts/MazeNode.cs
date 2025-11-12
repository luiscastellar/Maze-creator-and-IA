using UnityEngine;

namespace _Scripts
{
    public enum NodeState
    {
        Start,
        Investigated,         
        AlreadyInvestigated, 
        PerfectSearch,
        End
    }
    public class MazeNode : MonoBehaviour
    {
        [SerializeField] GameObject[] walls;
        [SerializeField] MeshRenderer floor;

        public int x;
        public int y;
        
        public int gCost;
        public int hCost;

        public MazeNode cameFromNode;

        public void RemoveWalls(int wallToRemove)
        {
            walls[wallToRemove].gameObject.SetActive(false);
        }

        public bool GetWalls(int wallNum)
        {
            switch (wallNum)
            {
                case 0:
                    return walls[wallNum].gameObject.activeInHierarchy;
                case 1:
                    return walls[wallNum].gameObject.activeInHierarchy;
                case 2:
                    return walls[wallNum].gameObject.activeInHierarchy;
                case 3:
                    return walls[wallNum].gameObject.activeInHierarchy;
            }
            return false;
        }
    
        public void SetState(NodeState state)
        {
            switch (state)
            {
                case NodeState.Investigated:
                    floor.material.color = Color.yellow;
                    break;
                case NodeState.AlreadyInvestigated:
                    floor.material.color = Color.black;
                    break;
                case NodeState.Start:
                    floor.material.color = Color.magenta;
                    break;
                case NodeState.PerfectSearch:
                    floor.material.color = Color.blue;
                    break;
                case NodeState.End:
                    floor.material.color = Color.red;
                    break;
            }
        }
    }
}