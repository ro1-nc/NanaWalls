using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;





namespace NanaWalls
{
    [Transaction(TransactionMode.Manual)]

    public class ExtrudeWall : IExternalCommand
    {

        List<string> all_details = new List<string>();
        //public selection pickobj { get; set; }
        public ElementType etype { get; set; }
        public Element ele { get; set; }
        public static void AddButton(RibbonPanel panel)
        {
            try
            {
                string thisClassName = MethodBase.GetCurrentMethod().DeclaringType.FullName;
                string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

                //PushButtonData buttonData = new PushButtonData("cmdFirst", "Revit First Tool", thisAssemblyPath, thisClassName);
                //PushButton pushButton = panel.AddItem(buttonData) as PushButton;
                //pushButton.ToolTip = "my first plugin\nVersion : 1.1.0";

                PushButtonData buttonData = new PushButtonData("cmdExtrude", "Extrudes profile", thisAssemblyPath, thisClassName);
                PushButton pushButton = panel.AddItem(buttonData) as PushButton;
                pushButton.ToolTip = "my extrude profile plugin\nVersion : 1.1.0";
            }
            catch (Exception ex)
            {
            }
        }


        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {



                UiDoc = commandData.Application.ActiveUIDocument;
                Doc = UiDoc.Document;


                Autodesk.Revit.UI.Selection.Selection selection = UiDoc.Selection;
                ICollection<Autodesk.Revit.DB.ElementId> selectedIds = UiDoc.Selection.GetElementIds();

                //pick element from user
                //UiDoc.Selection.PickObject(Autodesk.Revit.UI.Selection.Selection.)=pickobj ;


                Element eFromString = Doc.GetElement(selectedIds.First());

                LocationCurve LP = eFromString.Location as LocationCurve;

                XYZ start = LP.Curve.GetEndPoint(0);
                XYZ end = LP.Curve.GetEndPoint(1);

                Line sel = Line.CreateBound(new XYZ(start.X, start.Y, 0), new XYZ(end.X, end.Y, 0));

                Line leftpara = CreateParallelLine(sel, -1);
                Line leftpara2 = Line.CreateBound(new XYZ(leftpara.GetEndPoint(0).X, leftpara.GetEndPoint(0).Y, 10), new XYZ(leftpara.GetEndPoint(1).X, leftpara.GetEndPoint(1).Y, 10));

                Line rightpara = CreateParallelLine(sel, 1);

                Line invertrightpara = Line.CreateBound(rightpara.GetEndPoint(1), rightpara.GetEndPoint(0));
                Line invertrightpara2 = Line.CreateBound(new XYZ(invertrightpara.GetEndPoint(0).X, invertrightpara.GetEndPoint(0).Y, 10), new XYZ(invertrightpara.GetEndPoint(1).X, invertrightpara.GetEndPoint(1).Y, 10));

                XYZ topleft = leftpara.GetEndPoint(0);
                XYZ bottomleft = leftpara.GetEndPoint(1);

                XYZ topright = rightpara.GetEndPoint(0);
                XYZ bottomright = rightpara.GetEndPoint(1);

                Line toppara = Line.CreateBound(new XYZ(topleft.X, topleft.Y, 0), new XYZ(topright.X, topright.Y, 0));
                Line bottompara = Line.CreateBound(new XYZ(bottomleft.X, bottomleft.Y, 0), new XYZ(bottomright.X, bottomright.Y, 0));
                Line bottompara2 = Line.CreateBound(new XYZ(bottomleft.X, bottomleft.Y, 10), new XYZ(bottomright.X, bottomright.Y, 10));

                Line inverttoppara = Line.CreateBound(toppara.GetEndPoint(1), toppara.GetEndPoint(0));
                Line inverttoppara2 = Line.CreateBound(new XYZ(topright.X, topright.Y, 10), new XYZ(topleft.X, topleft.Y, 10));


                //r = Doc.GetRoomAtPoint();
                all_details.Add(eFromString.Name);



                using (Transaction t = new Transaction(Doc, "Extrude"))
                {

                    t.Start();

                    DrawModelLine(leftpara);
                    DrawModelLine(rightpara);
                    DrawModelLine(toppara);
                    DrawModelLine(bottompara);


                    SolidOptions options = new SolidOptions(ElementId.InvalidElementId, ElementId.InvalidElementId);

                    CurveLoop profileLoop = new CurveLoop();
                    CurveLoop profileLoop2 = new CurveLoop();
                    List<CurveLoop> profile3 = new List<CurveLoop>();
                    List<Line> lines = new List<Line>();
                    List<Line> zlines = new List<Line>();
                    lines.Add(leftpara);
                    lines.Add(bottompara);
                    lines.Add(invertrightpara);
                    lines.Add(inverttoppara);

                    zlines.Add(leftpara2);
                    zlines.Add(bottompara2);
                    zlines.Add(invertrightpara2);
                    zlines.Add(inverttoppara2);

                    foreach (Line l in lines)
                    {
                        profileLoop.Append(l);
                    }

                    foreach (Line l in zlines)
                    {
                        profileLoop2.Append(l);
                    }
                    //Here I want to add both created CurveLoops to list
                    profile3.Add(profileLoop);

                    

                    profile3.Add(profileLoop2);

                    //Create Loft
                    Solid loft = GeometryCreationUtilities.CreateLoftGeometry(profile3, options);
                    //profile3.Add(leftpara);






                    t.Commit();

                }

                // MessageBox.Show(string.Join(Environment.NewLine, all_details), "List of walls");
            }
            catch (Exception e)
            {

            }

