﻿using ColossalFramework.Math;
using UnityEngine;
using Boformer.Redirection;

namespace RealisticPopulationRevisited
{
    [TargetType(typeof(OfficeBuildingAI))]
    class OfficeBuildingAIMod : OfficeBuildingAI
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        /// <param name="width"></param>
        /// <param name="length"></param>
        /// <param name="level0"></param>
        /// <param name="level1"></param>
        /// <param name="level2"></param>
        /// <param name="level3"></param>
        [RedirectMethod(true)]
        public override void CalculateWorkplaceCount(ItemClass.Level level, Randomizer r, int width, int length, out int level0, out int level1, out int level2, out int level3)
        {
            ulong seed = r.seed;
            BuildingInfo item = this.m_info;
            int[] array = getArray(this.m_info, (int) level);

            PrefabEmployStruct output;
            // If not seen prefab, calculate
            if (!DataStore.prefabWorkerVisit.TryGetValue(item.gameObject.GetHashCode(), out output))
            {
                AI_Utils.CalculateprefabWorkerVisit(width, length, ref item, 10, ref array, out output);
                DataStore.prefabWorkerVisit.Add(item.gameObject.GetHashCode(), output);
            }

            level0 = output.level0;
            level1 = output.level1;
            level2 = output.level2;
            level3 = output.level3;
        }


        /// <summary>
        /// 
        /// </summary>
        [RedirectMethod(true)]
        public override void GetConsumptionRates(ItemClass.Level level, Randomizer r, int productionRate, out int electricityConsumption, out int waterConsumption, out int sewageAccumulation, out int garbageAccumulation, out int incomeAccumulation, out int mailAccumulation)
        {
            ItemClass item = this.m_info.m_class;
            int[] array = getArray(this.m_info, (int) level);

            electricityConsumption = array[DataStore.POWER];
            waterConsumption = array[DataStore.WATER];
            sewageAccumulation = array[DataStore.SEWAGE];
            garbageAccumulation = array[DataStore.GARBAGE];
            mailAccumulation = array[DataStore.MAIL];

            int landVal = AI_Utils.GetLandValueIncomeComponent(r.seed);
            incomeAccumulation = array[DataStore.INCOME] + landVal;

            electricityConsumption = Mathf.Max(100, productionRate * electricityConsumption) / 100;
            waterConsumption = Mathf.Max(100, productionRate * waterConsumption) / 100;
            sewageAccumulation = Mathf.Max(100, productionRate * sewageAccumulation) / 100;
            garbageAccumulation = Mathf.Max(100, productionRate * garbageAccumulation) / 100;
            incomeAccumulation = productionRate * incomeAccumulation;
            mailAccumulation = Mathf.Max(100, productionRate * mailAccumulation) / 100;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="productionRate"></param>
        /// <param name="cityPlanningPolicies"></param>
        /// <param name="groundPollution"></param>;
        /// <param name="noisePollution"></param>
        [RedirectMethod(true)]
        public override void GetPollutionRates(ItemClass.Level level, int productionRate, DistrictPolicies.CityPlanning cityPlanningPolicies, out int groundPollution, out int noisePollution)
        {
            ItemClass @class = this.m_info.m_class;
            int[] array = getArray(this.m_info, (int) level);

            groundPollution = array[DataStore.GROUND_POLLUTION];
            noisePollution = array[DataStore.NOISE_POLLUTION];
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        /// <param name="width"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        [RedirectMethod(true)]
        public override int CalculateProductionCapacity(ItemClass.Level level, Randomizer r, int width, int length)
        {
            ulong seed = r.seed;
            BuildingInfo item = this.m_info;
            PrefabEmployStruct worker;

            if (DataStore.prefabWorkerVisit.TryGetValue(item.gameObject.GetHashCode(), out worker))
            {
                // Employment is available
                int workers = worker.level0 + worker.level1 + worker.level2 + worker.level3;
                int[] array = getArray(this.m_info, (int) level);

                return Mathf.Max(1, workers / array[DataStore.PRODUCTION]);
            }
            else
            {
                // Return minimum to be safe
                return 1;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        private int[] getArray(BuildingInfo item, int level)
        {
            int[][] array = DataStore.office;

            try
            {
                switch (item.m_class.m_subService)
                {
                    case ItemClass.SubService.OfficeHightech:
                        array = DataStore.officeHighTech;
                        break;

                    case ItemClass.SubService.OfficeGeneric:
                    default:
                        break;
                }

                return array[level];
            }
            catch (System.Exception)
            {
                UnityEngine.Debug.Log("Realistic Population Revisited: " + item.gameObject.name + " attempted to be use " + item.m_class.m_subService.ToString() + " with level " + level + ". Returning as level 0.");
                return array[0];
            }
        }
    }
}