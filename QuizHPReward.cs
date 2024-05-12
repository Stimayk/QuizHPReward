using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Newtonsoft.Json;
using QuizApi;

namespace QuizHPReward
{
    public class QuizHPReward : BasePlugin
    {
        public override string ModuleName => "QuizHPReward";
        public override string ModuleAuthor => "E!N";
        public override string ModuleVersion => "v1.0";

        private IQuizApi? QUIZ_API;
        private QuizHPRewardConfig? _config;
        private int _Min;
        private int _Max;

        public override void OnAllPluginsLoaded(bool hotReload)
        {
            string configDirectory = GetConfigDirectory();
            EnsureConfigDirectory(configDirectory);
            string configPath = Path.Combine(configDirectory, "QuizHPRewardConfig.json");
            _config = QuizHPRewardConfig.Load(configPath);

            QUIZ_API = IQuizApi.Capability.Get();

            if (QUIZ_API == null)
            {
                Console.WriteLine($"{ModuleName} | Error: Quiz API is not available.");
                return;
            }

            InitializeQuizShopReward();

            QUIZ_API.OnPlayerWin += HandlePlayerWin;
        }

        private static string GetConfigDirectory()
        {
            return Path.Combine(Server.GameDirectory, "csgo/addons/counterstrikesharp/configs/plugins/Quiz/Modules");
        }

        private void EnsureConfigDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                Console.WriteLine($"{ModuleName} | Created configuration directory at: {directoryPath}");
            }
        }

        private void InitializeQuizShopReward()
        {
            if (_config == null)
            {
                Console.WriteLine($"{ModuleName} | Error: Configuration is not loaded.");
                return;
            }

            _Min = _config.WinMin;
            _Max = _config.WinMax;
            Console.WriteLine($"{ModuleName} | Initialized: Min = {_Min}, Max = {_Max}");
        }

        private void HandlePlayerWin(CCSPlayerController player)
        {
            if (QUIZ_API != null)
            {
                Server.NextFrame(() =>
                {
                    if (player.PlayerPawn.Value != null)
                    {
                        int reward = new Random().Next(_Min, _Max);
                        player.PlayerPawn.Value.Health += reward;
                        Utilities.SetStateChanged(player, "CBaseEntity", "m_iHealth");
                        player.PrintToChat($"{Localizer["RewardWin", QUIZ_API.GetTranslatedText("Prefix"), reward]}");
                    }
                });
            }
        }
    }

    public class QuizHPRewardConfig
    {
        public int WinMin { get; set; } = 1;
        public int WinMax { get; set; } = 50;

        public static QuizHPRewardConfig Load(string configPath)
        {
            if (!File.Exists(configPath))
            {
                QuizHPRewardConfig defaultConfig = new();
                File.WriteAllText(configPath, JsonConvert.SerializeObject(defaultConfig, Formatting.Indented));
                return defaultConfig;
            }

            string json = File.ReadAllText(configPath);
            return JsonConvert.DeserializeObject<QuizHPRewardConfig>(json) ?? new QuizHPRewardConfig();
        }
    }
}
