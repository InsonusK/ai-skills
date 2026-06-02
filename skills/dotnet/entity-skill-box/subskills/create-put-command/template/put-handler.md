---
name: entity-put-handler-template
description: Template of post handler
metadata:
  domain: dotnet
  tags:
    - dotnet
    - skill-box
    - put-command
    - put-handler
---
# Template of post handler

```C#
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
using {ProjectNamespace}.Entities.{EntityName}Entity.Commands;
using {ProjectNamespace}.Entities.{EntityName}Entity.Configs;
using {ProjectNamespace}.Entities.{EntityName}Entity.Models;

namespace {ProjectNamespace}.Entities.{EntityName}Entity.Handlers;

public class {EntityName}PutHandler : 
    IRequestHandler<{EntityName}PutCommand, Result<{EntityName}Response.Single>>
{
    private readonly IRepositoryBase<{EntityName}> _repository;
    private readonly IValidator<{EntityName}PutRequestDto> _dtoValidator;
    private readonly IValidator<{EntityName}> _entityValidator;
    private readonly ILogger<{EntityName}PutHandler> _logger;
    private readonly ICommandContextSource _commandContextSource;

    public {EntityName}PutHandler(
        IRepositoryBase<{EntityName}> repository,
        IValidator<{EntityName}PutRequestDto> dtoValidator,
        IValidator<{EntityName}> entityValidator,
        ILogger<{EntityName}PutHandler> logger,
        ICommandContextSource commandContextSource)
    {
        _repository = repository;
        _dtoValidator = dtoValidator;
        _entityValidator = entityValidator;
        _logger = logger;
        _commandContextSource = commandContextSource;
    }
    
    public async Task<Result<{EntityName}Response.Single>> Handle({EntityName}PutCommand request, CancellationToken cancellationToken)
    {
        await _dtoValidator.ValidateAndThrowOnErrorsAsync(request.Payload, cancellationToken);
        
        var entity = await _commandContextSource.GetForAsync<{EntityName}>(request, cancellationToken);

        request.Payload.UpdateEntity(entity, request.ActionTimeStamp);

        var entityValidation = await _entityValidator.ValidateAndThrowOnErrorsAsync(entity, cancellationToken);

        await _repository.UpdateAsync(entity, cancellationToken);
        _logger.LogInformation("Updated {EntityName} with Id {Id} and Guid {Guid}", entity.Id, entity.Guid);
        
        var response = {EntityName}Response.Single.FromEntity(entity, entityValidation);
        return Result.Success(response);
    }

}
```