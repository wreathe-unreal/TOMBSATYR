using UnityEngine;

namespace PolyTerrains.Demo
{
    public class FlyCamera : MonoBehaviour
    {
        public float lookSpeed = 200f;
        public float moveSpeed = 30f;
        public float ballSpeed = 1000f;

        private float rotationX;
        private float rotationY;

        private void Start()
        {
            rotationX = transform.localRotation.eulerAngles.y;
        }

        // Update is called once per frame
        private void Update()
        {
            rotationX += Input.GetAxis("Mouse X") * lookSpeed * Time.deltaTime;
            rotationY += Input.GetAxis("Mouse Y") * lookSpeed * Time.deltaTime;
            rotationY = Mathf.Clamp(rotationY, -90, 90);
            transform.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up) * Quaternion.AngleAxis(rotationY, Vector3.left);
            transform.position += (transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal")) * moveSpeed * Time.deltaTime;

            if (Input.GetMouseButtonDown(0)) {
                var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                ball.transform.position = transform.position;
                var body = ball.AddComponent<Rigidbody>();
                body.collisionDetectionMode = CollisionDetectionMode.Continuous;
                body.AddForce(transform.forward * ballSpeed);
            }
        }
    }
}