using UnityEngine;
using System.Collections;

namespace UI
{
    public static class UIHelpers
    {

        // a helper function to tween a transform position over a specified duration
        public static IEnumerator TweenPosition(Transform transform, Vector3 targetPosition, float duration)
        {
            // save the current position of the transform to animate
            Vector3 previousPosition = transform.position;
            float time = 0.0f;
            do
            {
                time += Time.deltaTime;
                transform.position = Vector3.Lerp(previousPosition, targetPosition, time / duration);
                yield return true;
                //Do the Lerp function while time is less than the move duration.
            } while (time < duration);
            yield return true;
        }

    }
}