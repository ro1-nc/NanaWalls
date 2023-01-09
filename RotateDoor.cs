using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
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

    class RotateDoor : IExternalCommand
    {
        List<double> all_values = new List<double>();

       

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

                PushButtonData buttonData = new PushButtonData("cmdRotateDoor", "Rotate Door", thisAssemblyPath, thisClassName);
                PushButton pushButton = panel.AddItem(buttonData) as PushButton;
                pushButton.ToolTip = "Rotates door left by 90 degree";

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
            ele = Doc.GetElement(eleid);

            ElementId etypeid = ele.GetTypeId();
            etype = Doc.GetElement(etypeid) as ElementType;

            var para = new List<KeyValuePair<string, double>>();

            para.Clear();
            foreach (Parameter p in etype.Parameters)
            {
                try
                {
                    para.Add(new KeyValuePair<string, double>(p.Definition.Name.ToString(),p.AsDouble()));
                    
                    //para.Add(string.Format("{0} = {1}", p.Definition.Name, p.AsValueString()));
                    
                   // all_values.Add(p.AsValueString());
                }
                catch (Exception e)
                {

                }

            }

            //double Xhinge=0;

            //foreach(var val in para.Where(e=>e.Key=="Hinge X"))
            //{
            //    all_values.Add(val.Value);
            //    Xhinge = val.Value;
            //}

            var Xhinge = from se in para
                         where se.Key == "Hinge X"
                         select se.Value;

            var Yhinge = from se in para
                     where se.Key == "Hinge Y"
                     select se.Value;

            var left_mark = from se in para
                         where se.Key == "Left Mark"
                         select se.Value;

            var instance_para = new List<KeyValuePair<string, double>>();
           // List<string> instance_para = new List<string>();
            List<string> instance_para_values = new List<string>();

            if (ele.GetType() == typeof(FamilyInstance))
            {
                FamilyInstance fi = ele as FamilyInstance;
                
                foreach(Parameter p in fi.Parameters)
                {
                    try
                    {
                        instance_para.Add(new KeyValuePair<string, double>(p.Definition.Name.ToString(),(p.AsDouble())));
                        instance_para_values.Add(p.AsValueString());
                    }
                    catch(Exception e)
                    {

                    }
                    
                }

                var left_dist = from se in instance_para
                                where se.Key == "Left Distance"
                                select se.Value;



                LocationPoint lp = fi.Location as LocationPoint;
                XYZ ppt = new XYZ(lp.Point.X - left_dist.First() + left_mark.First() - Xhinge.First(), lp.Point.Y + Yhinge.First(), 0);



                Line axis = Line.CreateBound(ppt, new XYZ(ppt.X, ppt.Y, ppt.Z + 10));

                using (Transaction t = new Transaction(Doc, "Rotate"))
                {

                    t.Start();
                    ElementTransformUtils.RotateElement(Doc, fi.Id, axis, (Math.PI / 180) * (90));

                    //XYZ mov_ppt = UiDoc.Selection.PickPoint("Click to specify where you want to change location.");



                    //XYZ mov_ppt = new XYZ(lp.Point.X+1, lp.Point.Y+ 1, 0); //UiDoc.Selection.PickPoint("Click to specify where you want to change location.");

                    //ElementTransformUtils.MoveElement(Doc, fi.Id, mov_ppt);

                    //TaskDialog.Show("Locations", string.Format("{0},{1}", ppt, mov_ppt));
                    t.Commit();

                }
            }
            else
            {
                try
                {
                    using (Transaction t = new Transaction(Doc, "Rotate"))
                    {

                        t.Start();
                        RotateColumn(Doc, ele);
                        t.Commit();

                    }

                    //RotateColumn(Doc,)
                }
                catch (Exception a)
                {

                }
            }




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
