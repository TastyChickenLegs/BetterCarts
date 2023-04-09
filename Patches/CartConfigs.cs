using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Configuration;
using BetterCarts;
using UnityEngine;

namespace BetterCarts.Patches
{
    internal class CartConfigsMain
    {

        public static ConfigEntry<KeyCode> cartHotKey;
        public static ConfigEntry<int> attachDistance;
        public static ConfigEntry<bool> allowOutOfPlaceAttach;
        public static ConfigEntry<bool> cartNoDamage;
        public static ConfigEntry<bool> includePuller;
        public static ConfigEntry<int> playerRange;
        public static ConfigEntry<int> maxPlayers;
        public static ConfigEntry<float> playerMassReduction;
        public static ConfigEntry<float> cartMassReduction;
        public static ConfigEntry<bool> allowPlayerHelp;

        
        

        internal static void Generate()
        {
            
         
            attachDistance = BetterCartsMain.context.config("Cart", "attachDistance", 3, 
                new ConfigDescription("Maximum distance to attach a cart from.", 
                new AcceptableValueRange<int>(1, 3), new ConfigurationManagerAttributes { DispName = "Attach Distance"}));

            allowOutOfPlaceAttach = BetterCartsMain.context.config("Cart", "allowOutOfPlaceAttach", true,
                new ConfigDescription("Allow attaching the cart even when out of place",null,
                new ConfigurationManagerAttributes { DispName = "Allow Out of Place Attach"}));

            cartHotKey = BetterCartsMain.context.config("Cart", "HotKey", 
                KeyCode.V, 
                new ConfigDescription("The hotkey to attach/detach a nearby cart", null,
                new ConfigurationManagerAttributes { DispName = "Attach Detach HotKey"}));

            cartNoDamage = BetterCartsMain.context.config("Cart", "noDamageToCarts", false,
                new ConfigDescription("No Damage to carts", null, 
                new ConfigurationManagerAttributes { DispName = "No Damage to Carts"}));

            cartMassReduction = BetterCartsMain.context.config("Cart", "CartMassReduction", 0.2f, 
                new ConfigDescription("For Single Player - fractional weight reduction for the cart", 
                new AcceptableValueRange<float>(0f, 0.75f), 
                new ConfigurationManagerAttributes { DispName = "Cart Mass Reduction"}));

            playerRange = BetterCartsMain.context.config("Multiple Players", "playerRange", 5, 
                new ConfigDescription("Maximum player distance to support the cart (metres).", 
                new AcceptableValueRange<int>(0, 5), 
                new ConfigurationManagerAttributes { DispName = "Player Range"}));

            maxPlayers = BetterCartsMain.context.config("Multiple Players", "maxPlayers", 4, 
                new ConfigDescription("Maximum number of supporting players.", 
                new AcceptableValueRange<int>(0, 4), 
                new ConfigurationManagerAttributes { DispName = "Max Players"}));

            playerMassReduction = BetterCartsMain.context.config("Multiple Players", "playerMassReduction", 0.2f,
                new ConfigDescription("Fractional weight reduction for each supporting player.",
                new AcceptableValueRange<float>(0f, 0.75f), new ConfigurationManagerAttributes { DispName = "Player Mass Reduction"}));

            includePuller = BetterCartsMain.context.config("Multiple Players", "includePuller", true,
                new ConfigDescription("Include the puller in weight reduction", null, 
                new ConfigurationManagerAttributes { DispName = "Include Puller"}));

            allowPlayerHelp = BetterCartsMain.context.config("Multiple Players", "allowPlayersToHelp", false,
                new ConfigDescription("Allow other players to help push cart", null, 
                new ConfigurationManagerAttributes { DispName = "Allow Players To Help"}));



        }
    }
}
