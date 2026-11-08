
using UnityEngine;

public class Projected2DSource : MonoBehaviour
{
    public Projected2DCollider Source { get; private set; }

    public void Initialize ( Projected2DCollider source )
    {
        Source = source;
    }
}
