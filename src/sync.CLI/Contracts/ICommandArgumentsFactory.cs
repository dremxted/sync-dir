using System.CommandLine;

namespace sync.CLI.Contracts;

public interface ICommandArgumentsFactory<TCommand>
    where TCommand : Command
{
    public IEnumerable<Argument> Create();
}
