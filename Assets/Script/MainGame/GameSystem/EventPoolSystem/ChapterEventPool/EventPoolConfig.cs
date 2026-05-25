using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EventPoolConfig", menuName = "Scriptable Objects/EventPoolConfig")]
public class EventPoolConfig : ScriptableObject
{
    public List<RandomEventData> allEvents;
}
