using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualDesigner;

public class Property
{
	public string Name;
	public PropertyType Type;
    public Func<object> OnGetValue;
    public Action<object> OnSetValue;
    public object? Parameters;
    public Func<bool>? IsAvailable;

    public Property(string Name, PropertyType Type, Func<object> OnGetValue, Action<object> OnSetValue, object? Parameters = null, Func<bool>? IsAvailable = null)
	{
		this.Name = Name;
		this.Type = Type;
		this.OnGetValue = OnGetValue;
		this.OnSetValue = OnSetValue;
        this.Parameters = Parameters;
        this.IsAvailable = IsAvailable;
	}

    public object GetValue()
    {
        return OnGetValue();
    }

    public void SetValue(object Value)
    {
        OnSetValue.Invoke(Value);
    }
}

public enum PropertyType
{
    Text,
    Numeric,
    Dropdown,
    Font,
    Padding,
    Boolean,
    Color
}