namespace PCLinkServer;
using System.Text.Json;
public struct AccessRecord
{
    public int Id { get; set; }
    public string Ip { get; set; }
    public string AccessMode { get; set; }
    public int AuthCode { get; set; }

    public AccessRecord(int Id, string Ip, string AccessMode, int AuthCode)
    {
        this.Id = Id;
        this.Ip = Ip;
        this.AccessMode = AccessMode;
        this.AuthCode = AuthCode;
    }
}

public class Authentification()
{
    public int Code { get; set; }

    public bool SaveNewRecord(string ip, string accessMode, int code)
    {
        try
        {
            List<AccessRecord> accessList = GetAllRecords();
            try
            {
                var record = accessList.Find(record => record.Ip == ip);
                UpdateRecord(record.Id, ip, accessMode, code);
                record.AuthCode = code;
                record.Ip = ip;
                record.AccessMode = accessMode;
                
                accessList[accessList.FindIndex(record => record.Ip == ip)] = record;
                Console.WriteLine("Edited?");
                Console.WriteLine(record.AuthCode);
                Console.WriteLine(accessList.Find(record => record.Ip == ip).AuthCode);
                // accessList.Add(new AccessRecord(accessList.Count, ip, accessMode, code));  
            }
            catch (Exception e)
            {
                accessList.Add(new AccessRecord(accessList.Count, ip, accessMode, code));    
            }

            
            string json = JsonSerializer.Serialize(accessList, new JsonSerializerOptions { WriteIndented = true });

            // Запись JSON в файл
            File.WriteAllText("access_records.json", json);
            
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            
            return false;
        }
        
    }
    public bool UpdateRecord(int id, string ip, string accessMode, int code)
    {
        try
        {
            List<AccessRecord> accessList = GetAllRecords();
            accessList.Add(new AccessRecord(accessList.Count, ip, accessMode, code));

            if (accessList.Remove(GetRecordById(id)))
            {
                AccessRecord recordToUpdate = new AccessRecord(id, ip, accessMode, code);
                string json = JsonSerializer.Serialize(accessList, new JsonSerializerOptions { WriteIndented = true });

                // Запись JSON в файл
                File.WriteAllText("access_records.json", json);
            
                return true;    
            }

            return false;

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            
            return false;
        }
        
    }

    public List<AccessRecord> GetAllRecords()
    {
        // Десериализация из JSON
        try
        {
            string jsonFromFile = File.ReadAllText("access_records.json");
            List<AccessRecord> loadedRecords = JsonSerializer.Deserialize<List<AccessRecord>>(jsonFromFile) ?? throw new InvalidOperationException();
            return loadedRecords;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new List<AccessRecord>();
        }

    }
    public AccessRecord GetRecordById(int id)
    {
        // Десериализация из JSON
        try
        {
            string jsonFromFile = File.ReadAllText("access_records.json");
            List<AccessRecord> loadedRecords = JsonSerializer.Deserialize<List<AccessRecord>>(jsonFromFile) ?? throw new InvalidOperationException();
            AccessRecord rec = new AccessRecord();
            loadedRecords.ForEach(record =>
            {
                if(record.Id == id)
                    rec = record;
            });
            return rec;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new AccessRecord();
        }

    }
    public AccessRecord? GetRecordByIp(string ip)
    {
        // Десериализация из JSON
        try
        {
            string jsonFromFile = File.ReadAllText("access_records.json");
            List<AccessRecord> loadedRecords = JsonSerializer.Deserialize<List<AccessRecord>>(jsonFromFile) ?? throw new InvalidOperationException();
            AccessRecord rec = new AccessRecord();
            loadedRecords.ForEach(record =>
            {
                if(record.Ip.Equals(ip))
                    rec = record;
            });
            return rec;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }

    }
    
    public int GenerateCode()
    {
        Random random = new Random();
        Code = random.Next(1000,9999);
        return Code;
        
    }

    public bool ReadCode(string code)
    {
        if (code.Equals(Code.ToString()))
            return true;
        return false;
    }
}