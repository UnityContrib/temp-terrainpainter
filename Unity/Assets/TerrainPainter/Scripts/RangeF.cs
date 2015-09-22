using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;
using System.Globalization;

[Serializable]
[StructLayout(LayoutKind.Sequential)]
public struct RangeF
{
    public float minimum;
    public float maximum;

    public RangeF(float minimum, float maximum)
    {
        this.minimum = minimum;
        this.maximum = maximum;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is RangeF))
        {
            return false;
        }
        var range = (RangeF)obj;
        return ((range.minimum == this.minimum) && (range.maximum == this.maximum));
    }

    public override string ToString()
    {
        return ("{Minimum=" + this.minimum.ToString(CultureInfo.CurrentCulture) + ",Maximum=" + this.maximum.ToString(CultureInfo.CurrentCulture) + "}");
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public bool Contains(float value)
    {
        return value >= this.minimum && value < this.maximum;
    }
}
