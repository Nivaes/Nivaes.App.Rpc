using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using MemoryPack;
using Nivaes.App.Rpc;

namespace Nivaes.App.Rpc.Sample;

public abstract partial class DataModel
    : INotifyPropertyChanged, IRpcDataModel
{
    public abstract Guid Id { get; }

    #region DateTimeStamp
    [MemoryPackIgnore]
    public DateTime DateTimeStamp
    {
        get => new DateTime(DateTimeStampTicks, DateTimeKind.Utc);
        set => DateTimeStampTicks = value.Ticks;
    }

    public long DateTimeStampTicks
    {
        get => field == 0 ? DateTime.Now.Ticks : field;
        set => field = value;
    }
    #endregion

    #region DeleteDateTimeStamp
    [MemoryPackIgnore]
    public DateTime? DeleteDateTimeStamp
    {
        get => DeleteDateTimeStampTicks.HasValue
            ? new(DeleteDateTimeStampTicks.Value, DateTimeKind.Utc)
            : null;
        set => DeleteDateTimeStampTicks = value?.Ticks;
    }

    [MemoryPackInclude]
    public long? DeleteDateTimeStampTicks { get; set; }
    #endregion

    #region INotifyPropertyChanged
    private event PropertyChangedEventHandler? _propertyChanged;

    public event PropertyChangedEventHandler? PropertyChanged
    {
        add => _propertyChanged += value;
        remove => _propertyChanged -= value;
    }

    protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = "")
    {
        RaisePropertyChanged(new PropertyChangedEventArgs(propertyName));
    }

    protected void RaisePropertyChanged(PropertyChangedEventArgs e)
    {
        _propertyChanged?.Invoke(this, e);
    }

    protected bool SetProperty<T>(ref T property, T newValue, [CallerMemberName] string propertyName = "")
    {
        if (object.Equals((object?)property, (object?)newValue))
        {
            return false;
        }
        else
        {
            property = newValue;

            RaisePropertyChanged(new PropertyChangedEventArgs(propertyName));

            return true;
        }
    }
    #endregion
}
