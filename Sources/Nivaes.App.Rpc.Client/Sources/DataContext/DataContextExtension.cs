using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Nivaes.App.Rpc.Client;

public static class DataContextExtension
{
    //public static async Task UpdateItemAsync(this DbContext db, object entity)
    //{
    //    var entry = db.Entry(entity);
    //    var entityType = entry.Metadata;

    //    var primaryKey = entityType.FindPrimaryKey();

    //    if (primaryKey == null)
    //        throw new InvalidOperationException($"The entity {entity.GetType().FullName} doesn't have a primary key. ");

    //    var keyValues = primaryKey.Properties.Select(p => p.PropertyInfo!.GetValue(entity)).ToArray();

    //    var existing = await db.FindAsync(entityType.ClrType, keyValues);

    //    if (existing == null)
    //        entry.State = EntityState.Added;
    //    else
    //        entry.State = EntityState.Modified;
    //}

    public static async Task UpdateItemAsync(this DbContext db, object entity)
    {
        var entityType = db.Model.FindEntityType(entity.GetType())
                ?? throw new InvalidOperationException($"The entity {entity.GetType().FullName} is not mapped.");

        var primaryKey = entityType.FindPrimaryKey()
            ?? throw new InvalidOperationException($"The entity {entity.GetType().FullName} doesn't have a primary key.");

        var keyValues = primaryKey.Properties
            .Select(p => p.PropertyInfo!.GetValue(entity))
            .ToArray();

        var existing = await db.FindAsync(
            entityType.ClrType,
            keyValues);

        if (existing == null)
        {
            db.Add(entity);
        }
        else
        {
            db.Entry(existing).CurrentValues.SetValues(entity);
        }

    }
}
