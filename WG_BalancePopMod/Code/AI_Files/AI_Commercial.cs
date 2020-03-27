﻿using ColossalFramework.Math;
using UnityEngine;
using Boformer.Redirection;

namespace WG_BalancedPopMod
{
    [TargetType(typeof(CommercialBuildingAI))]
    public class CommercialBuildingAIMod : CommercialBuildingAI
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

            PrefabEmployStruct output;
            // If not seen prefab, calculate
            if (!DataStore.prefabWorkerVisit.TryGetValue(item.gameObject.GetHashCode(), out output))
            {
                int[] array = GetArray(this.m_info, (int) level);
                AI_Utils.CalculateprefabWorkerVisit(width, length, ref item, 4, ref array, out output);
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

            int[] array = GetArray(this.m_info, (int) level);

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
        /// <param name="groundPollution"></param>
        /// <param name="noisePollution"></param>
        [RedirectMethod(true)]
        public override void GetPollutionRates(ItemClass.Level level, int productionRate, DistrictPolicies.CityPlanning cityPlanningPolicies, out int groundPollution, out int noisePollution)
        {
            ItemClass item = this.m_info.m_class;
            int[] array = GetArray(this.m_info, (int) level);

            groundPollution = array[DataStore.GROUND_POLLUTION];
            noisePollution = (productionRate * array[DataStore.NOISE_POLLUTION]) / 100;
            if (item.m_subService == ItemClass.SubService.CommercialLeisure)
            {
              if ((cityPlanningPolicies & DistrictPolicies.CityPlanning.NoLoudNoises) != DistrictPolicies.CityPlanning.None)
              {
                  noisePollution /= 2;
              }
            }
        }


        [RedirectMethod(true)]
        public override int CalculateVisitplaceCount(ItemClass.Level level, Randomizer r, int width, int length)
        {
            PrefabEmployStruct visitors;


            // All commercial places will need visitors. CalcWorkplaces is normally called first, redirected above to include a calculation of worker visits (CalculateprefabWorkerVisit).
            // However, there is a problem with some converted assets that don't come through the "front door" (i.e. Ploppable RICO - see below).

            // Try to retrieve previously calculated value.
            if (!DataStore.prefabWorkerVisit.TryGetValue(this.m_info.gameObject.GetHashCode(), out visitors))
            {
                // If we didn't get a value, most likely it was because the prefab wasn't properly initialised.
                // This can happen with Ploppable RICO when the underlying asset class isn't 'Default' (for example, where Ploppable RICO assets are originally Parks, Plazas or Monuments).
                // When that happens, the above line returns zero, which sets the building to 'Not Operating Properly'.
                // So, if the call returns false, we force a recalculation of workplace visits to make sure.

                // If it's still zero after this, then we'll just return a "legitimate" zero.
                visitors.visitors = 0;

                int[] array = GetArray(this.m_info, (int)level);
                AI_Utils.CalculateprefabWorkerVisit(width, length, ref this.m_info, 4, ref array, out visitors);
                DataStore.prefabWorkerVisit.Add(this.m_info.gameObject.GetHashCode(), visitors);
                Debug.Log("CalculateprefabWorkerVisit redux: " + this.m_info.name);
            }

            return visitors.visitors;
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
            ItemClass @class = this.m_info.m_class;
            int[] array = GetArray(this.m_info, (int) level);
            return Mathf.Max(100, width * length * array[DataStore.PRODUCTION]) / 100;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        private int[] GetArray(BuildingInfo item, int level)
        {
            int[][] array = DataStore.commercialLow;

            try
            {
                switch (item.m_class.m_subService)
                {
                    case ItemClass.SubService.CommercialLeisure:
                        array = DataStore.commercialLeisure;
                        break;
                
                    case ItemClass.SubService.CommercialTourist:
                        array = DataStore.commercialTourist;
                        break;

                    case ItemClass.SubService.CommercialEco:
                        array = DataStore.commercialEco;
                        break;

                    case ItemClass.SubService.CommercialHigh:
                        array = DataStore.commercialHigh;
                        break;

                    case ItemClass.SubService.CommercialLow:
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