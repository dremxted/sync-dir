using System.CommandLine;

namespace sync.CLI.Contracts;

public interface ICommandActionHandler<TCommand>
    where TCommand : Command
{
    public Task Handle(
        ParseResult parseResult, 
        CancellationToken cancellationToken);
}
