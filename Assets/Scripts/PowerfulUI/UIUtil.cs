using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PowerfulUI
{
    public static class UIUtil
    {
        public static Vector2 GetPoinsterPosition(int pointerID)
        {
            switch (pointerID)
            {
                case -1:
                case -2:
                case -3:
                    return Input.mousePosition;
                default:
                    {
                        for (int i = 0; i < Input.touchCount; i ++)
                        {
                            var touch = Input.touches[i];
                            if (touch.fingerId == pointerID)
                            {
                                return touch.position;
                            }
                        }
                    }
                    break;
            }
            return Vector2.zero;
        }
        
        
        public static bool SetStruct<T>(ref T currentValue, T newValue) where T : struct
        {
            if (EqualityComparer<T>.Default.Equals(currentValue, newValue))
                return false;

            currentValue = newValue;
            return true;
        }
        
        public static bool SetClass<T>(ref T currentValue, T newValue) where T : class
        {
            if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
                return false;

            currentValue = newValue;
            return true;
        }
    }
}
