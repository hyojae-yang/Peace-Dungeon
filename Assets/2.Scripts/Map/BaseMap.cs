using System.Collections.Generic;
using UnityEngine;

public abstract class BaseMap : MonoBehaviour
{
    private Camera mainCam;
    protected bool isDragging = false;
    private float lastClickTime = 0f;
    private const float doubleClickTime = 0.3f;

    protected SmallMapItem smallMapItem;

    [Header("Map Tile Data")]
    [SerializeField]
    protected List<Vector3> mapTiles = new List<Vector3>();

    [SerializeField]
    protected Vector3 originTile = Vector3.zero;

    [SerializeField] protected Vector3 tileSize = new Vector3(100f, 1f, 100f);

    protected Color validGizmoColor = Color.cyan;
    protected Color invalidGizmoColor = Color.red;

    protected virtual void Awake()
    {
        mainCam = Camera.main;

        if (mainCam == null)
        {
            Debug.LogError("메인 카메라를 찾을 수 없습니다. 'MainCamera' 태그가 올바르게 설정되었는지 확인하세요.");
        }

        smallMapItem = GetComponent<SmallMapItem>();
    }

    protected virtual void Update()
    {
        if (isDragging)
        {
            if (Input.GetMouseButtonDown(1))
            {
                transform.Rotate(0, 90, 0, Space.Self);
            }

            Vector3 mousePosition = GetMouseWorldPosition();
            transform.position = mousePosition;
        }
    }

    private void OnMouseDown()
    {
        if (Time.time - lastClickTime < doubleClickTime)
        {
            if (smallMapItem != null)
            {
                smallMapItem.HandleDoubleClick();
            }
            return;
        }

        isDragging = true;
        lastClickTime = Time.time;
    }

    private void OnMouseUp()
    {
        isDragging = false;
        PlaceMapOnGrid();
    }

    protected abstract void PlaceMapOnGrid();

    public List<Vector3> GetRotatedMapTiles()
    {
        List<Vector3> rotatedTiles = new List<Vector3>();
        foreach (Vector3 tile in mapTiles)
        {
            rotatedTiles.Add(transform.rotation * tile);
        }
        return rotatedTiles;
    }

    protected Vector3 GetMouseWorldPosition()
    {
        if (mainCam == null)
        {
            return transform.position;
        }

        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000f, LayerMask.GetMask("Ground")))
        {
            return hit.point;
        }
        return transform.position;
    }
}