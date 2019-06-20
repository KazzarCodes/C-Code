public abstract class PersistableViewModelBase : ViewModelBase, IPersistableViewModel
{
    protected virtual ICommandModel GetCommandModel() => null;
    
    /// <summary>
    /// Validates user input and executes the persistence command asynchronously.
    /// </summary>
    /// <returns></returns>
    public virtual async Task<bool> Persist(Func<bool> prePersistAction = null)
    {
        try
        {
            IsPersisting = true;

            if (!OnBeforePersist())
                return false;

            if (prePersistAction != null)
            {
                var prePersistResult = prePersistAction();

                if (!prePersistResult)
                    return false;
            }

            var commandModel = GetCommandModel();

            var doPersistResult = await DoWork(() =>
            {
                if (commandModel == null)
                    throw new Exception("Command model cannot be null");

                var validateResult = OnValidate();
                if (validateResult.Failed)
                    throw new SyncException(validateResult.Message, FailureSeverityType.Low);

                ExecuteCommand(commandModel);
            });

            OnPersistCompleted(doPersistResult, commandModel.Result);

            return doPersistResult;
        }
        finally
        {
            IsPersisting = false;
        }
    }

    /// <summary>
    /// Executes the command against the API.
    /// </summary>
    /// <param name="commandModel"></param>
    protected virtual void ExecuteCommand(ICommandModel commandModel)
    {
        SyncProxy.Instance.ExecuteCommand(commandModel);
    }
}
