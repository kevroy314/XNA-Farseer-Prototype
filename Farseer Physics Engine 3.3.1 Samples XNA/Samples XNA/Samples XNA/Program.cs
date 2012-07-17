namespace KevinsDemo
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main(string[] args)
        {
            using (GameMain game = new GameMain())
            {
                game.Run();
            }
        }
    }
}