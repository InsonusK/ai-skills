using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sample.Domain.Entities;

namespace Sample.Domain.Configurations;

public sealed class AttachmentConfig : IEntityTypeConfiguration<Attachment>
{
    public const string TableName = "Attachments";
    public const string UX_Guid = "UX_Attachment_Guid";

    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.ToTable(TableName);
        builder.HasKey(a => a.Id);

        builder.HasIndex(a => a.Guid).IsUnique().HasDatabaseName(UX_Guid);

        builder.Property(a => a.UserCreatedDateTime).IsRequired();
        builder.Property(a => a.ServerCreatedDateTime).IsRequired();
    }
}
