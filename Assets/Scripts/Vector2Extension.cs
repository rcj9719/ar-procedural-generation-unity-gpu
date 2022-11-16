using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class Vector2Extension
{
    // Ref link: Dilmer Valecillos - https://www.youtube.com/watch?v=NdrvihZhVqs
    public static bool IsPointOverUIObject(this Vector2 touchPosition)
    {
        if (EventSystem.current.IsPointerOverGameObject()) { 
            return false; 
        }

        PointerEventData eventPosition = new PointerEventData(EventSystem.current);
        eventPosition.position = new Vector2(touchPosition.x, touchPosition.y);

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventPosition, results);

        return results.Count > 0;
    }
}
