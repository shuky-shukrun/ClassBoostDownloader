using System;

namespace ClassBoostDownloader
{
    class RefreshingLine
    {
        public RefreshingLine(string str)
        {
            NewWriteLine(str);
        }
        public void WriteRefreshLine(string str)
        {
            ClearCurrentConsoleLine();
            Console.WriteLine(str);
            Console.SetCursorPosition(0, Console.CursorTop - 1);
        }
        public void NewWriteLine(string str)
        {
            ClearCurrentConsoleLine();
            Console.WriteLine(str);
        }
        private void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }
    }
}
