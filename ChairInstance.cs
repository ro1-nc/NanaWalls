using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Media.Imaging;

namespace NanaWalls
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    class ChairInstance : IExternalCommand
    {
        List<ElementId> _added_element_ids = new List<ElementId>();
        public Document Doc { get; set; }
        public UIDocument UiDoc { get; set; }
        public static void AddButton(RibbonPanel panel)
        {
            try
            {
                string thisClassName = MethodBase.GetCurrentMethod().DeclaringType.FullName;
                string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

                PushButtonData buttonData = new PushButtonData("cmdChairInstance", "Chair Instance", thisAssemblyPath, thisClassName);
                PushButton pushButton = panel.AddItem(buttonData) as PushButton;
                pushButton.ToolTip = "Creates chair instnace";

                //var path = "D:\\button_1.png";
                //Uri uriImage = new Uri(path);
                //BitmapImage largeimage = new BitmapImage();
                //pushButton.LargeImage = largeimage;

            }
            catch (Exception ex)
            {
            }

        }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            //UIApplication uiapp = commandData.Application;
            //UIDocument uidoc = uiapp.ActiveUIDocument;
            //Application app = uiapp.Application;
            //Document doc = uidoc.Document;

            UiDoc = commandData.Application.ActiveUIDocument;
            Doc = UiDoc.Document;

            FilteredElementCollector collector = new FilteredElementCollector(Doc);

            collector.OfCategory(BuiltInCategory.OST_Furniture);
            collector.OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>().ToList();

            FamilySymbol symbol = collector.FirstElement() as FamilySymbol;

            List<FamilySymbol> symbols = new List<FamilySymbol>();
            foreach (FamilySymbol syu in collector)
            {

                string famname = "Seating - Artemis - Lounge chair";
                if (famname == syu.FamilyName)
                {
                    symbols.Add(syu);
                }

            }


            _added_element_ids.Clear();

            commandData.Application.Application.DocumentChanged += new EventHandler<Autodesk.Revit.DB.Events.DocumentChangedEventArgs>(OnDocumentChanged);
            

            try
            {               

                UiDoc.PromptForFamilyInstancePlacement(symbols.First());


            }
            catch (Exception a)
            {
                
            }

            //FamilyInstance fam = symbols.First() as FamilyInstance;


            //UiDoc.CanPlaceElementType(symbols[1]);
            //MessageBox.Show("Hello");

            commandData.Application.Application.DocumentChanged -= new EventHandler<DocumentChangedEventArgs>(OnDocumentChanged);

            int n = _added_element_ids.Count;

            Element elem = Doc.GetElement(_added_element_ids[0]);
            FamilyInstance fi = elem as FamilyInstance;

            LocationPoint lp = fi.Location as LocationPoint;
            XYZ ppt = new XYZ(lp.Point.X, lp.Point.Y, 0);
            
            
            Line axis = Line.CreateBound(ppt, new XYZ(ppt.X, ppt.Y, ppt.Z + 10));

            using (Transaction t = new Transaction(Doc, "Rotate"))
            {

                t.Start();
                ElementTransformUtils.RotateElement(Doc, fi.Id, axis, (Math.PI / 180) * (90));

                XYZ mov_ppt = UiDoc.Selection.PickPoint("Click to specify where you want to change location.");

                //XYZ mov_ppt = new XYZ(lp.Point.X+1, lp.Point.Y+ 1, 0); //UiDoc.Selection.PickPoint("Click to specify where you want to change location.");
                
               ElementTransformUtils.MoveElement(Doc, fi.Id, mov_ppt);

                TaskDialog.Show("Locations", string.Format("{0},{1}", ppt, mov_ppt));
                t.Commit();

            }


            TaskDialog.Show(
              "Place Family Instance",
              string.Format(
                "{0} element{1} added.", n,
                ((1 == n) ? "" : "s")));

            return Result.Succeeded;
        }

        private void OnDocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            _added_element_ids.AddRange(e.GetAddedElementIds());
        }




    }
}
