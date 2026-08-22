namespace Praedora.Application;

// Tracks whether an EmailSyncService.SyncAsync run is currently in progress, regardless of
// whether it was triggered by the timer worker or the "Sync now" button. A lightweight,
// in-memory singleton — this is transient runtime state, not worth persisting across a restart —
// so the Review Queue page can show a live "syncing" indicator instead of relying on how long a
// detached background task takes to return.
public class SyncActivityTracker
{
    private volatile bool isSyncing;

    public bool IsSyncing => isSyncing;

    public void Start()
    {
        isSyncing = true;
    }

    public void Stop()
    {
        isSyncing = false;
    }
}
