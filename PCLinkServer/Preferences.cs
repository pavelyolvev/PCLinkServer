using System.Text.Json;

namespace PCLinkServer;

public struct Preference
{
    public bool isAllowedShutdown { get; set; }
    public bool isAllowedRestart { get; set; }
    public bool isAllowedSleep { get; set; }
    public bool startOnSystem { get; set; }
    public bool startMinimized { get; set; }
    public bool autoLaunch { get; set; }
    public bool isAllowedVideo { get; set; }

    public Preference(bool isAllowedShutdown, bool isAllowedRestart, bool isAllowedSleep, bool startOnSystem, bool startMinimized, bool autoLaunch, bool isAllowedVideo)
    {
        this.isAllowedShutdown = isAllowedShutdown;
        this.isAllowedRestart = isAllowedRestart;
        this.isAllowedSleep = isAllowedSleep;
        this.startOnSystem = startOnSystem;
        this.autoLaunch = autoLaunch;
        this.isAllowedVideo = isAllowedVideo;
    }
}
public class Preferences
{
    public static Preference GetPreference()
    {
        // Десериализация из JSON
        try
        {
            string jsonFromFile = File.ReadAllText("preferences.json");
            Preference loadedRecords = JsonSerializer.Deserialize<Preference>(jsonFromFile);
            return loadedRecords;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new Preference(false, false, false, false, false, false, false);
        }
    }
    public static bool SavePreference(Preference preference)
    {
        try
        {
            string json = JsonSerializer.Serialize(preference, new JsonSerializerOptions { WriteIndented = true });

            // Запись JSON в файл
            File.WriteAllText("preferences.json", json);
            
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            
            return false;
        }
    }

    public static bool UpdateShutdown(bool isAllowedShutdown)
    {
        Preference preference = GetPreference();
        preference.isAllowedShutdown = isAllowedShutdown;
        return SavePreference(preference);
    }
    public static bool UpdateRestart(bool isAllowedRestart)
    {
        Preference preference = GetPreference();
        preference.isAllowedRestart = isAllowedRestart;
        return SavePreference(preference);
    }
    public static bool UpdateSleep(bool isAllowedSleep)
    {
        Preference preference = GetPreference();
        preference.isAllowedSleep = isAllowedSleep;
        return SavePreference(preference);
    }
    public static bool UpdateStartMinimized(bool startMinimized)
    {
        Preference preference = GetPreference();
        preference.startMinimized = startMinimized;
        return SavePreference(preference);
    }
    public static bool UpdateAutoLaunch(bool autoLaunch)
    {
        Preference preference = GetPreference();
        preference.autoLaunch = autoLaunch;
        return SavePreference(preference);
    }
    public static bool UpdateVideo(bool isAllowedVideo)
    {
        Preference preference = GetPreference();
        preference.isAllowedVideo = isAllowedVideo;
        return SavePreference(preference);
    }
    public static bool UpdateStartOnSystem(bool startOnSystem)
    {
        Preference preference = GetPreference();
        preference.startOnSystem = startOnSystem;
        if (!SavePreference(preference)) return false;
        AutoStartManager.UpdateStartOnSystem(startOnSystem);
        return true;


    }
}