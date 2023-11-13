using CodeImp.Fluxtreme;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CodeImp.Fluxtreme
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IMainWindow Window => Current.MainWindow as IMainWindow;
    }
}
