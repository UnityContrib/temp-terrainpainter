using System;

public static class ArrayEx
{
    public static T[] Remove<T>(this T[] source, int index)
    {
        var destination = new T[source.Length - 1];
        Array.Copy(source, 0, destination, 0, index);
        Array.Copy(source, index + 1, destination, index, destination.Length - index);
        return destination;
    }

    public static T[] Insert<T>(this T[] source, int index, T element)
    {
        var destination = new T[source.Length + 1];
        Array.Copy(source, 0, destination, 0, index);
        Array.Copy(source, index, destination, index + 1, source.Length - index);
        destination[index] = element;
        return destination;
    }

    public static void ForEach<T>(this T[] source, Action<T> action)
    {
        Array.ForEach(source, action);
    }
}
