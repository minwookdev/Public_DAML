using UnityEngine;

namespace CoffeeCat
{
    public class DoorLayerAdjuster : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;

        private void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // sortingOrder 20 : over player
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out Player_Dungeon player))
                spriteRenderer.sortingOrder = 20;
        }

        // sortingOrder 10 : under player
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent(out Player_Dungeon player))
                spriteRenderer.sortingOrder = 10;
        }
    }
}