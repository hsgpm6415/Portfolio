using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SaveResistry
{
    static readonly Dictionary<string, ISaveable> m_map = new();
    public static void Resistry(ISaveable s) => m_map[s.SaveId] = s;
    public static void Unresistry(ISaveable s) => m_map.Remove(s.SaveId);
    public static bool TryGet(string id, out ISaveable s) => m_map.TryGetValue(id, out s);
    public static IEnumerable<ISaveable> All => m_map.Values;
}
