using BetterHoe.Tools.Houe;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace Vintagestory.GameContent
{
    public class ItemHoeEDIT : Item
    {
        SkillItem[] modes;

        public override void OnLoaded(ICoreAPI api)
        {
            modes = new SkillItem[]
            {
                new SkillItem()
                {
                    Code = new AssetLocation(Lang.Get("betterhoe:labourer")),
                    Name = Lang.Get("betterhoe:labourer")
                },
                new SkillItem()
                {
                    Code = new AssetLocation(Lang.Get("betterhoe:chemin")),
                    Name = Lang.Get("betterhoe:chemin")
                },
            };
            if (api is ICoreClientAPI capi)
            {
                modes[0].WithIcon(capi, capi.Gui.LoadSvgWithPadding(new AssetLocation("textures/icons/labourer.svg"), 48, 48, 5, ColorUtil.WhiteArgb));
                modes[0].TexturePremultipliedAlpha = false;
                modes[1].WithIcon(capi, capi.Gui.LoadSvgWithPadding(new AssetLocation("textures/icons/chemin.svg"), 48, 48, 5, ColorUtil.WhiteArgb));
                modes[1].TexturePremultipliedAlpha = false;
            }
        }

        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
        {
            return base.GetHeldInteractionHelp(inSlot).Append(new WorldInteraction()
            {
                ActionLangCode = "heldhelp-settoolmode",
                HotKeyCode = "toolmodeselect"
            });
        }
        private BlockPos initialBlockSelPosition;

        public static class MaterialSets
        {
            public static readonly HashSet<string> AllMaterials = new() { "chert", "granite", "andesite", "basalt", "obsidian", "peridotite", "flint", "copper", "tinbronze", "bismuthbronze", "blackbronze", "gold", "silver", "iron", "meteoriciron", "steel" };
            public static readonly HashSet<string> MinCopper = new() { "copper", "tinbronze", "bismuthbronze", "blackbronze", "gold", "silver", "iron", "meteoriciron", "steel" };
            public static readonly HashSet<string> MinBronze = new() { "tinbronze", "bismuthbronze", "blackbronze", "gold", "silver", "iron", "meteoriciron", "steel" };
            public static readonly HashSet<string> MinIron = new() { "gold", "silver", "iron", "meteoriciron", "steel" };
            public static readonly HashSet<string> Nothing = new() { "nothing" };
        }

        public static HashSet<string> GetMaterialSet(string selectedSet)
        {
            return selectedSet switch
            {
                "AllMaterials" => MaterialSets.AllMaterials,
                "MinCopper" => MaterialSets.MinCopper,
                "MinBronze" => MaterialSets.MinBronze,
                "MinIron" => MaterialSets.MinIron,
                "Nothing" => MaterialSets.Nothing,
                _ => new HashSet<string>()
            };
        }

        public override void OnHeldInteractStart(ItemSlot itemslot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling)
        {
            ICoreClientAPI capi = api as ICoreClientAPI;
            if (blockSel == null) return;
            if (byEntity.Controls.ShiftKey && byEntity.Controls.CtrlKey)
            {
                base.OnHeldInteractStart(itemslot, byEntity, blockSel, entitySel, firstEvent, ref handHandling);
                return;
            }
            BlockPos pos = blockSel.Position;
            Block block = byEntity.World.BlockAccessor.GetBlock(pos);
            Block aboveBlock = byEntity.World.BlockAccessor.GetBlock(pos.UpCopy());
            byEntity.Attributes.SetInt("didtill", 0);
            int toolMode = itemslot.Itemstack.Attributes.GetInt("toolMode", 0);
            if (toolMode == 0 && (block.Code.Path.StartsWith("soil") || block.Code.Path.Contains("tallgrass")) && aboveBlock.Code.Path == "air")
            {
                initialBlockSelPosition = blockSel.Position;
                handHandling = EnumHandHandling.PreventDefault;
            }
            else if (toolMode == 0)
            {
                capi?.TriggerIngameError(this, "", Lang.Get("betterhoe:ImpossibleDeLabourer"));
            }
            HashSet<string> allowedMaterials = GetMaterialSet(ItemHoeEDITConfig.HoeConfig.SelectedMaterialSet);
            string material = itemslot.Itemstack.Collectible.FirstCodePart(1);
            bool isMaterialAllowed = allowedMaterials.Contains(material);
            if (toolMode == 1)
            {
                if (ItemHoeEDITConfig.HoeConfig.SelectedMaterialSet.Equals("Nothing", StringComparison.OrdinalIgnoreCase))
                {
                    isMaterialAllowed = false;
                }
                if (block.Code.Path.StartsWith("forestfloor") && aboveBlock.Code.Path == "air" && isMaterialAllowed)
                {
                    initialBlockSelPosition = blockSel.Position;
                    handHandling = EnumHandHandling.PreventDefault;
                }
                else if (toolMode == 1 && block.Code.Path.StartsWith("forestfloor"))
                {
                    capi?.TriggerIngameError(this, "", Lang.Get("betterhoe:IlFautUneHoueDeMilleureQualite"));
                }
                if (    
                        (block.Code.Path.StartsWith("looseores") || block.Code.Path.StartsWith("egg") || block.Code.Path.StartsWith("loosestick")
                        || block.Code.Path.StartsWith("tallfern") || block.Code.Path.StartsWith("tallplant") || block.Code.Path.StartsWith("sapling")
                        || block.Code.Path.StartsWith("mushroom") || block.Code.Path.StartsWith("fern") || block.Code.Path.StartsWith("tallgrass")
                        || block.Code.Path.StartsWith("flower") || block.Code.Path.StartsWith("snowlayer") || block.Code.Path.StartsWith("looseboulders")
                        || block.Code.Path.StartsWith("loosestones") || block.Code.Path.StartsWith("soil") || block.Code.Path.StartsWith("packeddirtpathtransslab")
                        || block.Code.Path.StartsWith("packeddirtpathtransstairs") || block.Code.Path.StartsWith("packeddirtpathtransstairslow")
                        || block.Code.Path.StartsWith("packeddirtpathtransfull") || block.Code.Path.StartsWith("packeddirtpathtrans")
                        || block.Code.Path.StartsWith("looseflints"))
                        &&
                        (aboveBlock.Code.Path == "air" || aboveBlock.Code.Path.StartsWith("wildvine") || aboveBlock.Code.Path.StartsWith("cokeovendoor")
                        || aboveBlock.Code.Path.StartsWith("painting") || aboveBlock.Code.Path.StartsWith("woodenfencegate") || aboveBlock.Code.Path.StartsWith("trapdoor")
                        || aboveBlock.Code.Path.StartsWith("door") || aboveBlock.Code.Path.StartsWith("ladder") || aboveBlock.Code.Path.StartsWith("toolrack")
                        || aboveBlock.Code.Path.StartsWith("torchholder") || aboveBlock.Code.Path.Contains("banner") || aboveBlock.Code.Path.StartsWith("sign")
                        || aboveBlock.Code.Path.StartsWith("plaque"))
                    )
                {
                    initialBlockSelPosition = blockSel.Position;
                    handHandling = EnumHandHandling.PreventDefault;
                }
                else if (toolMode == 1 && !block.Code.Path.StartsWith("forestfloor") && block.Code.Path.StartsWith("soil"))
                {
                    capi?.TriggerIngameError(this, "", Lang.Get("betterhoe:ImpossibleDeFaireUnChemin"));
                }
                else if (toolMode == 1 && !block.Code.Path.StartsWith("soil") && !block.Code.Path.StartsWith("forestfloor"))
                {
                    capi?.TriggerIngameError(this, "", Lang.Get("betterhoe:ImpossibleDeFaireUnCheminSurCeTypeDeTerrain"));
                }
            }
        }

        public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            if (blockSel == null) return false;
            if (byEntity.Controls.ShiftKey && byEntity.Controls.CtrlKey) return false;
            if (blockSel.Position != initialBlockSelPosition) return false;
            IPlayer byPlayer = (byEntity as EntityPlayer).Player;
            if (byEntity.World is IClientWorldAccessor)
            {
                ModelTransform tf = new();
                tf.EnsureDefaultValues();
                float rotateToTill = GameMath.Clamp(secondsUsed * 18, 0, 2f);
                float scrape = GameMath.SmoothStep(1 / 0.4f * GameMath.Clamp(secondsUsed - 0.35f, 0, 1));
                float scrapeShake = secondsUsed > 0.35f && secondsUsed < 0.75f ? (float)(GameMath.Sin(secondsUsed * 50) / 60f) : 0;
                float rotateWithReset = Math.Max(0, rotateToTill - GameMath.Clamp(24 * (secondsUsed - 0.75f), 0, 2));
                float scrapeWithReset = Math.Max(0, scrape - Math.Max(0, 20 * (secondsUsed - 0.75f)));
                tf.Origin.Set(0f, 0, 0.5f);
                tf.Rotation.Set(0, rotateWithReset * 45, 0);
                tf.Translation.Set(scrapeShake, 0, scrapeWithReset / 2);
                byEntity.Controls.UsingHeldItemTransformBefore = tf;
            }
            if (secondsUsed > 0.35f && secondsUsed < 0.87f)
            {
                Vec3d dir = new Vec3d().AheadCopy(1, 0, byEntity.SidedPos.Yaw - GameMath.PI);
                Vec3d pos = blockSel.Position.ToVec3d().Add(0.5 + dir.X, 1.03, 0.5 + dir.Z);
                pos.X -= dir.X * secondsUsed * 1 / 0.75f * 1.2f;
                pos.Z -= dir.Z * secondsUsed * 1 / 0.75f * 1.2f;
                byEntity.World.SpawnCubeParticles(blockSel.Position, pos, 0.25f, 3, 0.5f, byPlayer);
            }
            if (secondsUsed > 0.6f && byEntity.Attributes.GetInt("didtill") == 0 && byEntity.World.Side == EnumAppSide.Server)
            {
                byEntity.Attributes.SetInt("didtill", 1);
                DoTill(secondsUsed, slot, byEntity, blockSel, entitySel);
            }
            return secondsUsed < 1;
        }

        public override bool OnHeldInteractCancel(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, EnumItemUseCancelReason cancelReason)
        {
            initialBlockSelPosition = null;
            return false;
        }

        private static string GetStairDirection(double playerYaw)
        {
            double yawDegrees = (playerYaw * (180 / Math.PI) + 360) % 360;
            if (yawDegrees >= 45 && yawDegrees < 135) return "north";
            if (yawDegrees >= 135 && yawDegrees < 225) return "west";
            if (yawDegrees >= 225 && yawDegrees < 315) return "south";
            if ((yawDegrees >= 315 && yawDegrees < 360) || (yawDegrees >= 0 && yawDegrees < 45)) return "east";
            return "north";
        }

        private static void PlaceBlockAndPlaySound(Entity byEntity, Block blockToPlace, BlockSounds sound, BlockPos pos)
        {
            if (sound != null)
            {
                byEntity.World.PlaySoundAt(sound.Place, pos.X, pos.Y, pos.Z, null);
            }
            // Transforme le block
            byEntity.World.BlockAccessor.SetBlock(blockToPlace.BlockId, pos);
            // Pour actualiser quand on fait des chemin dans l'eau
            byEntity.World.BlockAccessor.TriggerNeighbourBlockUpdate(pos);
            byEntity.World.BlockAccessor.MarkBlockDirty(pos);
        }

        private static void DamageTool(Entity byEntity, ItemSlot slot, int damage)
        {
            slot.Itemstack.Collectible.DamageItem(byEntity.World, byEntity, slot, damage);
            if (slot.Empty)
            {
                byEntity.World.PlaySoundAt(new AssetLocation("sounds/effect/toolbreak"), byEntity.Pos.X, byEntity.Pos.Y, byEntity.Pos.Z);
            }
        }

        public static Block GetNextTransformationBlock(Entity entity, string codePath)
        {
            if (codePath.StartsWith("forestfloor") || codePath.StartsWith("soil"))
            {
                return entity.World.GetBlock(new AssetLocation("packeddirtpathtrans-free"));
            }
            else if (codePath.StartsWith("packeddirtpathtrans") && !codePath.Contains("full") && !codePath.Contains("stairslow") && !codePath.Contains("stairsup") && !codePath.Contains("slab"))
            {
                return entity.World.GetBlock(new AssetLocation("packeddirtpathtransfull-free"));
            }
            else if (codePath.StartsWith("packeddirtpathtransfull"))
            {
                return entity.World.GetBlock(new AssetLocation($"packeddirtpathtransstairslow-up-{GetStairDirection(entity.SidedPos.Yaw)}-free"));
            }
            else if (codePath.StartsWith("packeddirtpathtransstairslow"))
            {
                return entity.World.GetBlock(new AssetLocation($"packeddirtpathtransstairsup-up-{GetStairDirection(entity.SidedPos.Yaw)}-free"));
            }
            else if (codePath.StartsWith("packeddirtpathtransstairsup"))
            {
                return entity.World.GetBlock(new AssetLocation("packeddirtpathtransslab-free"));
            }
            else if (codePath.StartsWith("packeddirtpathtransslab"))
            {
                return entity.World.GetBlock(new AssetLocation("packeddirtpathtrans-free"));
            }
            return null;
        }

        public virtual void DoTill(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            if (blockSel == null) return;
            BlockPos pos = blockSel.Position;
            Block block = byEntity.World.BlockAccessor.GetBlock(pos);
            BlockPos Pos = pos;
            IPlayer byPlayer = (byEntity as EntityPlayer).Player;
            if (
                block.Code.Path.StartsWith("tallgrass") || block.Code.Path.StartsWith("looseores") || block.Code.Path.StartsWith("looseflints")
             || block.Code.Path.StartsWith("egg") || block.Code.Path.StartsWith("loosestick") || block.Code.Path.StartsWith("tallfern")
             || block.Code.Path.StartsWith("tallplant") || block.Code.Path.StartsWith("sapling") || block.Code.Path.StartsWith("mushroom")
             || block.Code.Path.StartsWith("fern") || block.Code.Path.StartsWith("flower") || block.Code.Path.StartsWith("snowlayer")
             || block.Code.Path.StartsWith("looseboulders") || block.Code.Path.StartsWith("loosestones")
             && block.Sounds != null)
            {
                byEntity.World.PlaySoundAt(block.Sounds.Place, pos.X, pos.Y, pos.Z, null);
            }
            slot.Itemstack.Collectible.DamageItem(byEntity.World, byEntity, byPlayer.InventoryManager.ActiveHotbarSlot, ItemHoeEDITConfig.HoeConfig.DamageItemStandard);
            if (block.Code.Path.StartsWith("loosestick"))
            {
                api.World.PlaySoundAt(new AssetLocation("sounds/block/loosestick"), Pos.X, Pos.Y, Pos.Z, null, false, 16);
            }
            if (block.Code.Path.StartsWith("looseores") || block.Code.Path.StartsWith("looseflints") || block.Code.Path.StartsWith("loosestones") || block.Code.Path.StartsWith("looseboulders") || block.Code.Path.StartsWith("egg"))
            {
                api.World.PlaySoundAt(new AssetLocation("sounds/block/loosestone4"), Pos.X, Pos.Y, Pos.Z, null, false, 16);
            }
            byEntity.World.BlockAccessor.SetBlock(0, Pos);
            int toolMode = slot.Itemstack.Attributes.GetInt("toolMode", 0);
            if (toolMode == 0 && block.Code.Path.StartsWith("soil"))
            {
                string fertility = block.LastCodePart(1);
                Block farmland = byEntity.World.GetBlock(new AssetLocation("farmland-dry-" + fertility));
                if (farmland == null) return;
                if (block.Sounds != null)
                {
                    byEntity.World.PlaySoundAt(block.Sounds.Place, pos.X, pos.Y, pos.Z, null);
                }
                byEntity.World.BlockAccessor.SetBlock(farmland.BlockId, pos);
                DamageTool(byEntity, byPlayer.InventoryManager.ActiveHotbarSlot, ItemHoeEDITConfig.HoeConfig.DamageItemExtraPlow);
                BlockEntity be = byEntity.World.BlockAccessor.GetBlockEntity(pos);
                if (be is BlockEntityFarmland farmland1)
                {
                    farmland1.OnCreatedFromSoil(block);
                }
                byEntity.GetBehavior<EntityBehaviorHunger>()?.ConsumeSaturation(ItemHoeEDITConfig.HoeConfig.ConsumeSaturationExtraPlow);
                byEntity.World.BlockAccessor.MarkBlockDirty(pos);
            }
            if (toolMode == 1)
            {
                Block blockToPlace = GetNextTransformationBlock(byEntity, block.Code.Path);

                if (blockToPlace != null)
                {
                    PlaceBlockAndPlaySound(byEntity, blockToPlace, block.Sounds, pos);
                    DamageTool(byEntity, byPlayer.InventoryManager.ActiveHotbarSlot, ItemHoeEDITConfig.HoeConfig.DamageItemExtraPath);
                    byEntity.GetBehavior<EntityBehaviorHunger>()?.ConsumeSaturation(ItemHoeEDITConfig.HoeConfig.ConsumeSaturationExtraPath);
                    byEntity.World.BlockAccessor.MarkBlockDirty(pos);
                }
            }
            byEntity.GetBehavior<EntityBehaviorHunger>()?.ConsumeSaturation(ItemHoeEDITConfig.HoeConfig.ConsumeSaturationStandard);
        }

        public override SkillItem[] GetToolModes(ItemSlot slot, IClientPlayer forPlayer, BlockSelection blockSel)
        {
            return modes;
        }

        public override void SetToolMode(ItemSlot slot, IPlayer byPlayer, BlockSelection blockSelection, int toolMode)
        {
            slot.Itemstack.Attributes.SetInt("toolMode", toolMode);
        }

        public override int GetToolMode(ItemSlot slot, IPlayer byPlayer, BlockSelection blockSelection)
        {
            return slot.Itemstack.Attributes.GetInt("toolMode", 0);
        }

        public override void OnUnloaded(ICoreAPI api)
        {
            for (int i = 0; modes != null && i < modes.Length; i++)
            {
                modes[i]?.Dispose();
            }
        }
    }
}
