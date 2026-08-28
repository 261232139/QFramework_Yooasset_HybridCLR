using System;
using System.Collections.Generic;

public static class LinkedListExtensions
{
    public static void InsertAt<T>(this LinkedList<T> list, T value, int index)
    {
        if (index < 0 || index > list.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if (index == 0)
        {
            list.AddFirst(value);
            return;
        }

        if (index == list.Count)
        {
            list.AddLast(value);
            return;
        }

        var currentNode = list.First;
        for (int i = 0; i < index; i++)
        {
            currentNode = currentNode.Next;
        }

        list.AddBefore(currentNode, value);
    }
}
