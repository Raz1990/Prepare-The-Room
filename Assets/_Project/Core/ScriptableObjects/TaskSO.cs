using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Task", menuName = "Game/Task")]
public class TaskSO : ScriptableObject
{
    [Header("Task Info")]
    public string taskName; // Headline e.g., "Throw the Trash"

    [Header("Steps")]
    public List<TaskStep> steps = new List<TaskStep>();
}