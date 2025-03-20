using System.Text.RegularExpressions;

interface IPowerNotifier
{
    void NotifyLowBattery();
}

class EmptyBatteryException : Exception { }
class EmptySystemException : Exception { }
class ArgumentException : Exception { }
class ConnectionException : Exception { }

abstract class Device
{
    public string Id { get; set; }
    public string Name { get; set; }
    public bool IsTurnedOn { get; set; }
    
    public abstract void TurnOn();
    public abstract override string ToString();
}

class Smartwatch : Device, IPowerNotifier
{
    private int _batteryPercentage;
    public int BatteryPercentage 
    {
        get => _batteryPercentage;
        set
        {
            if (value < 0 || value > 100)
                throw new ArgumentOutOfRangeException("Battery percentage must be between 0 and 100.");
            
            _batteryPercentage = value;
            if (_batteryPercentage < 20)
                NotifyLowBattery();
        }
    }
    
    public void NotifyLowBattery()
    {
        Console.WriteLine("Warning: Low battery!");
    }

    public override void TurnOn()
    {
        if (BatteryPercentage < 11)
            throw new EmptyBatteryException();
        
        BatteryPercentage -= 10;
        IsTurnedOn = true;
    }
    
    public override string ToString()
    {
        return $"Smartwatch [ID: {Id}, Name: {Name}, Turned On: {IsTurnedOn}, Battery: {BatteryPercentage}%]";
    }
}

class PersonalComputer : Device
{
    public string OperatingSystem { get; set; }
    
    public override void TurnOn()
    {
        if (string.IsNullOrEmpty(OperatingSystem))
            throw new EmptySystemException();
        
        IsTurnedOn = true;
    }
    
    public override string ToString()
    {
        return $"Personal Computer [ID: {Id}, Name: {Name}, Turned On: {IsTurnedOn}, OS: {OperatingSystem ?? "Not Installed"}]";
    }
}

class EmbeddedDevice : Device
{
    private static readonly Regex IpRegex = new(@"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$");
    
    private string _ipAddress;
    public string IpAddress 
    {
        get => _ipAddress;
        set
        {
            if (!IpRegex.IsMatch(value))
                throw new ArgumentException();
            
            _ipAddress = value;
        }
    }
    
    public string NetworkName { get; set; }
    
    public void Connect()
    {
        if (!NetworkName.Contains("MD Ltd."))
            throw new ConnectionException();
    }
    
    public override void TurnOn()
    {
        Connect();
        IsTurnedOn = true;
    }
    
    public override string ToString()
    {
        return $"Embedded Device [ID: {Id}, Name: {Name}, Turned On: {IsTurnedOn}, IP: {IpAddress}, Network: {NetworkName}]";
    }
}

class DeviceManager
{
    private const int MaxDevices = 15;
    private readonly List<Device> _devices = new();
    
    public DeviceManager(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException();
        
        var lines = File.ReadAllLines(filePath);
        foreach (var line in lines)
        {
            try
            {
                var parts = line.Split(',');
                Device device = parts[0] switch
                {
                    string id when id.StartsWith("SW") => new Smartwatch { Id = parts[0], Name = parts[1], IsTurnedOn = bool.Parse(parts[2]), BatteryPercentage = int.Parse(parts[3].TrimEnd('%')) },
                    string id when id.StartsWith("P") => new PersonalComputer { Id = parts[0], Name = parts[1], IsTurnedOn = bool.Parse(parts[2]), OperatingSystem = parts.Length > 3 ? parts[3] : null },
                    string id when id.StartsWith("ED") => new EmbeddedDevice { Id = parts[0], Name = parts[1], IpAddress = parts[2], NetworkName = parts[3] },
                    _ => null
                };
                
                if (device != null && _devices.Count < MaxDevices)
                    _devices.Add(device);
            }
            catch
            {
                
            }
        }
    }
    
    public void AddDevice(Device device)
    {
        if (_devices.Count >= MaxDevices)
            throw new InvalidOperationException("Storage capacity reached.");
        
        _devices.Add(device);
    }
    
    public void RemoveDevice(string deviceId)
    {
        _devices.RemoveAll(d => d.Id == deviceId);
    }
    
    public void EditDevice(string deviceId, Action<Device> editAction)
    {
        var device = _devices.FirstOrDefault(d => d.Id == deviceId);
        if (device != null)
            editAction(device);
    }
    
    public void PrintAllDevices()
    {
        foreach (var device in _devices)
        {
            Console.WriteLine(device);
        }
    }
}


class Program
{
    static void Main()
    {
        string filePath = "/Users/deryaogus/Desktop/APBD-HW-2/APBD-HW-2/input.txt";
        try
        {
            DeviceManager manager = new DeviceManager(filePath);
            Console.WriteLine("Devices loaded successfully!");
            manager.PrintAllDevices();
            manager.AddDevice(new Smartwatch { Id = "SW-2", Name = "Galaxy Watch", BatteryPercentage = 50 });
            Console.WriteLine("New device added!");
            manager.PrintAllDevices();

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
