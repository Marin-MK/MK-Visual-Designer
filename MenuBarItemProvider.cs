using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    new MenuItem("New Project", _ => Program.NewProject()),
                    new MenuItem("Open Project", _ => Program.OpenProject()),
                    new MenuItem("Save Project", _ => Program.SaveProject()),
                    new MenuItem("Export As")
                    {
                        Items = new List<IMenuItem>()
                        {
                            new MenuItem("Image", _ => Program.ExportAsPNG()),
                            new MenuItem("Pseudo-Code", _ => Program.ExportAsPseudoCode())
                        }
                    },
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
                        OnClicked = _ => Program.Undo()
                    },
                    new MenuItem("Redo")
                    {
                        IsClickable = e => e.Value = Program.RedoList.Count > 0,
                        OnClicked = _ => Program.Redo()
                    }
                }
            }
        };
    }
}
