using UnityEngine;

public sealed class BoardCell : MonoBehaviour
{
    [SerializeField] private int column;
    [SerializeField] private int row;

    public int Column => column;
    public int Row => row;
}
