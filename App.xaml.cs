Current (likely) using System.Windows;

namespace Ketch { public partial class App : Application { } }

Replace with this (explicit WPF Application): using System.Windows;

namespace Ketch { public partial class App : System.Windows.Application { } }
