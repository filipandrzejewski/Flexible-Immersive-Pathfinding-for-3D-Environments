using UnityEngine;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    public static UnityMainThreadDispatcher Instance { get; private set; }
    private ConcurrentQueue<Action> actions = new ConcurrentQueue<Action>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        while (actions.TryDequeue(out var action))
        {
            action.Invoke();
        }
    }

    public async Task Enqueue(Action action)
    {
        actions.Enqueue(action);
        await Task.CompletedTask;
    }
}
