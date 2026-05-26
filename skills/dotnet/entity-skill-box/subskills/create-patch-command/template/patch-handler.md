# Template of patch handler

```csharp
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using Ardalis.Specification;
using FluentValidation;
using InsonusK.Shared.DataBase.Handlers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using InsonusK.Shared.Command.Interfaces;
using {ProjectNamespace}.Entities.{EntityName}Entity.Commands;
using {ProjectNamespace}.Entities.{EntityName}Entity.Configs;
using {ProjectNamespace}.Entities.{EntityName}Entity.Models;

namespace {ProjectNamespace}.Entities.{EntityName}Entity.Handlers;

public class {EntityName}PatchHandler : 
    IRequestHandler<{EntityName}PatchCommand, Result<{EntityName}Response.Single>>
{
    private readonly IRepositoryBase<{EntityName}> _repository;
    private readonly IValidator<{EntityName}PatchRequestDto> _dtoValidator;
    private readonly IValidator<{EntityName}> _entityValidator;
    private readonly ILogger<{EntityName}PatchHandler> _logger;
    private readonly ICommandContextSource _commandContextSource;

    public {EntityName}PatchHandler(
        IRepositoryBase<{EntityName}> repository,
        IValidator<{EntityName}PatchRequestDto> dtoValidator,
        IValidator<{EntityName}> entityValidator,
        ILogger<{EntityName}PatchHandler> logger,
        ICommandContextSource commandContextSource)
    {
        _repository = repository;
        _dtoValidator = dtoValidator;
        _entityValidator = entityValidator;
        _logger = logger;
        _commandContextSource = commandContextSource;
    }
    
    public async Task<Result<{EntityName}Response.Single>> Handle({EntityName}PatchCommand request, CancellationToken cancellationToken)
    {
        await _dtoValidator.ValidateAndThrowOnErrorsAsync(request.Payload, cancellationToken);
        
        var entity = await _commandContextSource.GetForAsync<{EntityName}>(request, cancellationToken);

        request.Payload.PatchEntity(entity, request.ActionTimeStamp);

        var entityValidation = await _entityValidator.ValidateAndThrowOnErrorsAsync(entity, cancellationToken);

        await _repository.UpdateAsync(entity, cancellationToken);
        _logger.LogInformation("Updated {EntityName} with Id {Id} and Guid {Guid}", entity.Id, entity.Guid);
        
        var response = {EntityName}Response.Single.FromEntity(entity, entityValidation);
        return Result.Success(response);
    }

}
```
