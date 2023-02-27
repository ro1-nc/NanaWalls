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


    class CreateParameter : IExternalCommand
    {

        public Document Doc { get; set; }
        public UIDocument UiDoc { get; set; }
        public static void AddButton(RibbonPanel panel)
        {
            try
            {
                string thisClassName = MethodBase.GetCurrentMethod().DeclaringType.FullName;
                string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

                PushButtonData buttonData = new PushButtonData("cmdpara", "Creates Parameter", thisAssemblyPath, thisClassName);
                PushButton pushButton = panel.AddItem(buttonData) as PushButton;
                pushButton.ToolTip = "Creates parameter";

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
            UIApplication uiApp = commandData.Application;
            Autodesk.Revit.ApplicationServices.Application app = uiApp.Application;
            Document doc = commandData.Application.ActiveUIDocument.Document;

            CategorySet categories = app.Create.NewCategorySet();
            categories.Insert(doc.Settings.Categories.get_Item(BuiltInCategory.OST_Doors));

            string originalFile = app.SharedParametersFilename;
            string tempFile = @"D:\Projects\NanaWalls\SharedParameters.txt";


            app.SharedParametersFilename = tempFile;
            DefinitionFile sharedParameterFile = app.OpenSharedParameterFile();

            DefinitionGroup dgc = sharedParameterFile.Groups.Create("New Param");

            ExternalDefinitionCreationOptions opt = new ExternalDefinitionCreationOptions("Log11",ParameterType.Text);

            Definition defc = dgc.Definitions.Create(opt);

            foreach (DefinitionGroup dg in sharedParameterFile.Groups)
            {
                // ExternalDefinition externalDefinition = app.Create.NewExternalDefinition(false, ParameterType.Text);
                ExternalDefinition externalDefinition = dg.Definitions.get_Item("Log11") as ExternalDefinition;

                using (Transaction t = new Transaction(doc))
                {
                    t.Start("Add Shared Parameters");
                    //parameter binding 
                    InstanceBinding newIB = app.Create.NewInstanceBinding(categories);
                    //parameter group to text
                    try
                    {
                        doc.ParameterBindings.Insert(externalDefinition, newIB, BuiltInParameterGroup.PG_TEXT);

                    }
                    catch (Exception e)
                    {

                    }
                    t.Commit();
                }

            }

            app.SharedParametersFilename = originalFile;

            return Result.Succeeded;

        }
    }
}
