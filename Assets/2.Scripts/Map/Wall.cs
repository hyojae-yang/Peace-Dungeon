using UnityEngine;
using System.Collections.Generic;

public enum WallType
{
    TypeA, TypeB, TypeC, TypeD, TypeE, TypeF, TypeG, TypeH, TypeI, TypeJ
}

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(MeshRenderer))]
public class Wall : MonoBehaviour
{
    [Header("벽 종류 설정")]
    public WallType wallType;

    [Header("이 벽과 붙어도 비활성화 되지 않음")]
    public List<WallType> ignoreTypes; // 예: TypeA 리스트에 넣으면 TypeA와 충돌해도 비활성화 안됨

    private Collider wallCollider;
    private MeshRenderer meshRenderer;
    private int overlapCount = 0;

    private void Awake()
    {
        wallCollider = GetComponent<Collider>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Wall otherWall = collision.gameObject.GetComponent<Wall>();
        if (otherWall == null) return;

        // 같은 종류는 무시
        if (otherWall.wallType == wallType) return;

        // ignoreTypes에 포함된 종류면 무시
        if (ignoreTypes.Contains(otherWall.wallType)) return;

        overlapCount++;
        if (overlapCount > 0)
        {
            SetWallVisible(false);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        Wall otherWall = collision.gameObject.GetComponent<Wall>();
        if (otherWall == null) return;

        if (otherWall.wallType == wallType) return;
        if (ignoreTypes.Contains(otherWall.wallType)) return;

        overlapCount--;
        if (overlapCount <= 0)
        {
            overlapCount = 0;
            SetWallVisible(true);
        }
    }

    private void SetWallVisible(bool isVisible)
    {
        wallCollider.enabled = isVisible;
        meshRenderer.enabled = isVisible;
    }
}
