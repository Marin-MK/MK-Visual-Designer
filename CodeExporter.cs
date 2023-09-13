using System.Text;

namespace VisualDesigner;

public class CodeExporter
{
	List<string> Dependencies;
	StringBuilder SB;
	WindowData Window;
	List<WidgetData> Widgets;
	WidgetData? CurrentWidget;

	public CodeExporter()
	{
		Dependencies = new List<string>() { "System" };
		SB = new StringBuilder();
		Widgets = new List<WidgetData>();
	}

	private void AddDependency(string Dependency)
	{
		if (!Dependencies.Contains(Dependency))
		{
			Dependencies.Add(Dependency);
		}
	}

	private void AddAllWidgetDependencies()
	{
		Widgets.ForEach(w =>
		{
			if (w.Dependencies.Length > 0)
			{
				foreach (string dp in w.Dependencies) AddDependency(dp);
			}
		});
	}

	private void WriteDependencies()
	{
		Dependencies.ForEach(dp =>
		{
			SB.AppendLine($"using {dp};");
		});
		SB.AppendLine();
	}

	private void WriteNamespace()
	{
		SB.AppendLine("namespace RPGStudioMK.Widgets;");
		SB.AppendLine();
	}

	private void WriteClassOpen()
	{
		string Parent = Window.IsPopup ? "PopupWindow" : "Widget";
		SB.AppendLine($"public class {Window.Name} : {Parent}");
		SB.AppendLine("{");
	}

	private void WriteClassClose()
	{
		SB.AppendLine("}");
	}

	private void WriteApplyVariable()
	{
		if (!Window.IsPopup) return;
		SB.AppendLine($"\tpublic bool Apply = false;");
		SB.AppendLine();
	}

	private void WriteWidgetDefinitions()
	{
		if (Widgets.Count == 0) return;
		Widgets.ForEach(w =>
		{
			SB.AppendLine($"\t{w.ClassName} {w.Name};");
		});
		SB.AppendLine();
	}

	private void WriteConstructorOpen()
	{
		if (Window.IsPopup) SB.AppendLine($"\tpublic {Window.Name}()");
		else SB.AppendLine($"\tpublic {Window.Name}(IContainer Parent) : base(Parent)");
		SB.AppendLine("\t{");
	}

	private void WriteConstructorBody()
	{
		if (Window.IsPopup)
		{
			SB.AppendLine($"\t\tSetTitle(\"{Window.Title}\");");
			if (Window.Fullscreen) SB.AppendLine("\t\tSetDocked(true);");
			else
			{
				SB.AppendLine($"\t\tMinimumSize = MaximumSize = new Size({Window.Size.Width - DesignWidget.WidthAdd}, {Window.Size.Height - DesignWidget.HeightAdd});");
				SB.AppendLine($"\t\tSetSize(MinimumSize);");
            }
            SB.AppendLine($"\t\tCenter();");
        }
		else
		{
			if (Window.Fullscreen) SB.AppendLine("\t\tSetDocked(true);");
			else SB.AppendLine($"\t\tSetSize({Window.Size.Width - DesignWidget.WidthAdd}, {Window.Size.Height - DesignWidget.HeightAdd});");
		}
		SB.AppendLine();
		for (int i = 0; i < Widgets.Count; i++)
		{
			WidgetData w = Widgets[i];
			string parent = w.ParentName;
			if (w.ParentName == Window.Name) parent = "this";
			SB.AppendLine($"\t\t{w.Name} = new {w.ClassName}({parent});");
			CurrentWidget = w;
			w.WriteCode(this);
			if (i != Widgets.Count - 1) SB.AppendLine();
		}
		CurrentWidget = null;
		if (Window.IsPopup)
		{
			if (Window.HasCancelButton || Window.HasOKButton || Window.OtherButtons.Count > 0) SB.AppendLine();
			WriteButtons();
			WriteShortcuts();
		}
	}

	private void WriteButtons()
	{
		if (Window.HasCancelButton) SB.AppendLine($"\t\tCreateButton(\"Cancel\", _ => Cancel());");
		if (Window.HasOKButton) SB.AppendLine($"\t\tCreateButton(\"OK\", _ => OK());");
		Window.OtherButtons.ForEach(btn =>
		{
			SB.AppendLine($"\t\tCreateButton(\"{btn}\");");
		});
	}

	private void WriteShortcuts()
	{
		if (!Window.HasOKButton) return;
		SB.AppendLine();
		SB.AppendLine("\t\tRegisterShortcuts(new List<Shortcut>()");
		SB.AppendLine("\t\t{");
		SB.AppendLine("\t\t\tnew Shortcut(this, new Key(Keycode.ENTER, Keycode.CTRL), _ => OK(), true)");
		SB.AppendLine("\t\t});");
	}

	/// <summary>
	/// To be used explicitly by widgets in the constructor body.
	/// </summary>
	/// <param name="Code">The code to write.</param>
	public void WriteCode(string Code, bool OnSelf = true)
	{
		if (OnSelf)
		{
			if (CurrentWidget == null) throw new Exception("Current widget is null.");
			SB.AppendLine($"\t\t{CurrentWidget.Name}.{Code}");
		}
		else SB.AppendLine($"\t\t{Code}");
	}

	private void WriteConstructorClose()
	{
		SB.AppendLine("\t}");
	}

	private void WriteOKMethod()
	{
		SB.AppendLine("\tprivate void OK()");
		SB.AppendLine("\t{");
		SB.AppendLine("\t\tApply = true;");
		SB.AppendLine("\t\tClose();");
		SB.AppendLine("\t}");
	}

	private void WriteCancelMethod()
	{
		SB.AppendLine("\tprivate void Cancel()");
		SB.AppendLine("\t{");
		SB.AppendLine("\t\tClose();");
		SB.AppendLine("\t}");
	}

	public void ExportWindow(WindowData Window)
	{
		this.Window = Window;
        PopulateWidgetsList(Window);
		AddAllWidgetDependencies();
        WriteDependencies();
		WriteNamespace();
		WriteClassOpen();
		if (Window.HasOKButton) WriteApplyVariable();
		WriteWidgetDefinitions();
		WriteConstructorOpen();
		WriteConstructorBody();
		WriteConstructorClose();
		if (Window.IsPopup && (Window.HasOKButton || Window.HasCancelButton))
		{
			SB.AppendLine();
			if (Window.HasOKButton) WriteOKMethod();
			if (Window.HasCancelButton)
			{
				if (Window.HasOKButton) SB.AppendLine();
				WriteCancelMethod();
			}
		}
		WriteClassClose();
	}

    private void PopulateWidgetsList(WidgetData Widget)
    {
        Widget.Widgets.ForEach(w =>
        {
            Widgets.Add(w);
            PopulateWidgetsList(w);
        });
    }

    public override string ToString()
	{
		return SB.ToString();
	}
}
