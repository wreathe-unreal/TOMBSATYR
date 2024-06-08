using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TOMBSATYR
{
    public static class Vector3Extensions
    {
        public static Vector3 DisplaceFromPoint(this Vector3 point, Vector3 direction, float displacement)
        {

            Vector3 desiredDisplacement = direction * displacement;
            Vector3 newPositionOffWall = desiredDisplacement + point;
            return newPositionOffWall;
        }
    }
    
    public static class CameraExtensions
    {
        public static bool IsInView(this Camera camera, GameObject obj, float angle)
        {
            Vector3 toObj = obj.transform.position - camera.transform.position;
            float dotProduct = Vector3.Dot(camera.transform.forward, toObj.normalized);

            float angleToObj = Mathf.Acos(dotProduct);

            return angleToObj <= Mathf.Deg2Rad * angle;
        }

        public static bool IsUnobstructed(this Camera camera, GameObject obj)
        {
            Vector3 toObj = obj.transform.position - camera.transform.position;
            Ray ray = new Ray(camera.transform.position, toObj);
            RaycastHit hit;

            LayerMask layerMask = 1 << LayerMask.NameToLayer("Player");
            layerMask |= 1 << LayerMask.NameToLayer("Fairy");
            layerMask = ~layerMask; // Invert the mask to hit everything except "Player" and "Fairy"

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
                {
                    if (hit.collider != null)
                    {
                        if (hit.collider.gameObject == obj)
                        {
                            return true;
                        }

                        if (hit.collider.transform.parent != null && hit.collider.transform.parent.gameObject == obj)
                        {
                            return true;
                        }
                    }
                }

                Debug.Log(hit.collider.gameObject.name);
                if ((hit.collider.gameObject != null && hit.collider.gameObject == obj) ||
                    (hit.collider.transform.parent.gameObject != null &&
                     hit.collider.transform.parent.gameObject == obj))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public static class GameObjectExtensions
    {
        public static GameObject FindRootWithComponent<T>(this GameObject obj) where T : Component
        {
            Transform currentTransform = obj.transform;

            while (currentTransform != null)
            {
                if (currentTransform.GetComponent<T>() != null)
                {
                    return currentTransform.gameObject;
                }

                currentTransform = currentTransform.parent;
            }

            return null; // Return null if no game object with the component is found
        }

        public static GameObject FindGameObjectWithScript<T>(this GameObject startObj) where T : MonoBehaviour
        {
            Transform currentTransform = startObj.transform;

            while (currentTransform != null)
            {
                if (currentTransform.GetComponent<T>() != null)
                {
                    return currentTransform.gameObject;
                }

                currentTransform = currentTransform.parent;
            }

            return null; // Return null if no game object with the script is found
        }

        public static GameObject FindChildWithTag(this GameObject parent, string tag)
        {
            Transform[] children = parent.GetComponentsInChildren<Transform>();
            foreach (Transform child in children)
            {
                if (child.CompareTag(tag))
                {
                    return child.gameObject;
                }
            }

            return null;
        }

        public static GameObject GetRoot(this GameObject obj)
        {
            Transform currentTransform = obj.transform;

            while (currentTransform.parent != null)
            {
                currentTransform = currentTransform.parent;
            }

            return currentTransform.gameObject;
        }
    }

    public class Utils : UnityEngine.Object
    {
        public static List<T> FindObjects<T>(Func<T, bool> filter) where T : UnityEngine.Object
        {
            List<T> foundObjects = new List<T>();

            foreach (T obj in FindObjectsOfType<T>())
            {
                if (filter(obj))
                {
                    foundObjects.Add(obj);
                }
            }

            return foundObjects;
        }

        public static T FindMaxObject<T>(Func<T, T, bool> compare) where T : UnityEngine.Object
        {
            T maxObj = null;

            foreach (T obj in FindObjectsOfType<T>())
            {
                if (maxObj == null)
                {
                    maxObj = obj;
                }
                else
                {
                    if (compare(maxObj, obj))
                    {
                        maxObj = obj;
                    }
                }
            }

            return maxObj;
        }
    }
}