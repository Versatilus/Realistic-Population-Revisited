﻿using ColossalFramework.Math;
using UnityEngine;

namespace WG_BalancedPopMod
{
    class Game_ResidentialAI
    {
        /// <summary>
        /// Calculated to the similar way the original code would have done it
        /// </summary>
        /// <param name="r"></param>
        /// <param name="width"></param>
        /// <param name="length"></param>
        /// <param name="subService"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static int CalculateHomeCount(Randomizer r, int width, int length, ItemClass.SubService subService, ItemClass.Level level)
        {
            int[][] residentialArray = { new int [] { 20, 25, 30, 35, 40 },
                                         new int [] { 60, 100, 130, 150, 160 } };

            int iLevel = (int)(level >= 0 ? level : 0);
            int density = (subService == ItemClass.SubService.ResidentialLow) ? 0 : 1;
            int num = residentialArray[density][iLevel];
            return Mathf.Max(100, width * length * num + r.Int32(100u)) / 100;
        }
    }
}