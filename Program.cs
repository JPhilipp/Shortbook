using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using Tesseract;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Media;

#pragma warning disable CS0219 // Warning for unused variables, as we're commenting out things.
#pragma warning disable CS1998 // Warning for async method without await, as we're commenting out things.
#pragma warning disable CA1416 // Warning for reachable on all platforms, as we're commenting out things.
#pragma warning disable CS8602 // Warning for dereferencing a possibly null reference.

public class Program
{
    [DllImport("user32.dll")]
    public static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    static IntPtr windowHandle = IntPtr.Zero;

    const int maxChunkSizeKb = 10;
    const int chunksAtATimeToKeepToRateLimit = 5;
    static OpenAiChatBookService? chatService;

    public static async Task Main()
    {
        // --- Set below values accordingly for each book you want to shorten. ---

        const string bookName = "SomeBookName";
        const int maxPages = 64;

        // keyPath should point to a text file containing your OpenAI API key.
        // Note that this will make API requests that cost money, use at your own risk.
        chatService = new OpenAiChatBookService(keyPath: @"D:\_misc\openai-key.txt");

        // chatService.summaryLanguage = "German";
        // chatService.translationLanguage = "German";
        // chatService.additionalSummaryInstructions = "Write in the style of Douglas Adams.";
        // chatService.addSummaryHeadlines = true;
        // chatService.addSummaryImages = true;

        const string projectFolder = "D:\\Shortbook";
        const string dataFolder = projectFolder + "\\Data\\" + bookName;
        const string screenshotsFolder = dataFolder + "\\Screenshots";
        const string textFile = dataFolder + "\\Text.txt";
        const string tessdataFolder = projectFolder + "\\tessdata";

        const string summariesFolder = dataFolder + "\\Summaries";
        const string summariesFolderGerman = dataFolder + "\\SummariesGerman";
        const string originalsFolder = dataFolder + "\\Originals";
        const string allFile = dataFolder + "\\Summary.txt";
        const string allFileGerman = dataFolder + "\\SummaryGerman.txt";

        System.IO.Directory.CreateDirectory(screenshotsFolder);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // --- Use below one by one, as this still requires some hand-holding and checks. ---

            // SavePagesAsImages(screenshotsFolder, maxPages);

            // SaveTextOfImages(tessdataFolder, screenshotsFolder, textFile, maxPages, TesseractLanguage.Languages.English);
            
            // await SaveSummariesOfText(textFile, originalsFolder, summariesFolder);

            // CombineSummariesIntoSingleFile(summariesFolder, allFile);

            // Note instead of translating the summaries (as defined by chatService.translationLanguage),
            // you can also directly have the summaries be generated in your target language (as defined
            // by chatService.summaryLanguage).
            
            // CombineSummariesIntoSingleFile(summariesFolderGerman, allFileGerman);
        }
        
