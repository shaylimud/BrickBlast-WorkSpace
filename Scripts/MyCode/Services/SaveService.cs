using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ray.Services
{
    public class SaveService : MonoBehaviour
    {
        public static SaveService Instance;

        private readonly Queue<(UserData data, TaskCompletionSource<bool> tcs)> _saveQueue = new();
        private bool _isProcessing = false;

        private void Awake()
        {
            Instance = this;
        }

        public Task Save(UserData saveData)
        {
            var tcs = new TaskCompletionSource<bool>();
            lock (_saveQueue)
            {
                _saveQueue.Enqueue((saveData, tcs));
                if (!_isProcessing)
                {
                    _ = ProcessQueue();
                }
            }
            return tcs.Task;
        }

        private async Task ProcessQueue()
        {
            _isProcessing = true;
            while (true)
            {
                (UserData data, TaskCompletionSource<bool> tcs) item;
                lock (_saveQueue)
                {
                    if (_saveQueue.Count == 0)
                    {
                        _isProcessing = false;
                        return;
                    }
                    item = _saveQueue.Dequeue();
                }
                try
                {
                    await Database.Instance.Save(item.data);
                    item.tcs.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    item.tcs.TrySetException(ex);
                }
            }
        }
    }
}
