using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualDesigner;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
sealed class WidgetTypeAndName : Attribute
{
    public Type DataType;
    public string DataName;

    public WidgetTypeAndName(Type DataType, string DataName)
    {
        this.DataType = DataType;
        this.DataName = DataName;
    }
}
