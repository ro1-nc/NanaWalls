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
using System.Windows.Media.Imaging;

namespace NanaWalls
{
    [Transaction(TransactionMode.Manual)]

    
    class Hello : IExternalCommand
    {

        public Document Doc { get; set; }
        public UIDocument UiDoc { get; set; }
        public static void AddButton(RibbonPanel panel)
        {
            try
            {
                string thisClassName = MethodBase.GetCurrentMethod().DeclaringType.FullName;
                string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

                PushButtonData buttonData = new PushButtonData("cmdHello", "Hello", thisAssemblyPath, thisClassName);
                PushButton pushButton = panel.AddItem(buttonData) as PushButton;
                pushButton.ToolTip = "Hello";

                var path= "D:\\button_1.png";
                Uri uriImage = new Uri(path);
                BitmapImage largeimage = new BitmapImage();
                pushButton.LargeImage = largeimage;

            }
            catch (Exception ex)
            {
            }

        }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {


            MessageBox.Show("Hello");
            return Result.Succeeded;
        }
    }
}
