using System.IO;
using System.Text;

namespace DinosaurGame
{
    class Program
    {
        public static int maxjump = 10;

        static void Main()
        {
            Console.OutputEncoding = Encoding.Unicode;
            Console.CursorVisible = false;

            // Nhận phím từ người dùng
            ConsoleKeyInfo key = new ConsoleKeyInfo();

            int heightjumping = 0,  // Độ cao khi nhảy
                speed = 9000;       // Tốc độ dịch chuyển phong cảnh

            // Lưu các Bitmap của T-rex, Xương rồng, Thằn lằn bay -> kiểm tra va chạm
            KeyValuePair<int[], bool[,]> ShapOfTrex
                = new KeyValuePair<int[], bool[,]>(new int[] { 0, 0 }, new bool[,] { { true } });

            KeyValuePair<int[][], bool[][,]> ShapOfCacti
                = new KeyValuePair<int[][], bool[][,]>(new int[][] { new int[] { 0, 0 } }, new bool[][,] { new bool[,] { { true } } });

            KeyValuePair<int[][], bool[][,]> ShapOfPteroes
                = new KeyValuePair<int[][], bool[][,]>(new int[][] { new int[] { 0, 0 } }, new bool[][,] { new bool[,] { { true } } });

            Display.Print(2, 0, "<Esc> thoát", "");

            // Khủng long chạy
            Thread Running = new Thread(() =>
            {
                while (key.Key != ConsoleKey.Escape)
                {
                    if (heightjumping == 0)
                        for (int i = 0; i < Display.TheNumberOfTrexImages; i++)
                        {
                            Display.Trex(0, i, ref ShapOfTrex);
                            if (heightjumping != 0) break;
                            Thread.Sleep(1000000 / speed);
                        }
                }
            });
            Running.Start();

            // Phong cảnh dịch chuyển
            Thread Scenery = new Thread(() =>
            {
                int move = 0,
                    spawncactus = 0;

                while (key.Key != ConsoleKey.Escape)
                {
                    if (move >= Display.LenghtOfTheRoad - 1) move = 0;

                    if (OstacleChecking(ShapOfTrex, ShapOfPteroes, ShapOfCacti)) Environment.Exit(0);
                    Display.Road(move);

                    // Lệnh sinh cây xương rồng, với (new Random()).Next(a, b), tỉ lệ sinh ra là 1/(b-a)
                    // (spawncactus > 40 || (spawncactus < 6 && spawncactus > 3)) không để xương rồng quá nhiều và gần nhau
                    if ((new Random()).Next(1, 40) == 1 && (spawncactus > 40 || (spawncactus < 6 && spawncactus > 3)))
                    {
                        spawncactus = 0;
                        Display.Cactus(Console.BufferWidth - 1, 0, true, ref ShapOfCacti);
                    }

                    Display.Cactus(0, 0, false, ref ShapOfCacti);

                    if (OstacleChecking(ShapOfTrex, ShapOfPteroes, ShapOfCacti)) Environment.Exit(0);

                    Thread.Sleep(1 / 9000);

                    move++;
                    spawncactus++;
                }
            });
            Scenery.Start();

            // Thằn lằn bay
            Thread Pterodactyls = new Thread(() =>
            {
                int move = 0,
                    spawnptero = 0;
                while (key.Key != ConsoleKey.Escape)
                {
                    if (move >= Display.LenghtOfTheRoad - 1) move = 0;
                    if (OstacleChecking(ShapOfTrex, ShapOfPteroes, ShapOfCacti)) Environment.Exit(0);

                    // Lệnh sinh ra thằn lằn bay, tương tự như của xương rồng
                    if ((new Random()).Next(1, 500) == 1 && (spawnptero > 40 || (spawnptero < 6 && spawnptero > 3)))
                    {
                        spawnptero = 0;
                        Display.Ptero(Console.BufferWidth - 5, true, ref ShapOfPteroes);
                    }
                    else
                        Display.Ptero(Console.BufferWidth - 5, false, ref ShapOfPteroes);
                    if (OstacleChecking(ShapOfTrex, ShapOfPteroes, ShapOfCacti)) Environment.Exit(0);

                    Thread.Sleep(20);

                    move++;
                    spawnptero++;
                }
            });
            Pterodactyls.Start();

            // In ra điểm số
            Thread Achievement = new Thread(() =>
            {
                int score = 0;
                while (key.Key != ConsoleKey.Escape)
                {
                    Display.Print(50, 0, "Điểm: " + score, "");
                    Thread.Sleep(speed / 20);
                    score++;
                }
            });
            Achievement.Start();

            // Xử lý phím từ người dùng
            while (true)
            {
                key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.Escape: Environment.Exit(0); break;
                    case ConsoleKey.Spacebar:
                        Thread jump = new Thread(() =>
                        {
                            // Nhảy lên
                            for (int j = 0; j < maxjump; j++)
                            {
                                heightjumping = j;
                                Display.Trex(j, 0, ref ShapOfTrex);
                                if (OstacleChecking(ShapOfTrex, ShapOfPteroes, ShapOfCacti)) Environment.Exit(0);

                                Thread.Sleep((30 + heightjumping * 2) * 10000 / speed);
                            }
                            // Rớt xuống
                            for (int j = maxjump; j >= 0; j--)
                            {
                                heightjumping = j;
                                Display.Trex(j, 0, ref ShapOfTrex);
                                if (OstacleChecking(ShapOfTrex, ShapOfPteroes, ShapOfCacti)) Environment.Exit(0);

                                Thread.Sleep((40 + heightjumping * 4) * 10000 / speed);
                            }
                        });

                        if (heightjumping == 0)
                        {
                            jump.Start();
                            Console.Beep(500, 100);
                        }
                        break;
                }
            }
        }
        // Khoá để chỉ cho một luồng truy cập vào một thời điểm
        static readonly object Lock = new object();
        
