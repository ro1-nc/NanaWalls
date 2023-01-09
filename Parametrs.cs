using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Media.Imaging;

namespace NanaWalls
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    class Parametrs : IExternalCommand
    {
        List<string> para = new List<string>();
        public Reference pickobj { get; set; }
        public ElementType etype { get; set; }
        public Element ele { get; set; }
        List<ElementId> _added_element_ids = new List<ElementId>();
        public Document Doc { get; set; }
        public UIDocument UiDoc { get; set; }
        public static void AddButton(RibbonPanel panel)
        {
            try
            {
                string thisClassName = MethodBase.GetCurrentMethod().DeclaringType.FullName;
                string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

                PushButtonData buttonData = new PushButtonData("cmdParameter", "Get parameter", thisAssemblyPath, thisClassName);
                PushButton pushButton = panel.AddItem(buttonData) as PushButton;
                pushButton.ToolTip = "Gets paramter of an element";

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

            Autodesk.Revit.UI.Selection.Selection pickobj = UiDoc.Selection;

            //picked element from user
            //pick element from user
            //pickobj = UiDoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
            ICollection<Autodesk.Revit.DB.ElementId> selectedIds = UiDoc.Selection.GetElementIds();

            selectedIds = selectedIds.ToList();


            //Retrieve element
            ElementId eleid = selectedIds.First();
            ele = Doc.GetElement(eleid) as Element;

            //Get element type
            ElementId etypeid = ele.GetTypeId();
            etype = Doc.GetElement(etypeid) as ElementType;



            para.Clear();
            foreach (Parameter p in etype.Parameters)
            {
                try
                {

                    para.Add(string.Format("{0} = {1}", p.Definition.Name, p.AsValueString()));

                }
                catch (Exception e)
                {

                }

            }






            File.WriteAllLines(@"D:\\output_file.txt", para);



            _added_element_ids.Clear();

            commandData.Application.Application.DocumentChanged += new EventHandler<Autodesk.Revit.DB.Events.DocumentChangedEventArgs>(OnDocumentChanged);




            //FamilyInstance fam = symbols.First() as FamilyInstance;


            //UiDoc.CanPlaceElementType(symbols[1]);
            //MessageBox.Show("Hello");

            commandData.Application.Application.DocumentChanged -= new EventHandler<DocumentChangedEventArgs>(OnDocumentChanged);

            int n = _added_element_ids.Count;


            //TaskDialog.Show(
            //  "Actions",
            //  string.Format(
            //    "{0} element rotated left.", ele.Name));

            return Result.Succeeded;
        }

        private void OnDocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            _added_element_ids.AddRange(e.GetAddedElementIds());
        }

        public void RotateColumn(Document document, Element element)
        {
            XYZ point1 = new XYZ(pickobj.GlobalPoint.X, pickobj.GlobalPoint.Y, 0);
            XYZ point2 = new XYZ(pickobj.GlobalPoint.X, pickobj.GlobalPoint.Y, 30);
            // The axis should be a bound line.
            Line axis = Line.CreateBound(point1, point2);
            ElementTransformUtils.RotateElement(document, element.Id, axis, Math.PI / 2.0);
        }




    }
}
