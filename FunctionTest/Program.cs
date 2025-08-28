using System.Collections.Concurrent;
using Bakend.Module.DataBaseModule;
using ConfigManager;
using EventModule;
using Module;

namespace FunctionTest;

public static class Program
{
    public static void Main(string[] args)
    {
        //ModuleCenter.SCreateModule<ConfigModule>();
        ModuleCenter.InitModules<ConfigModule>();

        Logger.Log($"{ConfigModule.Setting.Port}");
        Logger.Log($"{ConfigModule.Setting.PostgresDbPassword}");
        Logger.Log($"{ConfigModule.Setting.RedisPassword}");
        ModuleCenter.InitModules<DataBaseModule>();
        //  ConfigModule.Instance.Save();

    }


}

public class Core
{
    public interface ITaskData
    {

    }

    public class TaskData : IEvent, ITaskData
    {
        public string? Key { get; set; }
        public object? Value { get; set; }
        public int RetryCount { get; set; } = 0;

        // 支持同步或异步逻辑
        public Func<Task>? AsyncAction { get; set; }
        public Action? SyncAction { get; set; }

        // 用于通知调用方任务完成
        internal TaskCompletionSource<bool>? Tcs { get; set; }
    }

    public class BgProcessor : BaseModule, IEventListener<TaskData>
    {

        #region Module

        public static BgProcessor? API { get; private set; }

        protected override void OnPreInit()
        {
            base.OnPreInit();
            API = ModuleCenter.SGetModule<BgProcessor>();
        }

        protected override void OnInit()
        {
            base.OnInit();
            EventModule.EventModule.SRegisterListener(this);
        }

        protected override void UnInit()
        {
            base.UnInit();
            Stop();
            IsInitialized = false;

        }

        public static void StartWorker(int workerCount)
        {
            API?.Start(workerCount);
        }


        public static void EnqueueS(TaskData taskData)
        {
            API?.EnqueueSync(taskData);
        }

        public static Task? EnqueueAs(TaskData taskData)
        {
            return API?.EnqueueAsync(taskData);
        }

        #endregion

        private readonly ConcurrentQueue<TaskData> _tasksQueue = new();
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly SemaphoreSlim _signal = new(0);
        private bool _isRunning = false;

        public Task EnqueueAsync(TaskData taskData)
        {
            taskData.Tcs ??= new TaskCompletionSource<bool>();
            _tasksQueue.Enqueue(taskData);
            _signal.Release();
            return taskData.Tcs.Task;
        }

        public void EnqueueSync(TaskData taskData)
        {
            taskData.Tcs ??= new TaskCompletionSource<bool>();
            _tasksQueue.Enqueue(taskData);
            _signal.Release();

            taskData.Tcs.Task.GetAwaiter().GetResult();
        }

        private async Task ProcessTaskAsync(TaskData taskData)
        {
            try
            {
                taskData.SyncAction?.Invoke();

                if (taskData.AsyncAction != null)
                {
                    await taskData.AsyncAction();
                }
                taskData.Tcs?.TrySetResult(true);
            }
            catch (Exception ex)
            {
                Logger.LogError($"[Task {taskData.Key}] Failed: {ex}");
                if (taskData.RetryCount > 0)
                {
                    taskData.RetryCount--;
                    _tasksQueue.Enqueue(taskData);
                    _signal.Release();
                }
                else
                {
                    taskData.Tcs?.TrySetException(ex);
                }
            }
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            _isRunning = false;
        }

        public void Start(int workerCount)
        {
            if (_isRunning)
            {
                return;
            }
            _isRunning = true;

            for (int i = 0; i < workerCount; i++)
            {
                Task.Run(async () =>
                {
                    while (!_cancellationTokenSource.IsCancellationRequested)
                    {
                        await _signal.WaitAsync(_cancellationTokenSource.Token);
                        if (_tasksQueue.TryDequeue(out var task))
                        {
                            await ProcessTaskAsync(task);
                        }
                    }
                }, _cancellationTokenSource.Token);
            }

        }

        public void OnEvent(TaskData evt, params object[] args)
        {
            if (evt.AsyncAction != null)
            {
                EnqueueSync(evt);
            }
            else
            {
                EnqueueAsync(evt).Wait();
            }
        }
    }
}