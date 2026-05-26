```csharp
using InsonusK.Shared.AnnotationsForDI;
using Ardalis.Result;
using InsonusK.Shared.DataBase.Spec;
using Ardalis.Specification;
using InsonusK.Shared.Command.Service.Interface;
using InsonusK.Shared.Command.Interface;
using InsonusK.Shared.Command.Exception;

// TODO: adjust namespace to match your project structure
namespace {ProjectNamespace}.Entities.{EntityName}Entity.Services;

[Service(interfaces: typeof(IEntityCommandExtractor<{EntityName}>))]
public class {EntityName}CommandExtractor: IEntityCommandExtractor<{EntityName}>
{
    private readonly IReadRepositoryBase<{EntityName}> _{entityNamecamelCase}Repository;

    public {EntityName}CommandExtractor(IReadRepositoryBase<{EntityName}> {entityNamecamelCase}Repository)
    {
        _{entityNamecamelCase}Repository = {entityNamecamelCase}Repository;
    }

    public async Task<{EntityName}> GetAsync(IEntityKey entityKey, CancellationToken cancellationToken = default)
    {
        var taskTag = await this.TryGetAsync(entityKey, cancellationToken);
        if (taskTag is null)
            throw new ResultException(Result.NotFound("{EntityName} Id not found"));

        return taskTag;
    }

    public async Task<{EntityName}?> TryGetAsync(IEntityKey entityKey, CancellationToken cancellationToken = default)
    {
        // TODO: adjust ID type based on the entity type:
        // - if entity is IGuidModel => use ByStringIdSpec
        // - if entity is not IGuidModel and is not Composite => use getById (e.g. _repository.GetByIdAsync(entityKey.IntId))
        // - if entity Is Composite => use custom logic to get entity
        var spec = new ByStringIdSpec<{EntityName}>(entityKey.StringId, true);

        return await _{entityNamecamelCase}Repository.FirstOrDefaultAsync(spec, cancellationToken);
    }
}
```
