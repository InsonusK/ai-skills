---
name: entity-post-handler-template
description: Template of post handler
metadata:
  domain: dotnet
  tags:
    - dotnet
    - skill-box
    - post-command
    - post-handler
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

public class {EntityName}PostHandler : 
    IRequestHandler<{EntityName}PostCommand.Single, Result<{EntityName}Response.Single>>,
    IRequestHandler<{EntityName}PostCommand.Many, Result<{EntityName}Response.Many>>
{
    private readonly IRepositoryBase<{EntityName}> _repository;
    private readonly IValidator<{EntityName}PostRequestDto> _dtoValidator;
    private readonly IValidator<{EntityName}> _entityValidator;
    private readonly ILogger<{EntityName}PostHandler> _logger;

    public {EntityName}PostHandler(
        IRepositoryBase<{EntityName}> repository,
        IValidator<{EntityName}PostRequestDto> dtoValidator,
        IValidator<{EntityName}> entityValidator,
        ILogger<{EntityName}PostHandler> logger)
    {
        _repository = repository;
        _dtoValidator = dtoValidator;
        _entityValidator = entityValidator;
        _logger = logger;
    }
    private async Task<({EntityName}, ValidationResult)> DtoToEntity({EntityName}PostRequestDto dto, CancellationToken cancellationToken)
    {
        var validationResult = await _dtoValidator.ValidateAndThrowOnErrorsAsync(dto, cancellationToken);

        var entity = dto.ToNewEntity(request.ActionTimeStamp);
        
        var entityValidation = await _entityValidator.ValidateAndThrowOnErrorsAsync(entity, cancellationToken);
        return (entity, entityValidation);
    }
    
    public async Task<Result<{EntityName}Response.Single>> Handle({EntityName}PostCommand.Single request, CancellationToken cancellationToken)
    {
        var (entity, validationResult) = await DtoToEntity(request.Payload, cancellationToken);
        
        await _repository.AddAsync(entity, cancellationToken);
        _logger.LogInformation("Created new {EntityName} with Id {Id} and Guid {Guid}", entity.Id, entity.Guid);
        
        var response = {EntityName}Response.Single.FromEntity(entity, validationResult);
        return Result.Success(response);
    }

    public async Task<Result<{EntityName}Response.Many>> Handle({EntityName}PostCommand.Many request, CancellationToken cancellationToken)
    {
        var entities = new List<{EntityName}>();
        var validationResults = new List<ValidationResult>();
        foreach (var payload in request.Payload)
        {
            var (entity, validationResult) = await DtoToEntity(payload, cancellationToken);
            entities.Add(entity);   
            validationResults.Add(validationResult);
            _logger.LogInformation("Add new {EntityName} with Guid {Guid}", entity.Guid);
        }

        await _repository.AddRangeAsync(entities, cancellationToken);
        _logger.LogInformation("Created new {EntityName} with Ids {Ids} and Guids {Guids}", entities.Select(x => x.Id), entities.Select(x => x.Guid));

        var combinedValidationResult = new FluentValidation.Results.ValidationResult(validationResults.SelectMany(x => x.Errors));
        var response = {EntityName}Response.Many.FromEntity(entities, combinedValidationResult);
        return Result.Success(response);
    }
}
```