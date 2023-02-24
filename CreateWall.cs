using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NanaWalls
{
    [Transaction(TransactionMode.Manual)]


    class CreateWall : IExternalCommand
    {

        public Document Doc { get; set; }
        public UIDocument UiDoc { get; set; }
        public static void AddButton(RibbonPanel panel)
        {
            try
            {
                string thisClassName = MethodBase.GetCurrentMethod().DeclaringType.FullName;
                string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

                PushButtonData buttonData = new PushButtonData("cmdwalll", "Creates wall", thisAssemblyPath, thisClassName);
                PushButton pushButton = panel.AddItem(buttonData) as PushButton;
                pushButton.ToolTip = "Creates wall";

                Assembly myAssembly = Assembly.GetExecutingAssembly();

                pushButton.LargeImage = GetEmbeddedImage(myAssembly, "NanaWalls.Resources.icon.png");

            }
            catch (Exception ex)
            {
            }

        }

        private static ImageSource GetEmbeddedImage(System.Reflection.Assembly assemb, string imageName)
        {
            System.IO.Stream file = assemb.GetManifestResourceStream(imageName);
            PngBitmapDecoder bd = new PngBitmapDecoder(file, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

            return bd.Frames[0];
        }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            Transaction transaction = new Transaction(doc, "Create Wall");
            transaction.Start();

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            WallType wallType = collector.OfClass(typeof(WallType)).Cast<WallType>().FirstOrDefault(x => x.Name == "Wall-Ext_102Bwk-75Ins-100LBlk-12P") as WallType;
            FilteredElementCollector levcollector = new FilteredElementCollector(doc);
            Level level = levcollector.OfClass(typeof(Level)).Cast<Level>().FirstOrDefault() as Level;

            Wall wall = Wall.Create(doc, Line.CreateBound(new XYZ(0, 0, 0), new XYZ(10, 0, 0)), wallType.Id, level.Id, 10, 0, false, false);

            transaction.Commit();

            MessageBox.Show("Wall Created");
            return Result.Succeeded;
        }
    }
}