            return Result.Succeeded;
        }

        public Line CreateParallelLine(Line target, int offset)
        {


            XYZ p = target.GetEndPoint(0);
            XYZ q = target.GetEndPoint(1);

            var xDifference = target.GetEndPoint(0).X - target.GetEndPoint(1).X;
            var yDifference = target.GetEndPoint(0).Y - target.GetEndPoint(1).Y;
            var length = Math.Sqrt(Math.Pow(xDifference, 2) + Math.Pow(yDifference, 2));



            var X1 = (float)(target.GetEndPoint(0).X - offset * yDifference / length);
            var X2 = (float)(target.GetEndPoint(1).X - offset * yDifference / length);
            var Y1 = (float)(target.GetEndPoint(0).Y + offset * xDifference / length);
            var Y2 = (float)(target.GetEndPoint(1).Y + offset * xDifference / length);

            Line parallelLine = Line.CreateBound(new XYZ(X1, Y1, 0), new XYZ(X2, Y2, 0));

            return parallelLine;
        }

        public void DrawModelLine(Line line)
        {
            XYZ origin = new XYZ(0, 0, 0);

            XYZ dir = line.Direction;
            double x = dir.X, y = dir.Y, z = dir.Z;
            //XYZ normal = new XYZ(z - y, x - z, y - x);

            XYZ p = line.GetEndPoint(0);
            XYZ q = line.GetEndPoint(1);
            XYZ v = q - p;
            XYZ w, normal = null;

            double dxy = Math.Abs(v.X) + Math.Abs(v.Y);

            w = XYZ.BasisY;

            //? XYZ.BasisZ
            //: XYZ.BasisY;

            normal = v.CrossProduct(w).Normalize();

            //XYZ normal = new XYZ(1, 1, 0);

            Plane geomPlane = Plane.CreateByNormalAndOrigin(normal, origin);

            // Plane geomPlane =Plane.Create()eate.NewPlane(normal, origin);


            SketchPlane sketch = SketchPlane.Create(Doc, geomPlane);

            ModelLine line12 = Doc.Create.NewModelCurve(line, sketch) as ModelLine;

        }



        public void Comments()
        {
            //XYZ pt31 = new XYZ(0, 0, 0);
            //XYZ pt41 = new XYZ(10, 0, 0);

            //Line line1 = Line.CreateBound(pt31, pt41);

            //XYZ origin = new XYZ(0, 0, 0);

            //XYZ dir = line1.Direction;
            //double x = dir.X, y = dir.Y, z = dir.Z;
            //XYZ normal = new XYZ(z - y, x - z, y - x);

            ////XYZ normal = new XYZ(1, 1, 0);

            //Plane geomPlane = Plane.CreateByNormalAndOrigin(normal, origin);

            //// Plane geomPlane =Plane.Create()eate.NewPlane(normal, origin);


            //SketchPlane sketch = SketchPlane.Create(Doc, geomPlane);

            //ModelLine line12 = Doc.Create.NewModelCurve(line1, sketch) as ModelLine;


            //GeometryCreationUtilities.CreateExtrusionGeometry()

            //ModelLine line =Doc.Create.NewModelCurve(line1, sketchPlane) as ModelLine;

            //Element trial = Doc.GetElement(selectedIds.ToArray()[0]); ;

            //LocationPoint Lp = trial.Location as LocationPoint;

            //LocationCurve wallLocation = trial.Location  as LocationCurve;

            //XYZ pt1 = wallLocation.Curve.GetEndPoint(0);
            //XYZ pt2 = wallLocation.Curve.GetEndPoint(1);

            ////XYZ ElementPoint = wallLocation. as XYZ;
            //Room R = Doc.GetRoomAtPoint(pt2);
            //TaskDialog.Show("ROOM NAME", R.Name);
        }


        public Document Doc { get; set; }
        public UIDocument UiDoc { get; set; }

    }
}
