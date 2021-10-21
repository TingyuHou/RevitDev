using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage; // needed for Extensible Storage 
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace GLSRevitDev.UIAndEvent
{
  [Transaction(TransactionMode.Manual)]
  class Lab13_View: IExternalCommand
  {
    #region IExternalCommand Members

    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
      UIDocument uiDoc = commandData.Application.ActiveUIDocument;
      Document doc = uiDoc.Document;

      FilteredElementCollector fillCollector = new FilteredElementCollector(doc);
      fillCollector.OfClass(typeof(FillPatternElement));
      var setFillPattern = from elem in fillCollector
                           where elem.Name == "实体填充"
                           select elem;
      FillPatternElement patternElement = null;
      if(setFillPattern.Count() > 0)
        patternElement = setFillPattern.First() as FillPatternElement;

      if (patternElement == null)
        return Result.Failed;

      FilteredElementCollector wallCollector = new FilteredElementCollector(doc);
      wallCollector.OfClass(typeof(Wall));

      OverrideGraphicSettings settings = new OverrideGraphicSettings();
      Color c = new Color(255,0,0);
        settings.SetProjectionFillColor(c);
        settings.SetProjectionFillPatternId(patternElement.Id);

        Transaction trans = new Transaction(doc);
        trans.Start("修改颜色");
      foreach (Element elem in wallCollector)
      {
        doc.ActiveView.SetElementOverrides(elem.Id,settings);
      }
      trans.Commit();

      return Result.Succeeded;
    }

    #endregion
  }
}
