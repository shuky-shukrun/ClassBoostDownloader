using System;
using System.Collections.Generic;
using System.Text;

namespace ClassBoostDownloader
{
    class Program
    {
        public static void printTitle()
        {
            Console.WriteLine(@" ___ ___ ___ ___ ___ ___ ___ ___ ___ ___ ___ ___ ___ ___ ___ ___ ___ ___ ___ ___ ___ ___ ___ ___ ___ 
(___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___)
       ______________                    ________                   _____ 
       __  ____/__  /_____ _________________  __ )____________________  /_
       _  /    __  /_  __ `/_  ___/_  ___/_  __  |  __ \  __ \_  ___/  __/
        / /___  _  / / /_/ /_(__  )_(__  )_  /_/ // /_/ / /_/ /(__  )/ /_  
       \____/  /_/  \__,_/ /____/ /____/ /_____/ \____/\____//____/ \__/

:::::| :::::| :::::| :::::|         Or Eliyahu / V1.0   :::::| :::::| :::::| :::::|:::::| :::::|
:::::| :::::| :::::| :::::|  Download videos from https://classboost.co.il  :::::| :::::| :::::|
      ________                       ______            _________            
      ___  __ \________      ___________  /___________ ______  /____________
      __  / / /  __ \_ | /| / /_  __ \_  /_  __ \  __ `/  __  /_  _ \_  ___/
       _  /_/ // /_/ /_ |/ |/ /_  / / /  / / /_/ / /_/ // /_/ / /  __/  /    
      /_____/ \____/____/|__/ /_/ /_//_/  \____/\__,_/ \__,_/  \___//_/   
 ___ ___ ___ ___ ___ ___ ___ ___ ___ ___ ___ ___ ___ ___ ___ ___ ___ ___ ___ ___ ___ ___ ___ ___ ___ 
(___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___|___)
");
        }
        public static void hebrewPrint(string str)
        {
            Console.OutputEncoding = Encoding.GetEncoding(1255);
            char[] charArray = str.ToCharArray();
            Array.Reverse(charArray);
            Console.Write(new string(charArray));
            Console.OutputEncoding = Encoding.Default;
        }
        static void Main(string[] args)
        {
            ClassBoost cb = new ClassBoost();
            Console.Title = "ClassBoost Downloader / Or Eliyahu / V1.0";
            printTitle();

            Console.Write("Enter username: ");
            string email = Console.ReadLine();
            Console.Write("Enter password: ");
            string password = Console.ReadLine();


            if (cb.login(email, password))
            {
                uint mtId;
                Console.Clear();        
                printTitle();
                Console.WriteLine("Successfully logged in");
                Console.WriteLine("Enter meeting id:");
                Console.WriteLine("For example https://classboost.co.il/Pages/VideoPage.aspx?MeetingID=XXXXXXX - the number that appears in XX...X");
                if (uint.TryParse(Console.ReadLine(), out mtId))
                {
                    try
                    {
                        PlaylistM3U8 pl = cb.getMeeting(mtId);
                        List<VideoChunk> lvc = cb.loadPlaylist(pl);
                        lvc = cb.downloadMeeting(pl, lvc);
                        cb.mergeChunks(pl, lvc);
                    }catch(Exception ex)
                    {
                        Console.WriteLine("Error: " + ex.Message);
                    }
                }
                else
                {
                    Console.WriteLine("Meeting id must be a positive number");
                }
            }
            else
            {
                Console.WriteLine("Login error, please run again");
            }
            Console.ReadKey();
        }
    }
}
