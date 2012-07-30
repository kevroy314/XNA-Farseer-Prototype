namespace KevinsDemo
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main(string[] args)
        {
            //Create and run the GameMain
            using (GameMain game = new GameMain())
            {
                game.Run();
            }
        }
    }
}