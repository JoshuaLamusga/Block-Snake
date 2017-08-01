using System;

namespace Snake
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class MainInit
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (MainLoop game = new MainLoop())
                game.Run();
        }
    }
#endif
}
