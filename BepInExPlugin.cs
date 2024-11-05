using HarmonyLib;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;
using BepInEx;

namespace PieceDumper
{
    [
        BepInPlugin(
            "envygeeks.PieceDumper", "Piece Dumper", "1.0.0"
        )
    ]
    public class BepInExPlugin: BaseUnityPlugin
    {
        private Harmony _harmony;
        private static ConfigEntry<bool> _pieces;
        private static ConfigEntry<bool> _fireplace;
        private static ConfigEntry<bool> _cookingStation;
        private static readonly string PluginNS = "envygeeks";
        private static readonly string PluginName = "PieceDumper";
        private static readonly string PluginFQN = $"{PluginNS}.{PluginName}";
        public static readonly ManualLogSource Log =
            BepInEx.Logging.Logger.CreateLogSource(
                PluginName
            );

        private void Awake()
        {
            _harmony = new Harmony(PluginFQN);
            _fireplace = Config.Bind("General", "fireplace", true, "");
            _cookingStation = Config.Bind("General", "cooking_station", true, "");
            _pieces = Config.Bind("General", "pieces", false, "");
            ConditionallyPatch();
        }

        private void ConditionallyPatch()
        {
            if (_pieces.Value)
            {
                Log.LogDebug("Patching Piece");
                _harmony.Patch(
                    original: AccessTools.Method(typeof(Piece), "Awake"),
                    postfix: new HarmonyMethod(
                        typeof(PieceAwakePatch),
                        "Postfix"
                    )
                );
            }

            if (_fireplace.Value)
            {
                Log.LogDebug("Patching Fireplace");
                var tFireplace = typeof(Fireplace);
                var tFireplacePatch = typeof(
                    FireplaceUpdatePatch
                );

                _harmony.Patch(
                    original: AccessTools.Method(tFireplace, "UpdateFireplace"),
                    postfix: new HarmonyMethod(
                        tFireplacePatch, "Postfix"
                    )
                );
            }

            if (_cookingStation.Value)
            {
                Log.LogDebug("Patching CookingStation");
                var tCooking = typeof(CookingStation);
                var tCookingPatch = typeof(
                    CookingStationUpdatePatch
                );

                _harmony.Patch(
                    original: AccessTools.Method(tCooking, "UpdateCooking"),
                    postfix: new HarmonyMethod(
                        tCookingPatch, "Postfix"
                    )
                );
            }
        }

        private static void LogPiece(Object obj)
        {
            var pieceType = obj.GetType().ToString();
            var pieceName = obj.name;
            Log.LogDebug(
                $"Found piece {pieceName} of type {pieceType}"
            );
        }

        [HarmonyPatch]
        public static class PieceAwakePatch
        {
            static void Postfix(Piece __instance)
            {
                if (!_pieces.Value) return;
                LogPiece(
                    __instance
                );
            }
        }

        [HarmonyPatch]
        static class FireplaceUpdatePatch
        {
            static void Postfix(Fireplace __instance, ZNetView ___m_nview)
            {
                if (_fireplace.Value) {
                    LogPiece(
                        __instance
                    );
                }
            }
        }

        [HarmonyPatch]
        static class CookingStationUpdatePatch
        {
            static void Postfix(CookingStation __instance, ZNetView ___m_nview)
            {
                if (_cookingStation.Value) {
                    LogPiece(
                        __instance
                    );
                }
            }
        }
    }
}
