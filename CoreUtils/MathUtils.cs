using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtils
{
    public static float StepRemap(float min, float max, float a, float b, float value)
    {
        if (a > b)
        {
            throw new ArgumentException("a should be less than or equal to b");
        }
        
        if (value <= a)
        {
            return min;
        }
        
        if (value >= b)
        {
            return max;
        }
        
        float t = (value - a) / (b - a);  
        return Mathf.Lerp(min, max, t);   
    }
}
