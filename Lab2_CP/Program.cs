// Томилин ИСП-212
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

class Program
{
    // Импортируем необходимые функции из библиотеки kernel32.dll
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr CreateFile(
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        IntPtr lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        IntPtr hTemplateFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr CreateFileMapping(
        IntPtr hFile,
        IntPtr lpFileMappingAttributes,
        uint flProtect,
        uint dwMaximumSizeHigh,
        uint dwMaximumSizeLow,
        string lpName);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr MapViewOfFile(
        IntPtr hFileMappingObject,
        uint dwDesiredAccess,
        uint dwFileOffsetHigh,
        uint dwFileOffsetLow,
        UIntPtr dwNumberOfBytesToMap);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

    const uint GENERIC_READ = 0x80000000;
    const uint GENERIC_WRITE = 0x40000000;
    const uint OPEN_EXISTING = 3;
    const uint PAGE_READWRITE = 0x04;
    const uint FILE_MAP_ALL_ACCESS = 0xF001F;

    static void Main(string[] args)
    {
        string filePath = "example.txt";

        // Создаем текстовый файл и записываем в него пример текста
        File.WriteAllText(filePath, "poiuytrewqlkjhgfdsamnbvcxz");

        // Открываем файл
        IntPtr hFile = CreateFile(filePath, GENERIC_READ | GENERIC_WRITE, 0, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
        if (hFile == IntPtr.Zero)
        {
            Console.WriteLine("Не удалось открыть файл. Код ошибки: " + Marshal.GetLastWin32Error());
            return;
        }

        // Выводим дескриптор файла
        Console.WriteLine("Дескриптор файла: " + hFile.ToString("X")); // Вывод в шестнадцатеричном формате

        // Создаем отображение файла
        IntPtr hMapping = CreateFileMapping(hFile, IntPtr.Zero, PAGE_READWRITE, 0, 0, null);
        if (hMapping == IntPtr.Zero)
        {
            Console.WriteLine("Не удалось создать отображение файла. Код ошибки: " + Marshal.GetLastWin32Error());
            CloseHandle(hFile);
            return;
        }

        // Отображаем файл в память
        IntPtr pView = MapViewOfFile(hMapping, FILE_MAP_ALL_ACCESS, 0, 0, UIntPtr.Zero);
        if (pView == IntPtr.Zero)
        {
            Console.WriteLine("Не удалось отобразить файл в память. Код ошибки: " + Marshal.GetLastWin32Error());
            CloseHandle(hMapping);
            CloseHandle(hFile);
            return;
        }

        // Читаем данные из отображенного файла
        byte[] buffer = new byte[1024];
        Marshal.Copy(pView, buffer, 0, buffer.Length);
        string fileContent = Encoding.UTF8.GetString(buffer).TrimEnd('\0');

        Console.WriteLine("Содержимое файла: " + fileContent);

        // Упорядочиваем буквы по алфавиту
        char[] characters = fileContent.ToCharArray();
        Array.Sort(characters);
        string sortedContent = new string(characters);

        // Записываем отсортированное содержимое обратно в файл
        byte[] sortedBytes = Encoding.UTF8.GetBytes(sortedContent);
        Marshal.Copy(sortedBytes, 0, pView, sortedBytes.Length);

        // Освобождаем ресурсы
        UnmapViewOfFile(pView);
        CloseHandle(hMapping);
        CloseHandle(hFile);

        Console.WriteLine("Отсортированное содержимое записано в файл: " + sortedContent);
    }
}
