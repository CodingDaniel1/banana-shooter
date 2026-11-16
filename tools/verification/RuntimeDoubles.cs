using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UnityEngine
{
    public class MonoBehaviour { }
    public class SerializeField : Attribute { }
    public class DefaultExecutionOrder : Attribute { public DefaultExecutionOrder(int order) { } }
    public struct Vector3 { public float x, y, z; }
    public struct Color
    {
        public float r, g, b, a;
        public Color(float r, float g, float b, float a = 1f) { this.r = r; this.g = g; this.b = b; this.a = a; }
    }
    public class AudioSource { public float volume; }
    public static class Time
    {
        public static float realtimeSinceStartup;
        public static float deltaTime = 1f / 60f;
    }
    public static class Mathf
    {
        public static float Floor(float value) => (float)Math.Floor(value);
        public static float Pow(float value, float power) => (float)Math.Pow(value, power);
        public static float Abs(float value) => Math.Abs(value);
        public static float Lerp(float a, float b, float t) => a + (b - a) * Math.Clamp(t, 0f, 1f);
        public static int Max(int a, int b) => Math.Max(a, b);
        public static float Max(float a, float b) => Math.Max(a, b);
    }
    public static class Application { public static string persistentDataPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "banana-verification", "saves"); }
    public static class Debug
    {
        public static readonly List<string> Errors = new List<string>();
        public static void Log(object value) { }
        public static void LogError(object value) => Errors.Add(value?.ToString());
        public static void LogWarning(object value) => Errors.Add(value?.ToString());
    }
    public class WaitForSeconds { public WaitForSeconds(float duration) { } }
    public static class JsonUtility
    {
        public static T FromJson<T>(string json) => throw new NotSupportedException("Unity JsonUtility is outside this runner's scope.");
        public static string ToJson(object value, bool prettyPrint = false) => throw new NotSupportedException("Unity JsonUtility is outside this runner's scope.");
    }
    public class AsyncOperation { public float progress = 0.9f; public bool allowSceneActivation; }
}
namespace UnityEngine.SceneManagement
{
    public enum LoadSceneMode { Single }
    public static class SceneManager
    {
        public static int Loads;
        public static UnityEngine.AsyncOperation LoadSceneAsync(string name, LoadSceneMode mode) { Loads++; return new UnityEngine.AsyncOperation(); }
    }
}
namespace UnityEngine.Networking
{
    public class DownloadHandlerBuffer { public string text; }
    public class UploadHandlerRaw { public UploadHandlerRaw(byte[] data) { } }
    public class UnityWebRequest : IDisposable
    {
        public enum Result { InProgress, Success, ConnectionError, ProtocolError, DataProcessingError }
        public static Result NextResult = Result.Success;
        public static string NextBody = "{}";
        public static UnityWebRequest Last;
        public static bool Stall;
        public static Exception SendException;
        public readonly string url, method;
        public int timeout;
        public bool isDone, Disposed;
        public long responseCode;
        public string error = "fixture error";
        public Result result;
        public DownloadHandlerBuffer downloadHandler;
        public UploadHandlerRaw uploadHandler;
        public UnityWebRequest(string url, string method) { this.url = url; this.method = method; Last = this; }
        public void SetRequestHeader(string name, string value) { }
        public void SendWebRequest()
        {
            if (SendException != null) throw SendException;
            result = NextResult; downloadHandler.text = NextBody; responseCode = result == Result.Success ? 200 : 503; isDone = !Stall;
        }
        public void Dispose() => Disposed = true;
    }
}
namespace Steamworks
{
    public static class SteamUser { public static ulong GetSteamID() => 1; }

    /// <summary>
    /// Stand-in for the Steam stats API. Mirrors the two behaviours the game has to
    /// survive: stats are not available before Steam sends them down, and writes can
    /// be rejected.
    /// </summary>
    public static class SteamUserStats
    {
        public static readonly Dictionary<string, int> IntStats = new Dictionary<string, int>();
        /// <summary>False while the user's stats have not been received yet.</summary>
        public static bool StatsLoaded = true;
        /// <summary>When false SetStat/StoreStats fail, as they do without a Steam connection.</summary>
        public static bool AcceptStore = true;

        public static bool GetStat(string name, out int data)
        {
            data = 0;
            if (!StatsLoaded) return false;
            return IntStats.TryGetValue(name, out data);
        }

        public static bool SetStat(string name, int data)
        {
            if (!AcceptStore) return false;
            IntStats[name] = data;
            return true;
        }

        public static bool SetStat(string name, float data)
        {
            if (!AcceptStore) return false;
            IntStats[name] = (int)data;
            return true;
        }

        public static bool StoreStats() => AcceptStore;

        public static bool RequestCurrentStats() => true;

