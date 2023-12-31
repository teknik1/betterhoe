using Vintagestory.API.Common;

namespace BetterHoe.Tools.Houe
{
    static class ItemHoeEDITConfig
    {
        private const int CurrentConfigVersion = 1; // Attention changé a chaque maj du fichier config ! Ici et dans la méthode internal class BetterHoeConfig !!!
        private static BetterHoeConfig _HoeConfig;  // Cache pour la config
        private const string ConfigFileName = "BetterHoeConfig.json";
        private static ICoreAPI _api;

        public static void Initialize(ICoreAPI api)
        {
            _api = api;
        }

        public static void ReadConfig()
        {
            if (_HoeConfig == null)
            {
                _HoeConfig = LoadConfig(_api); // Utilise la référence statique _api
                if (_HoeConfig == null || _HoeConfig.ConfigVersion != CurrentConfigVersion)
                {
                    _api.Logger.Warning("The BetterHoeConfig.json configuration could not be loaded. Default values will be used.");
                    GenerateConfig(_api);
                    _HoeConfig = LoadConfig(_api);
                    if (_HoeConfig == null)
                    {
                        _api.Logger.Error("Unexpected error: Unable to generate default BetterHoeConfig.json configuration.");
                        _HoeConfig = new BetterHoeConfig(); // Utilise un constructeur par défaut
                    }
                }
                else
                {
                    _api.Logger.Notification("The BetterHoeConfig.json configuration has been loaded successfully.");
                }

                if (_HoeConfig.DamageItemStandard < 1)
                {
                    _api.Logger.Error("DamageItemStandard value is less than 1. It will be reset to 1.");
                    _HoeConfig.DamageItemStandard = 1;
                    _api.StoreModConfig(_HoeConfig, ConfigFileName);
                }
            }
        }

        private static BetterHoeConfig LoadConfig(ICoreAPI api)
        {
            return api.LoadModConfig<BetterHoeConfig>(ConfigFileName);
        }

        private static void GenerateConfig(ICoreAPI api)
        {
            var config = new BetterHoeConfig
            {
                ConfigVersion = CurrentConfigVersion,
                SelectedMaterialSet = "MinBronze",
                SetGravityOnPath = "On",
                DamageItemStandard = 1,
                DamageItemExtraPlow = 2,
                DamageItemExtraPath = 1,
                ConsumeSaturationStandard = 10,
                ConsumeSaturationExtraPlow = 5,
                ConsumeSaturationExtraPath = 2,
            };
            api.StoreModConfig(config, ConfigFileName);
        }

        public static BetterHoeConfig HoeConfig
        {
            get
            {
                if (_HoeConfig == null)
                {
                    ReadConfig(); // Note: Pas besoin de passer api ici
                }
                return _HoeConfig;
            }
        }

        internal class BetterHoeConfig
        {
            public int ConfigVersion { get; set; } = 1;
            public string Exemple_SelectedMaterialSet_AllowedForForestFloor { get; set; } = "Material selected for interaction with the forest floor. Possible values: AllMaterials, MinCopper, MinBronze, MinIron, Nothing. Default: MinBronze.";
            public string SelectedMaterialSet { get; set; }
            public string Exemple_Apply_Gravity_On_Path { get; set; } = "Apply gravity to paths. Possible values: On, Off. Default: On.";
            public string SetGravityOnPath { get; set; }
            public int DamageItemStandard { get; set; }
            public int DamageItemExtraPlow { get; set; }
            public int DamageItemExtraPath { get; set; }
            public int ConsumeSaturationStandard { get; set; }
            public int ConsumeSaturationExtraPlow { get; set; }
            public int ConsumeSaturationExtraPath { get; set; }
        }
    }
}

