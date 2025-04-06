using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileSelector : MonoBehaviour
{
    public GameObject tileHighlightPrefab;

    private GameObject tileHighlight;

    public static TileSelector inst;

    public GameObject selectedObject = null;


    void Start ()
    {
        inst = this;
        

        Vector2Int gridPoint = Geometry.GridPoint(0, 0);
        Vector3 point = Geometry.PointFromGrid(gridPoint);
        tileHighlight = Instantiate(tileHighlightPrefab, point, Quaternion.identity, gameObject.transform);
        tileHighlight.SetActive(false);
    }

    void Update ()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Vector3 point = hit.point;
            Vector2Int gridPoint = Geometry.GridFromPoint(point);

            tileHighlight.SetActive(true);
            tileHighlight.transform.position = Geometry.PointFromGrid(gridPoint);
            if (Input.GetMouseButtonDown(0) && selectedObject == null)
            {
                GameObject selectedPiece = GameManager.instance.PieceAtGrid(gridPoint);
                if (GameManager.instance.DoesPieceBelongToCurrentPlayer(selectedPiece))
                {
                    selectedObject = selectedPiece;
                    

                    GameManager.instance.SelectPiece(selectedPiece);
                    ExitState(selectedPiece);
                }
            }
        }
        else
        {
            tileHighlight.SetActive(false);
        }

        if (Input.GetMouseButtonDown(1) && selectedObject != null)
        {
            MoveSelector.inst.CancelMove();
            selectedObject = null;
        }
        
    }

    public void EnterState()
    {
        enabled = true;
    }

    private void ExitState(GameObject movingPiece)
    {

        tileHighlight.SetActive(false);
        MoveSelector move = GetComponent<MoveSelector>();

        move.EnterState(movingPiece);
    }
}
