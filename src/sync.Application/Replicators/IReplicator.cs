namespace sync.Application.Replicators;

public interface IReplicator
{
    public IAddedItemsReplicator GetAddedItemsReplicator();
    public IModifiedItemsReplicator GetModifiedItemsReplicator();
    public IObsoleteItemsCleaner GetObsoleteItemsCleaner();
}
