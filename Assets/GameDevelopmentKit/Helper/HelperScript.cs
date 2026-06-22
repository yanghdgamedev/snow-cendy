using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace GameDevelopmentKit.Scripts
{
    public static class HelperScript
    {
        private static System.Random rng = new System.Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }

        
        public static T GetRandom<T>(this IList<T> list)
        {
            if (list == null || list.Count == 0)
                throw new InvalidOperationException("List is empty, can not get random element.");

            int index = rng.Next(list.Count);
            return list[index];
        }
        
        public static void Shuffle<T>(this IList<T> list, int start, int end)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));
            if (start < 0 || end >= list.Count || start > end)
                throw new ArgumentOutOfRangeException("Invalid shuffle range.");

            int n = end - start + 1;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1) + start; // dịch chỉ số ngẫu nhiên về đúng phạm vi
                int last = start + n;
                (list[k], list[last]) = (list[last], list[k]);
            }
        }
        
        public static bool TrySwap<T>(this IList<T> list, int a, int b)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));
            if (a < 0 || b < 0 || a >= list.Count || b >= list.Count)
                return false;

            if (a == b)
                return true; // Không cần swap nhưng vẫn thành công

            (list[a], list[b]) = (list[b], list[a]);
            return true;
        }

        public static Material[] CloneAll(this Material[] materials)
        {
            if (materials == null)
                return null;

            Material[] clone = new Material[materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null)
                    clone[i] = new Material(materials[i]);
            }

            return clone;
        }

        public static Quaternion RotateTowardsTargetOverTime(Transform myTransform, Vector3 targetPosition,
            float speed = 50f)
        {
            Vector3 direction = targetPosition - myTransform.position;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                Quaternion smoothedRotation =
                    Quaternion.Slerp(myTransform.rotation, targetRotation, speed * Time.deltaTime);
                return smoothedRotation;
            }

            return myTransform.rotation;
        }

        public static Quaternion RotateTowardsTargetOverTimeWithDirection(Transform myTransform, Vector3 direction,
            float speed = 50f)
        {
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                Quaternion smoothedRotation =
                    Quaternion.Slerp(myTransform.rotation, targetRotation, speed * Time.deltaTime);
                return smoothedRotation;
            }

            return myTransform.rotation;
        }

        public static Quaternion RotateTowardsTargetOverQuaternion(Transform myTransform, Transform targetTransform,
            float speed = 50f)
        {
            Quaternion currentRotation = myTransform.rotation;
            Quaternion targetRotation = targetTransform.rotation;
            float t = speed * Time.deltaTime;
            Quaternion smoothedRotation = Quaternion.Lerp(currentRotation, targetRotation, t);

            return smoothedRotation;
        }

        private static Camera _camera;

        public static Camera Camera
        {
            get
            {
                if (_camera == null) _camera = Camera.main;
                return _camera;
            }
        }

        private static EventSystem _eventSystem;

        public static EventSystem EventSystemPP
        {
            get
            {
                if (_eventSystem == null)
                {
                    _eventSystem = Object.FindObjectOfType<EventSystem>();
                }

                return _eventSystem;
            }
        }

        private static readonly Dictionary<float, WaitForSeconds> WaitDictionary =
            new Dictionary<float, WaitForSeconds>();

        public static WaitForSeconds WaitForSeconds(float time)
        {
            if (WaitDictionary.TryGetValue(time, out var wait))
            {
                return wait;
            }

            var waitNew = new WaitForSeconds(time);
            WaitDictionary.Add(time, waitNew);
            return waitNew;
        }

        private static PointerEventData _eventDataCurrentPosition;
        private static List<RaycastResult> _results;

        public static bool IsOverUI()
        {
            _eventDataCurrentPosition = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
            _results = new List<RaycastResult>();
            EventSystemPP.RaycastAll(_eventDataCurrentPosition, _results);
            return _results.Count > 0;
        }

        public static Vector3 ScreenPointToWorldPointOnPlane(Camera camera, Vector3 screenPoint, float z)
        {
            screenPoint.z = z - camera.transform.position.z;
            Vector3 worldPoint = camera.ScreenToWorldPoint(screenPoint);
            return worldPoint;
        }

        public static Vector2 GetWorldPositionOfCanvasElement(RectTransform element)
        {
            RectTransformUtility.ScreenPointToWorldPointInRectangle(element, element.position, Camera.main,
                out var result);
            return result;
        }

        public static string GetFormattedDateTimeFromSeconds(long seconds)
        {
            if (seconds > 86400)
            {
                return string.Format("{0:00}d {1:00}:{2:00}:{3:00}", seconds / 86400, (seconds / 3600) % 24,
                    (seconds / 60) % 60, seconds % 60);
            }

            if (seconds > 3600)
            {
                return string.Format("{0:00}:{1:00}:{2:00}", seconds / 3600, (seconds / 60) % 60, seconds % 60);
            }

            return string.Format("0:{0:00}:{1:00}", seconds / 60, seconds % 60);
        }

        // public static DateTime GetLastMonday()
        // {
        //     DateTime now = WorldTimeAPI.Instance.GetCurrentDateTime();
        //     
        //     int daysToSubtract = (now.DayOfWeek - DayOfWeek.Monday + 7) % 7;
        //     DateTime lastMonday = now.Date.AddDays(-daysToSubtract);
        //     lastMonday = lastMonday.AddSeconds(1);
        //     return lastMonday;
        // }

        public static Vector2 FitSize(Vector2 child, Vector2 parent)
        {
            float ratio1 = child.x / child.y;
            float ratio2 = parent.x / parent.y;
            bool followWith = ratio1 >= ratio2;
            Vector2 newSize = new Vector2();
            if (followWith)
            {
                newSize.x = parent.x;
                newSize.y = parent.x / ratio1;
            }
            else
            {
                newSize.x = parent.y * ratio1;
                newSize.y = parent.y;
            }

            return newSize;
        }

        public static void CheckNull(object obj, string name)
        {
            if (obj == null)
            {
                Debug.LogError($"{name} is null");
            }
        }

        public static void DestroyAllChildren(Transform parentTransform)
        {
            foreach (Transform child in parentTransform)
            {
                child.gameObject.SetActive(false);
                GameObject.Destroy(child.gameObject);
            }
        }
        
        public static int[] GenerateRandomPartition(int total, int parts)
        {
            int range = total + parts - 1;

            HashSet<int> cuts = new HashSet<int>();
            while (cuts.Count < parts - 1)
            {
                if (range < 0)
                {
                    Debug.LogError($"range: {range}, total: {total}, parts: {parts}");
                }
                cuts.Add(rng.Next(0, range));
            }

            List<int> sortedCuts = new List<int>(cuts);
            sortedCuts.Sort();
            sortedCuts.Insert(0, -1);
            sortedCuts.Add(range);

            int[] result = new int[parts];
            for (int i = 0; i < parts; i++)
            {
                result[i] = sortedCuts[i + 1] - sortedCuts[i] - 1;
            }

            return result;
        }
        
        public static int[] GenerateRandomPartitionWithMinMax(int total, int parts, int min, int max)
        {
            if (parts * min > total || parts * max < total)
            {
                Debug.LogError($"Impossible to partition total with given min and max per part. total: {total}, parts: {parts}, min: {min}, max: {max}");
                return null;
            }

            int[] result = new int[parts];
            int remaining = total - (min * parts);

            // Khởi tạo tất cả phần tử với giá trị min
            for (int i = 0; i < parts; i++)
            {
                result[i] = min;
            }

            // Phân phối remaining ngẫu nhiên vào các phần tử, không vượt quá max
            while (remaining > 0)
            {
                int index = rng.Next(parts);
                int maxAdd = Math.Min(max - result[index], remaining);
                if (maxAdd > 0)
                {
                    int add = rng.Next(1, maxAdd + 1);
                    result[index] += add;
                    remaining -= add;
                }
            }

            return result;
        }

        public static Vector2 ConvertWorldToCanvasPosition(Transform target, RectTransform uiElement, RectTransform canvasRectTransform, Camera camera)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position);
            Vector2 localPos;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRectTransform,
                screenPos,
                camera, // nếu canvas là Screen Space - Overlay
                out localPos
            );

            return localPos;
        }

        /// <summary>
        /// Converts a UI element position to a world position for a 3D object in the gameplay camera.
        /// Works with orthographic cameras.
        /// </summary>
        /// <param name="uiElement">The UI RectTransform to match position with</param>
        /// <param name="uiCamera">The camera rendering the UI</param>
        /// <param name="gameplayCamera">The camera that will see the 3D object</param>
        /// <param name="worldY">The desired Y position in world space (default: 2)</param>
        /// <returns>World position where the 3D object should be placed</returns>
        // public static Vector3 ConvertUIPositionToWorldPosition(RectTransform uiElement, Camera uiCamera, Camera gameplayCamera, float worldY = 2f)
        // {
        //     // Get screen position of UI element
        //     Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(uiCamera, uiElement.position);
        //
        //     // Convert screen position to world position
        //     // For orthographic camera, we need to calculate the distance from camera
        //     float distanceFromCamera = Mathf.Abs(gameplayCamera.transform.position.z);
        //     screenPos.z = distanceFromCamera;
        //
        //     Vector3 worldPos = gameplayCamera.ScreenToWorldPoint(screenPos);
        //
        //     // Override the Y position to the desired value
        //     worldPos.y = worldY;
        //
        //     return worldPos;
        // }
        
        public static Vector3 ConvertUIPositionToWorldPosition(
            Transform uiElement,
            Camera uiCamera,
            Camera gameplayCamera,
            float worldY = 1f)
        {
            // Lấy vị trí trung tâm UI element trong screen space
            Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(uiCamera, uiElement.position);

            // Bắn ray từ camera gameplay qua screen point
            Ray ray = gameplayCamera.ScreenPointToRay(screenPos);

            // Tính mặt phẳng song song với XZ tại Y = worldY
            Plane plane = new Plane(Vector3.up, new Vector3(0, worldY, 0));

            if (plane.Raycast(ray, out float enter))
            {
                return ray.GetPoint(enter);
            }

            // fallback nếu fail
            return Vector3.zero;
        }


        private static Vector3 alignPosition;
        public static Vector3 AlignPosition(this Vector3 position)
        {
            alignPosition.x = Mathf.Round(position.x);
            alignPosition.y = position.y;
            alignPosition.z = Mathf.Round(position.z);
            return alignPosition;
        }
        
        public static float GetAngleBetweenVectors(Vector3 v1, Vector3 v2)
        {
            v1.y = 0;
            v2.y = 0;
            float dotProduct = Vector3.Dot(v1, v2);
            float magnitudeV1 = v1.magnitude;
            float magnitudeV2 = v2.magnitude;
            float cosTheta = dotProduct / (magnitudeV1 * magnitudeV2);
            cosTheta = Mathf.Clamp(cosTheta, -1f, 1f);
            float angleInRadians = Mathf.Acos(cosTheta);
            float angleInDegrees = Mathf.Rad2Deg * angleInRadians;
            return angleInDegrees;
        }
        
        public static Vector3 WithYZero(this Vector3 v)
        {
            return new Vector3(v.x, 0f, v.z);
        }

        /// <summary>
        /// dựa theo vector A để biết vector B có xu hướng di chuyển như nào
        /// </summary>
        /// <param name="vectorA"></param>
        /// <param name="vectorB"></param>
        /// <returns></returns>
        public static RelativeDirection GetRelativeDirection(Vector3 vectorA, Vector3 vectorB)
        {
            vectorA = vectorA.WithYZero().normalized;
            vectorB = vectorB.WithYZero().normalized;
            
            float angle = Vector3.Angle(vectorA, vectorB);

            if (angle <= 45f)
            {
                Logger.Log($"<color=yellow>Direction: Forward</color>");
                return RelativeDirection.Forward;
            }

            if (angle >= 135f)
            {
                Logger.Log($"<color=yellow>Direction: Backward</color>");
                return RelativeDirection.Backward;
            }
            
            float crossProductY = vectorA.x * vectorB.z - vectorA.z * vectorB.x;
            if (crossProductY > 0)
            {
                Logger.Log($"<color=yellow>Direction: Left</color>");
                return RelativeDirection.Left;
            }
            else
            {
                Logger.Log($"<color=yellow>Direction: Right</color>");
                return RelativeDirection.Right;
            }
        }

        public static Vector3 GetRelativeDirection(this Vector3 vectorA, RelativeDirection direction)
        {
            vectorA = vectorA.WithYZero().normalized;

            if (vectorA == Vector3.zero)
            {
                vectorA = Vector3.forward;
            }

            Vector3 resultVector = Vector3.zero;

            switch (direction)
            {
                case RelativeDirection.Forward:
                    resultVector = vectorA;
                    break;

                case RelativeDirection.Backward:
                    resultVector = -vectorA;
                    break;

                case RelativeDirection.Left:
                    /// quay y một góc θ
                    /// x' = x*cosθ + z*sinθ
                    /// z' = -x*sinθ + z*cosθ
                    /// y' = y
                    resultVector = new Vector3(-vectorA.z, 0, vectorA.x);
                    break;

                case RelativeDirection.Right:
                    /// quay y một góc θ
                    /// x' = x*cosθ + z*sinθ
                    /// z' = -x*sinθ + z*cosθ
                    /// y' = y
                    resultVector = new Vector3(vectorA.z, 0, -vectorA.x);
                    break;
            }

            return resultVector.normalized;
        }

        public static Vector3 GetPerpendicularProjection(this Vector3 vectorA , Vector3 vectorB)
        {
            // Handle edge case where vectorA is zero
            if (vectorA.sqrMagnitude < 0.0001f)
            {
                return vectorB;
            }

            // Calculate the projection of B onto A
            // projA(B) = ((B · A) / (A · A)) * A
            float dotProduct = Vector3.Dot(vectorB, vectorA);
            float magnitudeSquared = Vector3.Dot(vectorA, vectorA);
            Vector3 projectionOntoA = (dotProduct / magnitudeSquared) * vectorA;

            // Calculate the perpendicular component
            // perpA(B) = B - projA(B)
            Vector3 perpendicularComponent = vectorB - projectionOntoA;

            return projectionOntoA;
        }
        
        public static Vector3 GetLocalPosition(Vector3 A_World_Position, Vector3 A_Forward_Direction, Vector3 A_Up_Direction, Vector3 A_Right_Direction, Vector3 B_World_Position)
        {
            Vector3 AB = B_World_Position - A_World_Position;
            float localX = Vector3.Dot(AB, A_Right_Direction);
            float localY = Vector3.Dot(AB, A_Up_Direction);
            float localZ = Vector3.Dot(AB, A_Forward_Direction);

            return new Vector3(localX, localY, localZ);
        }
        
        public static float InterpolateValueFromPosition(Vector3 rootWoldPosition,
            Vector3 rootPositionForwardDirection,
            Vector3 rootPositionUpDirection,
            Vector3 rootPositionRightDirection, Vector3 target)
        {
            var positionObj2OntoObj = GetLocalPosition(rootWoldPosition,
                rootPositionForwardDirection,
                rootPositionUpDirection,
                rootPositionRightDirection, target);
            // Debug.Log($"rootWoldPosition: {rootWoldPosition}, " +
            //           $"rootPositionForwardDirection: {rootPositionForwardDirection}, " +
            //           $"rootPositionUpDirection: {rootPositionUpDirection}, " +
            //           $"rootPositionRightDirection: {rootPositionRightDirection}, " +
            //           $"target: {target}, " +
            //           $"positionObj2OntoObj: " + positionObj2OntoObj);
            return Mathf.Clamp(positionObj2OntoObj.z + Mathf.Clamp((positionObj2OntoObj.z) / 0.5f, 0, 1) * Mathf.Abs(positionObj2OntoObj.x), 0, 1);
        }

        public static (bool hasHit, RaycastHit hitInfo) SphereCast(Vector3 origin, float radius, Vector3 direction, float maxDistance, int layerMask = -1)
        {
            RaycastHit hit;
            bool hasHit = Physics.SphereCast(origin, radius, direction, out hit, maxDistance, layerMask);
            return (hasHit, hit);
        }

        public static (bool hasHit, RaycastHit hitInfo) SphereCast(Ray ray, float radius, float maxDistance, int layerMask = -1)
        {
            RaycastHit hit;
            bool hasHit = Physics.SphereCast(ray, radius, out hit, maxDistance, layerMask);
            return (hasHit, hit);
        }

        public static void DrawDebugWireSphere(Vector3 position, float radius, Color color, float duration)
        {
            int segments = 16;
            float angleStep = 360f / segments;

            // Draw horizontal circle
            for (int i = 0; i < segments; i++)
            {
                float angle1 = i * angleStep * Mathf.Deg2Rad;
                float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;

                Vector3 point1 = position + new Vector3(Mathf.Cos(angle1) * radius, 0, Mathf.Sin(angle1) * radius);
                Vector3 point2 = position + new Vector3(Mathf.Cos(angle2) * radius, 0, Mathf.Sin(angle2) * radius);

                Debug.DrawLine(point1, point2, color, duration);
            }

            // Draw vertical circle (X-Y plane)
            for (int i = 0; i < segments; i++)
            {
                float angle1 = i * angleStep * Mathf.Deg2Rad;
                float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;

                Vector3 point1 = position + new Vector3(Mathf.Cos(angle1) * radius, Mathf.Sin(angle1) * radius, 0);
                Vector3 point2 = position + new Vector3(Mathf.Cos(angle2) * radius, Mathf.Sin(angle2) * radius, 0);

                Debug.DrawLine(point1, point2, color, duration);
            }

            // Draw vertical circle (Z-Y plane)
            for (int i = 0; i < segments; i++)
            {
                float angle1 = i * angleStep * Mathf.Deg2Rad;
                float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;

                Vector3 point1 = position + new Vector3(0, Mathf.Sin(angle1) * radius, Mathf.Cos(angle1) * radius);
                Vector3 point2 = position + new Vector3(0, Mathf.Sin(angle2) * radius, Mathf.Cos(angle2) * radius);

                Debug.DrawLine(point1, point2, color, duration);
            }
        }

        public static void DrawSphereCast(Vector3 origin, float radius, Vector3 direction, float distance, bool hasHit,
            RaycastHit hit, Color color, float duration = 0.1f)
        {
            // Draw the ray line
            Debug.DrawRay(origin, direction * distance, color, duration);

            // Draw sphere at origin
            DrawDebugWireSphere(origin, radius, color, duration);

            // Draw sphere at hit point or max distance
            Vector3 endPosition = hasHit ? origin + direction * hit.distance : origin + direction * distance;
            DrawDebugWireSphere(endPosition, radius, color, duration);

            // Draw connecting lines between spheres (cylinder outline)
            Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;
            if (perpendicular == Vector3.zero)
                perpendicular = Vector3.Cross(direction, Vector3.right).normalized;

            Vector3 offset1 = perpendicular * radius;
            Vector3 offset2 = -perpendicular * radius;

            Debug.DrawLine(origin + offset1, endPosition + offset1, color, duration);
            Debug.DrawLine(origin + offset2, endPosition + offset2, color, duration);

            // Draw hit point indicator if there was a hit
            if (hasHit)
            {
                Debug.DrawRay(hit.point, hit.normal * 0.3f, Color.magenta, duration);
                // Draw a small X at hit point
                float crossSize = 0.1f;
                Debug.DrawLine(hit.point - Vector3.right * crossSize, hit.point + Vector3.right * crossSize,
                    Color.magenta, duration);
                Debug.DrawLine(hit.point - Vector3.forward * crossSize, hit.point + Vector3.forward * crossSize,
                    Color.magenta, duration);
            }
        }
        
        public static Vector3 MoveTowards(Vector3 current, Vector3 target, float maxDistanceDelta, UnityAction actionUponArrival)
        {
            float num1 = target.x - current.x;
            float num2 = target.y - current.y;
            float num3 = target.z - current.z;
            float d = (float) ((double) num1 * (double) num1 + (double) num2 * (double) num2 + (double) num3 * (double) num3);
            if ((double)d == 0.0 || (double)maxDistanceDelta >= 0.0 &&
                (double)d <= (double)maxDistanceDelta * (double)maxDistanceDelta)
            {
                actionUponArrival?.Invoke();
                return target;
            }
            float num4 = (float) Math.Sqrt((double) d);
            return new Vector3(current.x + num1 / num4 * maxDistanceDelta, current.y + num2 / num4 * maxDistanceDelta, current.z + num3 / num4 * maxDistanceDelta);
        }
    }
}

public enum RelativeDirection
{
    None = 0,
    Forward = 1,
    Backward = 2,
    Left = 3,
    Right = 4,
}