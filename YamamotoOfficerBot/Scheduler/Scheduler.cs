using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace YamamotoOfficerBot.Scheduler;

public abstract class Scheduler(IServiceScopeFactory serviceScopeFactory)
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private Task? _task;
    private int _started = 0;

    protected abstract int Interval { get; }

    public void Start()
    {
        if (Interlocked.CompareExchange(ref _started, 1, 0) != 0) return;
        _task = Task.Run(async () => await RunAsync(_cancellationTokenSource.Token));
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(Interval, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }

            using var scope = serviceScopeFactory.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());

            try
            {
                await ExecuteTaskAsync(scope);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while executing the task.");
            }
        }
    }

    protected abstract Task ExecuteTaskAsync(IServiceScope scope);

    public async Task Stop()
    {
        await _cancellationTokenSource.CancelAsync();
        if (_task != null) await _task;
        Interlocked.Exchange(ref _started, 0);
    }
}
