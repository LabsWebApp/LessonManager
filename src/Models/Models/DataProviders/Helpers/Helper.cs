namespace Models.DataProviders.Helpers;

public static class Helper
{
    public static int RejectChanges(this DbContext context, CancellationToken cancellationToken)
    {
        int result = 0;
        if (!cancellationToken.IsCancellationRequested) return result;
        foreach (var entry in context.ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                case EntityState.Modified:
                    entry.State = EntityState.Unchanged;
                    result++;
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Modified; //Revert changes made to deleted entity.
                    entry.State = EntityState.Unchanged;
                    result++;
                    break;
                case EntityState.Added:
                    entry.State = EntityState.Detached;
                    result++;
                    break;
            }
        }
        return result;
    }
}