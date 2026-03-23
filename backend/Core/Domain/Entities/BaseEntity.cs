using System;

namespace Domain.Entities;

public class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ModifiedAt { get; private set; }

    public BaseEntity()
    {
        Id = Guid.NewGuid();
    }

    public void SetCreatedAt(DateTime createdAt)
    {
        CreatedAt = createdAt;
    }

    public void SetModifiedAt(DateTime modifiedAt)
    {
        ModifiedAt = modifiedAt;
    }
}
