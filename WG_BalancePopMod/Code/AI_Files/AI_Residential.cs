﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using UnityEngine;
using System.IO;

namespace WG_BalancedPopMod
{
    class ResidentialBuildingAIMod : ResidentialBuildingAI
    {
        // CalculateHomeCount is only called once in construction or upgrading. Only store consumption
        private static Dictionary<ulong, consumeStruct> consumeCache = new Dictionary<ulong, consumeStruct>(DataStore.CACHE_SIZE);

        public static void clearCache()
        {
            consumeCache.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        /// <param name="width"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public override int CalculateHomeCount(Randomizer r, int width, int length)
        {
            BuildingInfo item = this.m_info;
            consumeCache.Remove(r.seed);  // Clean out the consumption cache on upgrade
            int level = (int)(item.m_class.m_level >= 0 ? item.m_class.m_level : 0); // Force it to 0 if the level was set to None

            int[] array = DataStore.residentialLow[level];
            if (item.m_class.m_subService == ItemClass.SubService.ResidentialHigh)
            {
                array = DataStore.residentialHigh[level];
            }

            // Check x and z just incase they are 0. A few user created assets are. If they are, then base the calculation of 3/4 of the width and length given
            Vector3 v = item.m_size;
            int x = (int)v.x;
            int z = (int)v.z;

            if (x <= 0)
            {
                x = width * 6;
            }
            if (z <= 0) 
            {
                z = length * 6;
            }

            int returnValue = Mathf.Max(1, ((x * z * Mathf.CeilToInt(v.y / array[DataStore.LEVEL_HEIGHT])) / array[DataStore.PEOPLE]));
/*
 * TODO - Maybe push to ensure units, etc
 * 
            // TODO - If the new value is greater than the previous, disconnect, everyone moves out
            //item.m_buildingAI.
            //TransferManager.TransferReason.LeaveCity0;
            // Grab anything after what we don't want, release the units
            //Singleton<CitizenManager>.instance.ReleaseUnits(data.m_citizenUnits);  
            //ResidentAI.tryMoveFamily(uint citizenID, ref Citizen data, int familySize);

            // Or just make the citizens disappear
            CitizenManager instance = Singleton<CitizenManager>.instance;
            uint num = data.m_citizenUnits; // This is from the building
            int num2 = 0;
            while (num != 0u)
            {
                for (int i = 0; i < 5; i++)
                {
                    uint citizen = instance.m_units.m_buffer[(int)((UIntPtr)num)].GetCitizen(i);
                    if (citizen != 0u)
                    {
                        ushort instance2 = instance.m_citizens.m_buffer[(int)((UIntPtr)citizen)].m_instance;
                        instance.m_citizens.m_buffer[(int)((UIntPtr)citizen)].Arrested = false;

                        if (instance2 != 0)
                        {
                            instance.ReleaseCitizenInstance(instance2);
                        }
                        instance.m_citizens.m_buffer[(int)((UIntPtr)citizen)].SetVisitplace(citizen, 0, 0u);
                    }
                }
                num = instance.m_units.m_buffer[(int)((UIntPtr)num)].m_nextUnit;
            }
*/

            return returnValue;
        }


        public override void ReleaseBuilding(ushort buildingID, ref Building data)
        {
            consumeCache.Remove(new Randomizer((int)buildingID).seed);
            base.ReleaseBuilding(buildingID, ref data);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        /// <param name="productionRate"></param>
        /// <param name="electricityConsumption"></param>
        /// <param name="waterConsumption"></param>
        /// <param name="sewageAccumulation"></param>
        /// <param name="garbageAccumulation"></param>
        /// <param name="incomeAccumulation"></param>
        public override void GetConsumptionRates(Randomizer r, int productionRate, out int electricityConsumption, out int waterConsumption, out int sewageAccumulation, out int garbageAccumulation, out int incomeAccumulation)
        {
            ulong seed = r.seed;
            ItemClass item = this.m_info.m_class;
            consumeStruct output;
            bool needRefresh = true;

            if (consumeCache.TryGetValue(seed, out output))
            {
                needRefresh = output.productionRate != productionRate;
            }

            if (needRefresh)
            {
                consumeCache.Remove(seed);
                int level = (int)(item.m_level >= 0 ? item.m_level : 0); // Force it to 0 if the level was set to None
                int[] array = (item.m_subService == ItemClass.SubService.ResidentialHigh) ? DataStore.residentialHigh[level] : DataStore.residentialLow[level];
                electricityConsumption = array[DataStore.POWER];
                waterConsumption = array[DataStore.WATER];
                sewageAccumulation = array[DataStore.SEWAGE];
                garbageAccumulation = array[DataStore.GARBAGE];
                incomeAccumulation = array[DataStore.INCOME];

                if (electricityConsumption != 0)
                {
                    electricityConsumption = Mathf.Max(100, productionRate * electricityConsumption + r.Int32(100u)) / 100;
                }
                if (waterConsumption != 0)
                {
                    int num = r.Int32(100u);
                    waterConsumption = Mathf.Max(100, productionRate * waterConsumption + num) / 100;
                    if (sewageAccumulation != 0)
                    {
                        sewageAccumulation = Mathf.Max(100, productionRate * sewageAccumulation + num) / 100;
                    }
                }
                else if (sewageAccumulation != 0)
                {
                    sewageAccumulation = Mathf.Max(100, productionRate * sewageAccumulation + r.Int32(100u)) / 100;
                }
                if (garbageAccumulation != 0)
                {
                    garbageAccumulation = Mathf.Max(100, productionRate * garbageAccumulation + r.Int32(100u)) / 100;
                }
                if (incomeAccumulation != 0)
                {
                    incomeAccumulation = productionRate * incomeAccumulation;
                }
                output.productionRate = productionRate;
                output.electricity = electricityConsumption;
                output.water = waterConsumption;
                output.sewage = sewageAccumulation;
                output.garbage = garbageAccumulation;
                output.income = incomeAccumulation;

                consumeCache.Add(seed, output);
            }
            else
            {
                productionRate = output.productionRate;
                electricityConsumption = output.electricity;
                waterConsumption = output.water;
                sewageAccumulation = output.sewage;
                garbageAccumulation = output.garbage;
                incomeAccumulation = output.income;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="productionRate"></param>
        /// <param name="cityPlanningPolicies"></param>
        /// <param name="groundPollution"></param>
        /// <param name="noisePollution"></param>
        public override void GetPollutionRates(int productionRate, DistrictPolicies.CityPlanning cityPlanningPolicies, out int groundPollution, out int noisePollution)
        {
            ItemClass @class = this.m_info.m_class;
            int level = (int)(@class.m_level >= 0 ? @class.m_level : 0); // Force it to 0 if the level was set to None
            int[] array = (@class.m_subService == ItemClass.SubService.ResidentialHigh) ? DataStore.residentialHigh[level] : DataStore.residentialLow[level];

            groundPollution = array[DataStore.GROUND_POLLUTION];
            noisePollution = array[DataStore.NOISE_POLLUTION];
        }
    }
}