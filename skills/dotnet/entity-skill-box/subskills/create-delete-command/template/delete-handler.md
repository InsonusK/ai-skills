# Template of delete handler

```csharp
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Ardalis.Specification;
using InsonusK.Shared.DataBase.Handlers;
using MediatR;
using Microsoft.Extensions.Logging;
using InsonusK.Shared.Command.Interfaces;
using {ProjectNamespace}.Entities.{EntityName}Entity.Commands;
using {ProjectNamespace}.Entities.{EntityName}Entity.Models;

namespace {ProjectNamespace}.Entities.{EntityName}Entity.Handlers;

public class {EntityName}DeleteHandler : 
    IRequestHandler<{EntityName}DeleteCommand, Result<{EntityName}Response.Single>>
{
    private readonly IRepositoryBase<{EntityName}> _repository;
    private readonly ILogger<{EntityName}DeleteHandler> _logger;
    private readonly ICommandContextSource _commandContextSource;

    public {EntityName}DeleteHandler(
        IRepositoryBase<{EntityName}> repository,
        ILogger<{EntityName}DeleteHandler> logger,
        ICommandContextSource commandContextSource)
    {
        _repository = repository;
        _logger = logger;
        _commandContextSource = commandContextSource;
    }
    
    public async Task<Result<{EntityName}Response.Single>> Handle({EntityName}DeleteCommand request, CancellationToken cancellationToken)
    {
        var entity = await _commandContextSource.GetForAsync<{EntityName}>(request, cancellationToken);
        
        entity.IsDeleted = true;
        entity.UserUpdatedDateTime = request.ActionTimeStamp;

        await _repository.UpdateAsync(entity, cancellationToken);
        
        _logger.LogInformation("Deleted {EntityName} with Id {Id} and Guid {Guid}", entity.Id, entity.Guid);
        
        if (!cmd.Payload.ReturnLastState)
            return Result.NoContent();
            
        var response = {EntityName}Response.Single.FromEntity(entity);
        return Result.Success(response);
    }
}
```