        Console.WriteLine("Done.");
    }

    static async Task TranslateSummaries(string summariesFolder, string summariesFolderGerman)
    {
        Directory.CreateDirectory(summariesFolderGerman);

        var files = Directory.GetFiles(summariesFolder).OrderBy(f => int.Parse(Path.GetFileNameWithoutExtension(f))).ToArray();

        for (int chunkFrom = 0; chunkFrom < files.Length; chunkFrom += chunksAtATimeToKeepToRateLimit)
        {
            var tasks = new List<Task>();
            int chunkTo = chunkFrom + chunksAtATimeToKeepToRateLimit;

            for (int i = chunkFrom; i < chunkTo && i < files.Length; i++)
            {
                var file = files[i];
                string fileName = Path.GetFileName(file);

                string translationPath = Path.Combine(summariesFolderGerman, fileName);
                if (!File.Exists(translationPath))
                {
                    tasks.Add(TranslateFile(file, translationPath));
                }
            }

            await Task.WhenAll(tasks);
        }
    }

    static async Task TranslateFile(string filePath, string translationPath)
    {
        Console.WriteLine($"Translating \"{filePath}\" into \"{translationPath}\"");

        string summary = await File.ReadAllTextAsync(filePath);
        string translation = await chatService.GetTranslation(summary);
        await File.WriteAllTextAsync(translationPath, translation);
    }

    static void CombineSummariesIntoSingleFile(string summariesFolder, string allFile)
    {
        Console.WriteLine("Starting CombineSummaries...");

        var files = Directory.GetFiles(summariesFolder).OrderBy(f => int.Parse(Path.GetFileNameWithoutExtension(f)));

        var summaries = new List<string>();

        foreach (var file in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            if (int.TryParse(fileName, out int _))
            {
                string summary = File.ReadAllText(file);

                summary = summary.Replace("[...]", "...");
                summary = summary.Replace("(...)", "...");

                summaries.Add(summary);
            }
        }

        File.WriteAllText(allFile, string.Join("\n\n", summaries));
    }

    static async Task SaveSummariesOfText(string textFile, string originalsFolder, string summariesFolder)
    {
        Console.WriteLine("Starting SaveSummariesOfText...");
        string text = await File.ReadAllTextAsync(textFile);
        Console.WriteLine("Got text...");

        string[] chunks = SplitBookIntoChunks(text);
        Console.WriteLine("Got chunks...");

        await SummarizeBookChunks(originalsFolder, summariesFolder, chunks);
    }

    static async Task SummarizeBookChunks(string originalsFolder, string summariesFolder, string[] bookChunks)
    {
        // bookChunks = new string[] { bookChunks[0] };

        for (int chunkFrom = 0; chunkFrom < bookChunks.Length; chunkFrom += chunksAtATimeToKeepToRateLimit)
        {
            var tasks = new List<Task>();

            int chunkTo = chunkFrom + chunksAtATimeToKeepToRateLimit;

            for (int i = chunkFrom; i < chunkTo && i < bookChunks.Length; i++)
            {
                string chunk = bookChunks[i];
                string fileName = (i + 1) + ".txt";

                if (!File.Exists(summariesFolder + "\\" + fileName))
                {
                    var task = SummarizeAndWriteFile(chunk, originalsFolder, summariesFolder, fileName, i + 1, bookChunks.Length);
                    tasks.Add(task);
                }
            }

            await Task.WhenAll(tasks);
        }

        Console.WriteLine("SummarizeBookChunks done!");
        
        PlaySuccessSound();
    }

    static void PlaySuccessSound()
    {
        const string soundPath = ".\\sounds\\success.wav";
        using (var soundPlayer = new SoundPlayer(soundPath))
        {
            Console.WriteLine($"Playing success sound: {soundPath}");
            soundPlayer.PlaySync();
        }
    }

    static async Task SummarizeAndWriteFile(string chunk, string originalsFolder, string summariesFolder, string fileName, int chunkNumber, int totalChunks)
    {
        Directory.CreateDirectory(originalsFolder);
        Directory.CreateDirectory(summariesFolder);

        Console.WriteLine($"Summarizing {chunkNumber} of {totalChunks}");
        await File.WriteAllTextAsync(originalsFolder + "\\" + fileName, chunk);

        string summary = await chatService.GetSummary(chunk);
        await File.WriteAllTextAsync(summariesFolder + "\\" + fileName, summary);
    }

    static string[] SplitBookIntoChunks(string text)
    {
        string[] sections = text.Split(new string[] { "\n\n" }, StringSplitOptions.None);
        List<string> chunks = new List<string>();
        StringBuilder currentChunk = new StringBuilder();

        foreach (string section in sections)
        {
            if (Encoding.UTF8.GetByteCount(currentChunk.ToString() + "\n\n" + section) / 1024.0 > maxChunkSizeKb)
            {
                chunks.Add(currentChunk.ToString());
                currentChunk.Clear();
            }

            if (currentChunk.Length > 0)
            {
                currentChunk.Append("\n\n");
            }

            currentChunk.Append(section);
        }

        if (currentChunk.Length > 0)
        {
            chunks.Add(currentChunk.ToString());
        }

        return chunks.ToArray();
    }

    static void SavePagesAsImages(string folder, int maxPages)
    {
        const bool overwriteOld = false;

        EnumWindows(FindAndFrontKindleWindow, IntPtr.Zero);
     
        var sim = new WindowsInput.InputSimulator();

        for (int i = 1; i <= maxPages; i++)
        {
            if (i > 1)
            {
                sim.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.RIGHT);
                System.Threading.Thread.Sleep(100);
            }

            string filePath = folder + "\\" + i + ".png";
            if (overwriteOld || !System.IO.File.Exists(filePath))
            {
                var image = ScreenCapture.CaptureDesktop(56, 102, 1905, 1030);
                image.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
            }
        }
    }

    static void SaveTextOfImages(string tessdataFolder, string screenshotsFolder, string textFile, int maxPages, TesseractLanguage.Languages language = TesseractLanguage.Languages.English)
    {
        var ocr = new Tesseract.TesseractEngine(
            tessdataFolder, TesseractLanguage.GetLanguageCode(language), Tesseract.EngineMode.Default
        );

        var sb = new StringBuilder();

        for (int i = 1; i <= maxPages; i++)
        {
            string filePath = screenshotsFolder + "\\" + i + ".png";
            var img = Pix.LoadFromFile(filePath);
            using (var page = ocr.Process(img))
            {
                sb.AppendLine(page.GetText());
            }
            Console.WriteLine("OCR page " + i + "/ " + maxPages + " done.");
        }

        System.IO.File.WriteAllText(textFile, sb.ToString());
    }

    public static bool FindAndFrontKindleWindow(IntPtr hWnd, IntPtr lParam)
    {
        const string titlePartToMatch = "Kindle for PC";

        int size = GetWindowTextLength(hWnd);
        if (size++ > 0 && IsWindowVisible(hWnd))
        {
            StringBuilder sb = new StringBuilder(size);
            GetWindowText(hWnd, sb, size);
            if (sb.ToString().Contains(titlePartToMatch))
            {
                SetForegroundWindow(hWnd);
                windowHandle = hWnd;
                SetForegroundWindow(hWnd);
                return false;
            }
        }
        return true;
    }
}
