using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using System.Diagnostics;


namespace BetterCarts.Patches;

    internal class CartPatches
    {
        [HarmonyPatch(typeof(Vagon), nameof(Vagon.GetHoverText))]
        private class VagonGetHoverText_Patch
        {
            private static void Postfix(Vagon __instance, ref string __result, ref ZNetView ___m_nview, ref string ___m_name)

            {
                __result = Localization.instance.Localize(___m_name + "\n[<color=yellow>"+ CartConfigsMain.cartHotKey.Value.MainKey +"</color>] Quick Attach/Detach" + "\n <color=yellow>Better Carts Mod</color>");
                //"\n[<color=yellow><b>E</b></color>] Use" +
            }
        }

        [HarmonyPatch(typeof(Vagon), "CanAttach")]
        private static class Vagon_CanAttach_Patch
        {
            private static bool Prefix(Vagon __instance, GameObject go, ref bool __result)
            {
            float attachDistanceFloat = Convert.ToSingle(CartConfigsMain.attachDistance.Value);

            if (!BetterCartsMain.modEnabled.Value || !CartConfigsMain.allowOutOfPlaceAttach.Value || __instance.transform.up.y < 0.1f || go != Player.m_localPlayer.gameObject)
                    return true;
                __result = !Player.m_localPlayer.IsTeleporting() && !Player.m_localPlayer.InDodge() 
                && Vector3.Distance(go.transform.position + __instance.m_attachOffset, 
                __instance.m_attachPoint.position) < attachDistanceFloat;
                return false;
            }
        }

  

        [HarmonyPatch(typeof(Vagon), "SetMass")]
        private static class SetMass_Patch
        {
            private static void Prefix(Vagon __instance, ZNetView ___m_nview, ref float mass)
            {
                if (!BetterCartsMain.modEnabled.Value || !___m_nview.IsOwner())
                    return;

                float before = mass;

                //check to see if want to allow other player to help push.  Otherwise single player mode allows to reduce
                if (CartConfigsMain.allowPlayerHelp.Value)
                {
                
                    float maxPlayersFloat = Convert.ToSingle(CartConfigsMain.maxPlayers.Value);
                
                    List<Player> players = new List<Player>();

                    float playerRangeFloat = Convert.ToSingle(CartConfigsMain.playerRange.Value);

                    Player.GetPlayersInRange(__instance.gameObject.transform.position, playerRangeFloat, players);
                    if (players.Count > (CartConfigsMain.includePuller.Value ? 0 : 1))
                        mass = Mathf.Max(0, mass - mass * CartConfigsMain.playerMassReduction.Value *
                            Mathf.Min(maxPlayersFloat, players.Count - (CartConfigsMain.includePuller.Value ? 0 : 1)));
                }
                else
                {
                //float cartMassReductionFloat = Convert.ToSingle(CartConfigsMain.cartMassReduction.Value);
                mass = Mathf.Max(0, mass - mass * CartConfigsMain.cartMassReduction.Value);
                }
            }

            
        }
    
    //wear and tear for carts.
    [HarmonyPatch(typeof(WearNTear), nameof(WearNTear.ApplyDamage))]
        public static class WearNTearApplyDamagePatch
        {
            private static bool Prefix(ref WearNTear __instance, ref float damage)
            {
                // Gets the name of the method calling the ApplyDamage method
                StackTrace stackTrace = new();
                string callingMethod = stackTrace.GetFrame(2).GetMethod().Name;

                if (!__instance.name.StartsWith("Cart(Clone)", StringComparison.Ordinal))
                {
                    return true;
                }
                else
                {
                    //logger.LogInfo(cartNoDamage.Value);
                    return !CartConfigsMain.cartNoDamage.Value && (!CartConfigsMain.cartNoDamage.Value ||
                                                                          stackTrace.GetFrame(15).GetMethod().Name != "UpdateWaterForce");
                }
            }
        }
    }

