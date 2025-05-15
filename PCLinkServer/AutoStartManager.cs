namespace PCLinkServer;

using Microsoft.Win32;
using System;
using System.Windows.Forms;

public static class AutoStartManager
{
    private const string AppName = "PCLink"; // Имя, под которым будет зарегистрировано приложение в автозапуске
    private static readonly string AppPath = Application.ExecutablePath; // Путь к текущему exe-файлу

    public static void UpdateStartOnSystem(bool startOnSystem)
    {
        try
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Run", true))
            {
                if (startOnSystem)
                {
                    // Добавляем в автозапуск
                    key.SetValue(AppName, "\"" + AppPath + "\"");
                }
                else
                {
                    // Удаляем из автозапуска
                    if (key.GetValue(AppName) != null)
                        key.DeleteValue(AppName);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка при изменении автозапуска: " + ex.Message);
        }
    }

    // Опционально: получить текущее состояние автозапуска
    public static bool IsStartOnSystemEnabled()
    {
        using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
            @"Software\Microsoft\Windows\CurrentVersion\Run", false))
        {
            var value = key?.GetValue(AppName) as string;
            return value != null && value.Contains(AppPath);
        }
    }
}