        // Kiểm tra va chạm
        static bool OstacleChecking(KeyValuePair<int[], bool[,]> ShapOfTrex, KeyValuePair<int[][], bool[][,]> ShapOfPteroes, KeyValuePair<int[][], bool[][,]> ShapOfCacti)
        {
            KeyValuePair<int[][], bool[][,]> UseForComparision = new KeyValuePair<int[][], bool[][,]>();

            lock (Lock)
            {
                bool Crash = false;

                for (int h = 0; h < 2; h++)
                {
                    if (h == 0)
                        UseForComparision = ShapOfPteroes;

                    if (h == 1)
                        UseForComparision = ShapOfCacti;

                    for (int i = 0; i < UseForComparision.Key.Length; i++)
                        for (int j = 0; j < ShapOfTrex.Value.GetLength(0); j++)
                            for (int k = 0; k < ShapOfTrex.Value.GetLength(1); k++)
                            {
                                int Index1 = k + UseForComparision.Key[i][0] - ShapOfTrex.Key[0];
                                int Index2 = j + UseForComparision.Key[i][1] - ShapOfTrex.Key[1];

                                if (Index1 + 1 < UseForComparision.Value[i].GetLength(0) && Index2 < UseForComparision.Value[i].GetLength(1) && Index1 + 1 >= 0 && Index2 >= 0)
                                    if (ShapOfTrex.Value[j, k] && UseForComparision.Value[i][Index1 + 1, Index2])
                                        Crash = true;

                                if (Index1 - 1 < UseForComparision.Value[i].GetLength(0) && Index2 < UseForComparision.Value[i].GetLength(1) && Index1 - 1 >= 0 && Index2 >= 0)
                                    if (ShapOfTrex.Value[j, k] && UseForComparision.Value[i][Index1 - 1, Index2])
                                        Crash = true;

                                if (Index1 < UseForComparision.Value[i].GetLength(0) && Index2 + 1 < UseForComparision.Value[i].GetLength(1) && Index1 >= 0 && Index2 + 1 >= 0)
                                    if (ShapOfTrex.Value[j, k] && UseForComparision.Value[i][Index1, Index2 + 1])
                                        Crash = true;

                                if (Index1 < UseForComparision.Value[i].GetLength(0) && Index2 - 1 < UseForComparision.Value[i].GetLength(1) && Index1 >= 0 && Index2 - 1 >= 0)
                                    if (ShapOfTrex.Value[j, k] && UseForComparision.Value[i][Index1, Index2 - 1])
                                        Crash = true;
                            }
                }
                return Crash;
            }
        }
    }
    class Display
    {
        public static int
            TheNumberOfTrexImages = 0,      // Số lượng image của cho T-rex
            LenghtOfTheRoad = 0,            // Độ dài image con đường
            PreviousLocationOfTrex = 0;     // Vị trí trước đó của T-rex

        private static List<int> PreviousLocationOfCacti = new List<int>(); // Vị trí trước đó của các cây xương rồng mọc trên đường, dùng để xoá vết in khi các cây xương rồng dịch chuyển
        private static List<int> PreviousLocationOfPteroes = new List<int>(); // Vị trí trước đó của thằn lằn bay, tương tự xương rồng

        static int[] vitri = { 1, Console.BufferHeight / 2 + 3 }; // Vị trí gốc cho toàn bộ đối tượng

        // Cho một luồng truy cập vào một đoạn code khi sử dụng với từ khoá 'lock'
        // Khi luồng này truy cập vào thì các luồng khác sẽ đợi khi luồng đó truy cập xong rồi mới được truy cập
        private static readonly object locker = new object();

        public static void Trex(int jump, int run, ref KeyValuePair<int[], bool[,]> ShapOfTrex)
        {
            string[][] ImagesOfTrex = new string[3][];
            ImagesOfTrex[0] = new string[]
            {
                "        ▄▄▄▄ ",
                "        █▄███",
                "█▄    ▄████  ",
                " ██▄▄▄████▄  ",
                "  ▀██████▀ ▀ ",
                "     █ █▄    "
            };
            ImagesOfTrex[1] = new string[]
            {
                "        ▄▄▄▄ ",
                "        █▄███",
                "█▄    ▄████  ",
                " ██▄▄▄████▄  ",
                "  ▀██████▀ ▀ ",
                "     ▀ █▄    "
            };
            ImagesOfTrex[2] = new string[]
            {
                "        ▄▄▄▄ ",
                "        █▄███",
                "█▄    ▄████  ",
                " ██▄▄▄████▄  ",
                "  ▀██████▀ ▀ ",
                "     █ ▀▀    "
            };

            TheNumberOfTrexImages = ImagesOfTrex.Length;

            // Set bitmap cho khủng long
            bool[,] Bitmap = new bool[ImagesOfTrex[0].Length, ImagesOfTrex[0][0].Length];

            for (int i = 0; i < Bitmap.GetLength(0); i++)
                for (int j = 0; j < Bitmap.GetLength(1); j++)
                    if (ImagesOfTrex[0][i][j] != ' ') Bitmap[i, j] = true;

            int[] Location = { vitri[0] + 14, vitri[1] - 1 - jump };
            ShapOfTrex = new KeyValuePair<int[], bool[,]>(Location, Bitmap);

            // Xoá vết in của khủng long
            if (PreviousLocationOfTrex != 0) Clear(0, 0, 0, 0, "Trexrun");
            if (PreviousLocationOfTrex != 0 && PreviousLocationOfTrex < jump) Clear(0, 0, 0, 0, "Trexjump" + (jump - 1));
            if (PreviousLocationOfTrex != 0 && PreviousLocationOfTrex > jump) Clear(0, 0, 0, 0, "Trexjump" + (jump + 1));

            // In ra khủng long
            if (jump == 0)
                Print(vitri[0] + 14, vitri[1] - 1, ImagesOfTrex[run], "Trexrun");
            else
                Print(vitri[0] + 14, vitri[1] - 1 - jump, ImagesOfTrex[0], "Trexjump" + jump);

            PreviousLocationOfTrex = jump;
        }
        public static void Ptero(int initialization, bool spawn, ref KeyValuePair<int[][], bool[][,]> ShapOfPteroes)
        {
            string[][] Ptero = new string[2][];
            Ptero[0] = new string[]
            {
                "             ",
                "     █▄      ",
                "   ▄ ███     ",
                "▄███▄ ███    ",
                "   ▀██████▄▄ ",
                "     ▀▀▀▀▄▄ ▀",
                "             ",
                "             ",
            };
            Ptero[1] = new string[]
            {
                "             ",
                "             ",
                "   ▄         ",
                "▄███▄ ▄▄▄    ",
                "   ▀██████▄▄ ",
                "     ███▀▄▄ ▀",
                "    ██▀      ",
                "    ▀        "
            };

            // Set bitmap cho thằn lằn bay
            int[][] Location = new int[PreviousLocationOfPteroes.Count][];
            bool[][,] Bitmap = new bool[PreviousLocationOfPteroes.Count][,];

            int usepicture = 0; // Sử dụng pic quạt cánh lên hay xuống, tạo hoạt ảnh cho thằn lằn bay

            for (int i = 0; i < PreviousLocationOfPteroes.Count; i++)
            {
                Location[i] = new int[] { vitri[0] + PreviousLocationOfPteroes[i], vitri[1] - Program.maxjump - 2 };
                if (PreviousLocationOfPteroes[i] % 12 < 4) usepicture = 1;

                for (int j = 0; j < Ptero[usepicture].Length; j++)
                {
                    Bitmap[i] = new bool[Ptero[usepicture].Length, Ptero[usepicture].Length];
                    for (int k = 0; k < Ptero[usepicture].Length; k++)
                        if (Ptero[usepicture][j][k] != ' ')
                            Bitmap[i][j, k] = true;
                }
            }
            ShapOfPteroes = new KeyValuePair<int[][], bool[][,]>(Location, Bitmap);

            // Lệnh sinh ra thằn lằn bay
            if (spawn) PreviousLocationOfPteroes.Add(initialization);

            // In ra thằn lằn bay
            for (int i = 0; i < PreviousLocationOfPteroes.Count; i++)
            {
                Clear(0, 0, 0, 0, "Ptero" + i);

                lock (locker)
                {
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Print(vitri[0] - 12 + PreviousLocationOfPteroes[i] + PreviousLocationOfPteroes[i], vitri[1] - Program.maxjump - 2, Ptero[usepicture], "Ptero" + i);
                    Console.ResetColor();
                }
                PreviousLocationOfPteroes[i]--;

                // Xoá thằn lằn bay đã bay hết đường
                if (PreviousLocationOfPteroes[i] <= 1)
                {
                    PreviousLocationOfPteroes.RemoveAt(i);
                    Clear(0, 0, 0, 0, "Ptero" + PreviousLocationOfPteroes.Count);
                }
            }
        }
        public static void Cactus(int initialization, byte type, bool spawn, ref KeyValuePair<int[][], bool[][,]> ShapOfCacti)
        {
            string[][] Cactus = new string[1][];
            Cactus[0] = new string[]
            {
                "  ▄▄  ",
                "█ ██  ",
                "█▄██ █",
                "  ██▀▀",
                "  ██  "
            };

            // Set bitmap cho xương rồng
            int[][] Location = new int[PreviousLocationOfCacti.Count][];
            bool[][,] Bitmap = new bool[PreviousLocationOfCacti.Count][,];

            for (int i = 0; i < PreviousLocationOfCacti.Count; i++)
            {
                Location[i] = new int[] { vitri[0] - 12 + PreviousLocationOfCacti[i], vitri[1] };
                for (int j = 0; j < Cactus[type].Length; j++)
                {
                    Bitmap[i] = new bool[Cactus[type].Length, Cactus[type][j].Length];
                    for (int k = 0; k < Cactus[type][j].Length; k++)
                        if (Cactus[type][j][k] != ' ')
                            Bitmap[i][j, k] = true;
                }
            }
            ShapOfCacti = new KeyValuePair<int[][], bool[][,]>(Location, Bitmap);

            // Lệnh sinh ra xương rồng
            if (spawn) PreviousLocationOfCacti.Add(initialization);

            // In các cây xương rồng
            for (int i = 0; i < PreviousLocationOfCacti.Count; i++)
            {
                Clear(0, 0, 0, 0, "Cactus" + i);

                lock (locker)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Print(vitri[0] - 6 + PreviousLocationOfCacti[i], vitri[1], Cactus[type], "Cactus" + i);
                    Console.ResetColor();
                }
                PreviousLocationOfCacti[i]--;

                // Xoá cây xương rồng đã chạy đến cuối đường
                if (PreviousLocationOfCacti[i] <= 1)
                {
                    PreviousLocationOfCacti.RemoveAt(i);
                    Clear(0, 0, 0, 0, "Cactus" + PreviousLocationOfCacti.Count);
                }
            }
        }
        public static void Road(int move)
        {
            string[] road = new string[]
            {
                "▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄   ▄▄▄▄▄▄▄▄▄▀▀▀█▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄",
                "     ▀  ▄▄       ▄▄           ▀▀▀                  ▄▄               ",
                "▀▀                                   █                              "
            };
            LenghtOfTheRoad = road[0].Length;

            // In ra con đường
            for (int i = 0; i < road.Length; i++)
            {
                int k = move;
                for (int j = 0; j < Console.BufferWidth - 1; j++)
                    lock (locker)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        if (k >= road[0].Length) k = 0;
                        Print(vitri[0] + j, 5 + vitri[1] + i, road[i][k].ToString(), "");
                        k++;
                        Console.ResetColor();
                    }
            }
        }
        public static void Batdau()
        {
            string[] batdau = new string[]
            {
                "┏━━━━━━━┓",
                "┃ Space ┃",
                "┗━━━━━━━┛"
            };
            for (int i = 0; i < batdau.Length; i++)
                Print((Console.BufferWidth - batdau[0].Length) / 2, (Console.BufferHeight - batdau.Length) / 2 + i, batdau[i], "");
        }
        public static void Gameover()
        {
            string[] gameover = new string[]
            {
                "G A M E  O V E R !",
                "    ┏━━━━━━━┓     ",
                "    ┃ Enter ┃     ",
                "    ┗━━━━━━━┛     ",
                "   Lưu kết quả    "
            };
            for (int i = 0; i < gameover.Length; i++)
                Print((Console.BufferWidth - gameover[0].Length) / 2, (Console.BufferHeight - gameover.Length) / 2 + i, gameover[i], "");
        }
        public static void Luukq()
        {
            string[] luukq = new string[]
            {
                "┏━━━━━━━━━━━━━━━━━━━━━━━┓",
                "┃ Tên:                  ┃",
                "┗━━━━━━━━━━━━━━━━━━━━━━━┛",
            };
        }
        public static void Bangxephang(Dictionary<string, int> Diemso, byte hang)
        {
            byte x = 4, y = 2;
            string[] bangxephang = new string[]
            {
                "                     BẢNG XẾP HẠNG                    ",
                "┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓",
                "┃                                                  ▲ ┃",
                "┃                                                    ┃",
                "┃                                                    ┃",
                "┃                                                    ┃",
                "┃                                                    ┃",
                "┃                                                    ┃",
                "┃                                                    ┃",
                "┃                                                    ┃",
                "┃                                                    ┃",
                "┃                                                    ┃",
                "┃                                                    ┃",
                "┃                                                    ┃",
                "┃                                                    ┃",
                "┃                                                  ▼ ┃",
                "┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛",
            };
            int vt1 = (Console.BufferWidth - bangxephang[0].Length) / 2,
                vt2 = (Console.BufferHeight - bangxephang.Length) / 2;

            for (int i = 0; i < bangxephang.Length; i++)
                Print(vt1, vt2 + i, bangxephang[i], "");

            Clear(vt1 + x, vt2 + y, bangxephang[0].Length - 2 * x, bangxephang.Length - y - 1, "");
            for (int i = 0; i < Diemso.LongCount(); i++) ;
        }

        // Dùng để lưu một hình đã in, sau đó hàm Clear() sử dụng lại để xoá nhanh vết in đó.
        private static Dictionary<string, int[,]> SavePrints = new Dictionary<string, int[,]>();

        // In ra một hình với vị trí (x, y), nếu cần lưu lại trong 'SavePrint' thì đặt tên cho 'NameImage', nếu không thì đặt trống: ""
        public static void Print<T>(int x, int y, T print, string NameImage)
        {
            lock (locker)
            {
                int[,] MarkOfPrint = new int[0, 0];
                if (print is string)
                {
                    if (x >= 0 && y >= 0 && x + ((dynamic)print).Length < Console.BufferWidth && y < Console.BufferHeight)
                    {
                        Console.SetCursorPosition(x, y);
                        Console.Write(print);

                        MarkOfPrint = new int[1, 3];
                        MarkOfPrint[0, 0] = x;
                        MarkOfPrint[0, 1] = y;
                        MarkOfPrint[0, 2] = ((dynamic)print).Length;
                    }
                }
                if (print is string[])
                {
                    MarkOfPrint = new int[((dynamic)print).Length, 3];
                    for (int i = 0; i < ((dynamic)print).Length; i++)
                    {
                        if (x >= 0 && y >= 0 && x + ((dynamic)print)[i].Length < Console.BufferWidth && y + ((dynamic)print).Length < Console.BufferHeight)
                        {
                            Console.SetCursorPosition(x, y + i);
                            Console.Write(((dynamic)print)[i]);

                            MarkOfPrint[i, 0] = x;
                            MarkOfPrint[i, 1] = y + i;
                            MarkOfPrint[i, 2] = ((dynamic)print)[i].Length;
                        }
                    }
                }
                if (!(print is string) && !(print is string[]))
                    throw new Exception("Kiểu dữ liệu không hợp lệ, phải là kiểu string hoặc string[].");

                if (NameImage != "" && MarkOfPrint != null)
                {
                    if (!SavePrints.ContainsKey(NameImage))
                        SavePrints.Add(NameImage, MarkOfPrint);
                }
            }
        }
        // Xoá một khoảng trên màn hình ở vị trí (x, y), độ dài chiều ngang, dọc (w, h); hoặc sử dụng 'NameImage' đã lưu để xoá nhanh một hình
        public static void Clear(int x, int y, int w, int h, string NameImage)
        {
            lock (locker)
            {
                string c = "";

                for (int i = 0; i < w; i++)
                    c += " ";

                for (int i = 0; i < h; i++)
                {
                    Console.SetCursorPosition(x, y + i);
                    Console.Write(c);
                }

                if (SavePrints.ContainsKey(NameImage))
                {
                    for (int i = 0; i < SavePrints[NameImage].GetLength(0); i++)
                        Clear(SavePrints[NameImage][i, 0], SavePrints[NameImage][i, 1], SavePrints[NameImage][i, 2], 1, "");
                    SavePrints.Remove(NameImage);
                }
            }
        }
    }
}
