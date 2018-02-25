using PicTag.Data;
using PicTag.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PicTag
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            FileSystem fs = new FileSystem();
            FormState source = new FormState(fs);
            MenuModule menu = new MenuModule();
            ListModule list = new ListModule(menu, source);
            TreeModule tree = new TreeModule(source);
            Application.Run(new MainForm(menu, list, tree, source));
        }
    }
}
