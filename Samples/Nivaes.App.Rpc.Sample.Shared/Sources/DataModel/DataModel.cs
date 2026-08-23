using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Nivaes.App.Rpc;

namespace Nivaes.App.RPC.Sample;

public abstract class DataModel
    : INotifyPropertyChanged, IRpcDataModel
{
    public abstract Guid Id { get; }

    #region TimeStamp
    //[MemoryPackIgnore]
    public DateTime TimeStamp
    {
        get => new DateTime(TimeStampTicks, DateTimeKind.Utc);
        set => TimeStampTicks = value.Ticks;
    }

    //[MemoryPackInclude]
    public long TimeStampTicks
    {
        get
        {
            if (field == 0)
                field = DateTime.Now.Ticks;
            return field;
        }
        set => field = value;
    }

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
