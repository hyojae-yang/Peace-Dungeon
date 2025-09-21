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
    [Header("�� ���� ����")]
    public WallType wallType;

    [Header("�� ���� �پ ��Ȱ��ȭ ���� ����")]
    public List<WallType> ignoreTypes; // ��: TypeA ����Ʈ�� ������ TypeA�� �浹�ص� ��Ȱ��ȭ �ȵ�

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

        // ���� ������ ����
        if (otherWall.wallType == wallType) return;

        // ignoreTypes�� ���Ե� ������ ����
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