        public static void Reset()
        {
            IntStats.Clear();
            StatsLoaded = true;
            AcceptStore = true;
        }
    }
}
namespace Steamworks.NET
{
    public static class SteamManager { public static bool Initialized = true; public static SecureServer.PlayerBanSummary CurrentUserBanSummary; }
}
namespace Menu
{
    public class PreloadMenu
    {
        public string Error;
        public void SetLoadingStateText(Manager.Preload.LoadingState state) { }
        public void SetProgress(int step, float progress) { }
        public void ShowLoadingError(string error) => Error = error;
    }
    public class TransitionUI
    {
        public static TransitionUI Instance = new TransitionUI();
        public void StartTransition() { }
        public void ClearTransition() { }
    }
}
namespace CodingDaniel.MapEditor.MEEditor.MESave { public static class MapSaver { public static bool Initialized = true; } }
namespace Quest { public static class QuestManager { public static bool Initialized = true; } }
namespace SteamWorkshop { public static class SteamWorkshopManager { public static bool Initialized = true; } }
namespace Manager
{
    public static class GameManager { public static bool Initialized = true; public enum PowerType { Boomer, Healer } }

    /// <summary>Stand-in for the production achievement manager's stat API.</summary>
    public class AchievementManager
    {
        public static AchievementManager Instance = new AchievementManager();

        public enum EStats
        {
            SNIPER_SHOT = 0,
            START_GAME_1000,
            DEATHS,
            AL48_SHOT,
            LASER_SHOT,
            KNIFE_KILL,
            WINS,
            TARGET_SCORE,
            PARKOUR_TIME,
            EXPERIENCE,
            KILLS,
            BANANA_KILL,
            MAP_PLAY_AMOUNT,
            GET_BLIND_AMOUNT
        }

        public enum StatsType
        {
            Int,
            Float
        }

        public int SetStatsPlusOneCalls;

        public bool SetStat(EStats stats, StatsType type, float score)
        {
            switch (type)
            {
                case StatsType.Int:
                    if (!Steamworks.SteamUserStats.SetStat(stats.ToString(), (int)score)) return false;
                    break;
                case StatsType.Float:
                    if (!Steamworks.SteamUserStats.SetStat(stats.ToString(), score)) return false;
                    break;
                default:
                    return false;
            }

            return Steamworks.SteamUserStats.StoreStats();
        }
    }
}
namespace Weapon
{
    public class WeaponStat { public uint bulletAmount = 1; }
    public class ActiveWeapon
    {
        public WeaponStat Stat = new WeaponStat();
        public int Attacks;
        public EShootingResult DoAttack() { Attacks++; return EShootingResult.EResultOk; }
    }
}
namespace Multiplayer.Entity.Interface
{
    public interface IPlayerServer
    {
        ushort Id { get; }
        bool IsInfected { get; }
        Weapon.ActiveWeapon GetCurrentWeapon();
        void DisableInvincible();
    }
}
namespace Multiplayer.Entity.Server
{
    public class ServerPlayer : Multiplayer.Entity.Interface.IPlayerServer
    {
        public static Dictionary<ushort, Multiplayer.Entity.Interface.IPlayerServer> list = new Dictionary<ushort, Multiplayer.Entity.Interface.IPlayerServer>();
        public ushort Id { get; set; }
        public bool IsInfected { get; set; }
        public Weapon.ActiveWeapon Weapon = new Weapon.ActiveWeapon();
        public void DisableInvincible() { }
        public Weapon.ActiveWeapon GetCurrentWeapon() => Weapon;
    }
}
namespace Riptide
{
    public enum MessageSendMode { Unreliable }
    public class MessageHandler : Attribute { public MessageHandler(ushort id, byte group) { } }
    public class Message
    {
        public static Message Create(MessageSendMode mode, ushort id) => new Message();
        public uint GetUInt() => 10;
        public UnityEngine.Vector3 GetVector3() => new UnityEngine.Vector3();
        public void Add(ushort value) { }
        public void Add(uint value) { }
        public void Add(UnityEngine.Vector3 value) { }
        public void AddInt(int value) { }
    }
}
namespace Multiplayer
{
    public enum ServerToClientId { Shoot }
    public enum ClientToServerId { Shoot }
    public class FakeServer
    {
        public HashSet<ushort> Connected = new HashSet<ushort>();
        public int Sent;
        public bool TryGetClient(ushort id, out object client) { client = null; return Connected.Contains(id); }
        public void SendToAll(Riptide.Message message) => Sent++;
    }
    public class NetworkServerManager
    {
        public const byte PlayerHostedDemoMessageHandlerGroupId = 255;
        public static NetworkServerManager Instance = new NetworkServerManager();
        public static NetworkServerManager Iinstance => Instance;
        public FakeServer Server = new FakeServer();
        public int Hits;
        public void ShootLagCompensation(uint tick, UnityEngine.Vector3 direction, UnityEngine.Vector3 origin, ushort id, Weapon.ActiveWeapon weapon) => Hits++;
    }
    public class LeaderboardManager
    {
        public static LeaderboardManager Instance = new LeaderboardManager();
        public static bool Initialized = true;
        public void Refresh() { }
    }
}
