﻿namespace VisualDesigner;

public class Property
{
	public string Name;
	public PropertyType Type;
    public Func<object> OnGetValue;
    public Action<object> OnSetValue;
    public object? Parameters;
    public Func<bool>? IsAvailable;
    public string UnavailableText;

    public Property(string Name, PropertyType Type, Func<object> OnGetValue, Action<object> OnSetValue, object? Parameters = null, Func<bool>? IsAvailable = null, string UnavailableText = "")
	{
		this.Name = Name;
		this.Type = Type;
		this.OnGetValue = OnGetValue;
		this.OnSetValue = OnSetValue;
        this.Parameters = Parameters;
        this.IsAvailable = IsAvailable;
        this.UnavailableText = UnavailableText;
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
    Color,
    List
}