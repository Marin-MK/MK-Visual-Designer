namespace VisualDesigner;

public static class MenuBarItemProvider
{
    public static List<MenuItem> GetItems()
    {
        return new List<MenuItem>()
        {
            new MenuItem("File")
            {
                Items = new List<IMenuItem>()
                {
                    new MenuItem("New", _ => Program.NewProject()) { Shortcut = "Ctrl+N" },
                    new MenuItem("Open", _ => Program.OpenProject()) { Shortcut = "Ctrl+O" },
                    new MenuItem("Save", _ => Program.SaveProject()) { Shortcut = "Ctrl+S" },
                    new MenuItem("Save As", _ => Program.SaveProjectAs()) { Shortcut = "Ctrl+Shift+S" },
                    new MenuItem("Export as Code", _ => Program.ExportAsPseudoCode()) { Shortcut = "Ctrl+E" },
                    new MenuSeparator(),
                    new MenuItem("Exit", _ => Program.Exit(true))
                }
            },
            new MenuItem("Edit")
            {
                Items = new List<IMenuItem>()
                {
                    new MenuItem("Undo")
                    {
                        IsClickable = e => e.Value = Program.UndoList.Count > 0,
                        OnClicked = _ => Program.Undo(),
                        Shortcut = "Ctrl+Z"
                    },
                    new MenuItem("Redo")
                    {
                        IsClickable = e => e.Value = Program.RedoList.Count > 0,
                        OnClicked = _ => Program.Redo(),
                        Shortcut = "Ctrl+Y"
                    }
                }
            }
        };
    }
}
