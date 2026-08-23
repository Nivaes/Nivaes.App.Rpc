using System;
using System.Collections.Generic;
using System.Text;
using MemoryPack;

namespace Nivaes.App.RPC.Sample;

[MemoryPackable]
public partial class UserDataModel
    : DataModel
{
    public override Guid Id => IdUser;

    public Guid IdUser { get; set; }

    public string? Identification
    {
        get => field;
        set => base.SetProperty(ref field, value);
    }

    public string? Name
    {
        get => field;
        set => base.SetProperty(ref field, value);
    }

    public string? Description
    {
        get => field;
        set => base.SetProperty(ref field, value);
    }

    public string? ProfileAvatar
    {
        get => field;
        set => base.SetProperty(ref field, value);
    }

    public string? GivenName
    {
        get => field;
        set
        {
            if (base.SetProperty(ref field, value))
            {
                base.RaisePropertyChanged(nameof(FullName));
            }
        }
    }

    public string? FamilyName
    {
        get => field;
        set
        {
            if (base.SetProperty(ref field, value))
            {
                base.RaisePropertyChanged(nameof(FullName));
            }
        }
    }

    public string FullName => string.Join(" ", new string[] { GivenName ?? string.Empty, FamilyName ?? string.Empty }.Where(s => !string.IsNullOrEmpty(s)));

    public string Initials => GivenName?.Substring(0, 1) + FamilyName?.Substring(0, 1);

    public string? PhoneNumber
    {
        get => field;
        set => base.SetProperty(ref field, value);
    }
    public string? Email
    {
        get => field;
        set => base.SetProperty(ref field, value);
    }
}
