---
name: entity-response-dto-template
description: Template of response dto
metadata:
  domain: dotnet
  tags:
    - dotnet
    - skill-box
    - response-dto
    - reponse-dto
---
# Template of response dto

```CSharp
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using InsonusK.Shared.Models.Template;

namespace {ProjectNamespace}.Entities.{EntityName}Entity.Models;

public static class {EntityName}Response
{
    public class Dto : {EntityName}.IBody, {{ response dto interface }}
    {
        // TODO: Add fields from entity
        // If field is required in entity, add [Required] attribute
        // If field is nullable in entity, add ? to type

        protected Dto({EntityName} entity)
        {
            // TODO: Add fields from entity
        }
        public static Dto FromEntity({EntityName} entity) => new Dto(entity);
    }


    public class Single : Dto, IResponseWithValidationInfo
    {
        public IEnumerable<ValidationError> ValidationMessages { get; internal set; } = [];

        private Single({EntityName} entity, FluentValidation.Results.ValidationResult? requestValidationResult = null) : base(entity)
        {
            ValidationMessages = requestValidationResult?.AsErrors() ?? [];
        }
        public static Single FromEntity({EntityName} entity) => FromEntity(entity, null);

        public static Single FromEntity({EntityName} entity, FluentValidation.Results.ValidationResult? requestValidationResult = null)
        {
            return new Single(entity, requestValidationResult);
        }
    }
    public class Many : BulkResponseDto<Dto>
    {
        [SetsRequiredMembers]
        private Many(IEnumerable<{EntityName}> entity, FluentValidation.Results.ValidationResult? requestValidationResult = null)
         : base()
        {
            Items = entity.Select(Dto.FromEntity);
            ValidationMessages = requestValidationResult?.AsErrors() ?? [];
        }

        public static Many FromEntity(IEnumerable<{EntityName}> entity) => FromEntity(entity, null);

        public static Many FromEntity(IEnumerable<{EntityName}> entity, FluentValidation.Results.ValidationResult? requestValidationResult = null)
        {
            return new Many(entity, requestValidationResult);
        }
    }
}
```
