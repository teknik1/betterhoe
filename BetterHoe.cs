using BetterHoe.Tools.Houe;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.GameContent;
using Vintagestory.API.Datastructures;
using Newtonsoft.Json.Linq;
using System.Linq;
using Vintagestory.API.Server;
using Vintagestory.API.Client;

namespace BetterHoe
{
    public class Core : ModSystem
    {
        ICoreAPI api;
        
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            this.api = api;
            this.api.World.Logger.Event("started 'Core BetterHoe' mod");
            ItemHoeEDITConfig.Initialize(api); // Initialise la configuration ici
            this.api.RegisterItemClass("ItemHoeEDIT", typeof(ItemHoeEDIT));
            this.api.RegisterBlockClass("BlockStairsPathBH", typeof(BlockStairsPathBH));
        }

        public override void StartClientSide(ICoreClientAPI capi)
        {
            this.api = capi;
            base.StartClientSide(capi);
            // ItemHoeEDITConfig.ReadConfig(capi); // Pas nécessaire maintenant
            capi.Logger.Notification("[CONFIG] Initializing the BetterHoeConfig.json file");
            capi.World.Logger.Event("started 'Client BetterHoe' mod");
        }

        public override void StartServerSide(ICoreServerAPI sapi)
        {
            this.api = sapi;          
            // ItemHoeEDITConfig.ReadConfig(sapi); // Pas nécessaire maintenant
            sapi.Logger.Notification("[CONFIG] Initializing the BetterHoeConfig.json file");
            sapi.World.Logger.Event("started 'Server BetterHoe' mod");
        }

        public override void AssetsFinalize(ICoreAPI api)
        {                    
            bool invalidConfigLogged = false;

            foreach (var block in api.World.Blocks)
            {
                if (block == null || block.Code == null || block.BlockBehaviors == null)
                {
                    continue;
                }

                string blockCode = block.Code.Path;
                string gravitySetting = ItemHoeEDITConfig.HoeConfig.SetGravityOnPath;

                if (gravitySetting == "On" &&
                   (blockCode.StartsWith("packeddirtpathtrans") ||
                    blockCode.StartsWith("packeddirtpathtransfull") ||
                    blockCode.StartsWith("packeddirtpathtransstairslow") ||
                    blockCode.StartsWith("packeddirtpathtransstairsup") ||
                    blockCode.StartsWith("packeddirtpathtransslab"))
                   )
                {
                    var jsonProps = new JsonObject(JObject.FromObject(new Dictionary<string, object>
                    {
                        { "fallSound", "effect/rockslide" },
                        { "fallSideways", true },
                        { "dustIntensity", 0.2 }
                    }));

                    var unstableFallingBehavior = new BlockBehaviorUnstableFalling(block);
                    unstableFallingBehavior.Initialize(jsonProps);

                    block.BlockBehaviors = block.BlockBehaviors.Append(unstableFallingBehavior).ToArray();
                }
                else if (gravitySetting == "Off")
                {
                    // Ne rien faire car la gravité est désactivée
                }
                if (gravitySetting != "On" && gravitySetting != "Off" && !invalidConfigLogged)
                {
                    api.World.Logger.Error($"The value of SetGravityOnPath '{gravitySetting}' is invalid. Use default value 'On'.");
                    invalidConfigLogged = true;
                }
            }
        }
    }
}



