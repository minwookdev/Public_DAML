using UnityEngine;

namespace CoffeeCat {
    public class Projectile : MonoBehaviour {
        protected Transform tr = null;
        protected Rigidbody2D rigidBody2D = null;

        public void ForceToForward(float force) {
            rigidBody2D.AddForce(transform.right * force, ForceMode2D.Impulse);
        }

        public void ForceToDirection(Vector2 direction, float force) {
            direction.Normalize();
            rigidBody2D.AddForce(direction * force, ForceMode2D.Impulse);
        }
    }
}
