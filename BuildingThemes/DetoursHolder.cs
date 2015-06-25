using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Reflection;
using System.Threading;
using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;
using ColossalFramework.UI;

namespace BuildingThemes
{

    class DetoursHolder
    {

        private static Dictionary<ulong, ushort> seedTable = new Dictionary<ulong, ushort>();
        public static IFilteringStrategy FilteringStrategy;

        public static void InitTable()
        {
            seedTable.Clear();
            for (ushort _seed = 0; _seed <= 65534; ++_seed)
            {
                var seed = (ulong)(6364136223846793005L * (long)_seed + 1442695040888963407L);
                seedTable.Add(seed, _seed);
            }
        }

        //we'll use this variable to pass position to GetRandomBuildingInfo method. Or we can just pass District
        public static Vector3 position;

        public static RedirectCallsState getRandomBuildingInfoState;
        public static MethodInfo getRandomBuildingInfo;

        public static RedirectCallsState zoneBlockSimulationStepState;
        public static MethodInfo zoneBlockSimulationStep;
        public static IntPtr zoneBlockSimulationStepPtr;
        public static IntPtr zoneBlockSimulationStepDetourPtr;
        
        private static MethodInfo refreshAreaBuidlings;
        private static MethodInfo getAreaIndex;

        public static RedirectCallsState resourceManagerAddResourceState;
        public static MethodInfo resourceManagerAddResource;
        public static IntPtr resourceManagerAddResourcePtr;
        public static IntPtr resourceManagerAddResourceDetourPtr;

        //this is detoured version of BuildingManger#GetRandomBuildingInfo method. Note, that it's an instance method. It's better because this way all registers will be expected to have the same values
        //as in original methods
        public BuildingInfo GetRandomBuildingInfo(ref Randomizer r, ItemClass.Service service, ItemClass.SubService subService, ItemClass.Level level, int width, int length, BuildingInfo.ZoningMode zoningMode)
        {

            var isRandomizerSimulatorManagers = r.seed == Singleton<SimulationManager>.instance.m_randomizer.seed; //I do it here in case if randomizer methodos mell be called later
            var randimizerSeed = r.seed; //if they are called seed will change
            if (BuildingThemesMod.isDebug)
            {
                UnityEngine.Debug.LogFormat("Building Themes: Detoured GetRandomBuildingInfo was called. seed: {0} (singleton seed: {1}). service: {2}, subService: {3}," +
                    "level: {4}, width: {5}, length: {6}, zoningMode: {7}, current thread: {8}\nStack trace: {9}", r.seed, Singleton<SimulationManager>.instance.m_randomizer.seed,
                    service, subService, level, width, length, zoningMode,
                                            Thread.CurrentThread.ManagedThreadId, System.Environment.StackTrace);
            }
            //this part is the same as in original method
            var buildingManager = Singleton<BuildingManager>.instance;
            if (refreshAreaBuidlings == null)
            {
                refreshAreaBuidlings = typeof(BuildingManager).GetMethod("RefreshAreaBuildings", BindingFlags.NonPublic | BindingFlags.Instance);
            }
            refreshAreaBuidlings.Invoke(buildingManager, new object[] { });
            var areaBuildings = (FastList<ushort>[])GetInstanceField(typeof(BuildingManager), buildingManager, "m_areaBuildings");
            if (getAreaIndex == null)
            {
                getAreaIndex = typeof(BuildingManager).GetMethod("GetAreaIndex", BindingFlags.NonPublic | BindingFlags.Static);
            }
            FastList<ushort> fastList = areaBuildings[(int)getAreaIndex.Invoke(null, new object[] { service, subService, level, width, length, zoningMode })];
            if (fastList == null)
            {
                if (BuildingThemesMod.isDebug)
                {
                    UnityEngine.Debug.LogFormat("Building Themes: Fast list is null. Return null, current thread: {0}",
                        Thread.CurrentThread.ManagedThreadId);
                }
                return (BuildingInfo)null;
            }

            if (fastList.m_size == 0)
            {
                if (BuildingThemesMod.isDebug)
                {
                    UnityEngine.Debug.LogFormat(
                        "Building Themes: Fast list is empty. Return null, current thread: {0}",
                        Thread.CurrentThread.ManagedThreadId);
                }
                return (BuildingInfo)null;
            }

            if (isRandomizerSimulatorManagers)
            {
                if (BuildingThemesMod.isDebug)
                {
                    UnityEngine.Debug.LogFormat(
                        "Building Themes: Getting position from static variable. position: {0}, current thread: {1}",
                        position, Thread.CurrentThread.ManagedThreadId);
                }
                FilterList(position, ref fastList);
            }
            else
            {
                if (BuildingThemesMod.isDebug)
                {
                    UnityEngine.Debug.LogFormat(
                        "Building Themes: Getting position from seed {0}... current thread: {1}", randimizerSeed,
                        Thread.CurrentThread.ManagedThreadId);
                }
                var buildingId = seedTable[randimizerSeed];
                var building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingId];
                var buildingPosition = building.m_position;
                if (BuildingThemesMod.isDebug)
                {
                    UnityEngine.Debug.LogFormat(
                        "Building Themes: Getting position from seed {0}. building: {1}, buildingId: {2}, position: {3}, threadId: {4}",
                        randimizerSeed, building.Info.name, buildingId, buildingPosition,
                        Thread.CurrentThread.ManagedThreadId);
                }
                FilterList(buildingPosition, ref fastList);
            }
            if (fastList.m_size == 0)
            {
                if (BuildingThemesMod.isDebug)
                {
                    UnityEngine.Debug.LogFormat(
                        "Building Themes: Filtered list is empty. Return null, current thread: {0}",
                        Thread.CurrentThread.ManagedThreadId);
                }
                return (BuildingInfo)null;
            }
            int index = r.Int32((uint)fastList.m_size);
            var buildingInfo = PrefabCollection<BuildingInfo>.GetPrefab((uint)fastList.m_buffer[index]);
            return buildingInfo;
        }

        private static void FilterList(Vector3 position, ref FastList<ushort> list)
        {
            //districtIdx==0 probably means 'outside of any district'
            var districtIdx = Singleton<DistrictManager>.instance.GetDistrict(position);

            if (BuildingThemesMod.isDebug)
            {
                UnityEngine.Debug.LogFormat(
                    "Building Themes: Detoured GetRandomBuildingInfo. districtIdx: {0};current thread: {1}",
                    districtIdx, Thread.CurrentThread.ManagedThreadId);
            }

            var newList = new FastList<ushort>();
            for (var i = 0; i < list.m_size; i++)
            {
                var name = PrefabCollection<BuildingInfo>.GetPrefab(list.m_buffer[i]).name;
                if (FilteringStrategy.DoesBuildingBelongToDistrict(name, districtIdx))
                {
                    newList.Add(list.m_buffer[i]);
                }
            }
            list = newList;
        }


        private static MethodInfo _CheckBlock;
        private static MethodInfo _IsGoodPlace;

        public void ZoneBlockSimulationStep(ushort blockID)
        {
            var zoneBlock = Singleton<ZoneManager>.instance.m_blocks.m_buffer[blockID];
            if (BuildingThemesMod.isDebug)
            {
                UnityEngine.Debug.LogFormat(
                    "Building Themes: Detoured ZoneBlock.SimulationStep was called. blockID: {0}, position: {1}. current thread: {2}",
                    blockID, zoneBlock.m_position, Thread.CurrentThread.ManagedThreadId);
            }
            position = zoneBlock.m_position;

            /*
            RedirectionHelper.RevertJumpTo(zoneBlockSimulationStepPtr, zoneBlockSimulationStepState);
            zoneBlockSimulationStep.Invoke(zoneBlock, new object[] { blockID });
            RedirectionHelper.PatchJumpTo(zoneBlockSimulationStepPtr, zoneBlockSimulationStepDetourPtr);
            */

            ZoneManager instance = Singleton<ZoneManager>.instance;

            int rowCount = zoneBlock.RowCount;

            float m_angle = zoneBlock.m_angle;

            Vector2 xDirection = new Vector2(Mathf.Cos(m_angle), Mathf.Sin(m_angle)) * 8f;
            Vector2 zDirection = new Vector2(xDirection.y, -xDirection.x);
            ulong num = zoneBlock.m_valid & ~(zoneBlock.m_occupied1 | zoneBlock.m_occupied2);
            int spawnpointRow = 0;
            ItemClass.Zone zone = ItemClass.Zone.Unzoned;
            int num3 = 0;
            while (num3 < 4 && zone == ItemClass.Zone.Unzoned)
            {
                spawnpointRow = Singleton<SimulationManager>.instance.m_randomizer.Int32((uint)rowCount);
                if ((num & 1uL << (spawnpointRow << 3)) != 0uL)
                {
                    zone = zoneBlock.GetZone(0, spawnpointRow);
                }
                num3++;
            }
            DistrictManager instance2 = Singleton<DistrictManager>.instance;

            Vector3 m_position = (Vector3)zoneBlock.m_position;

            byte district = instance2.GetDistrict(m_position);
            int num4;
            switch (zone)
            {
                case ItemClass.Zone.ResidentialLow:
                    num4 = instance.m_actualResidentialDemand;
                    num4 += instance2.m_districts.m_buffer[(int)district].CalculateResidentialLowDemandOffset();
                    break;
                case ItemClass.Zone.ResidentialHigh:
                    num4 = instance.m_actualResidentialDemand;
                    num4 += instance2.m_districts.m_buffer[(int)district].CalculateResidentialHighDemandOffset();
                    break;
                case ItemClass.Zone.CommercialLow:
                    num4 = instance.m_actualCommercialDemand;
                    num4 += instance2.m_districts.m_buffer[(int)district].CalculateCommercialLowDemandOffset();
                    break;
                case ItemClass.Zone.CommercialHigh:
                    num4 = instance.m_actualCommercialDemand;
                    num4 += instance2.m_districts.m_buffer[(int)district].CalculateCommercialHighDemandOffset();
                    break;
                case ItemClass.Zone.Industrial:
                    num4 = instance.m_actualWorkplaceDemand;
                    num4 += instance2.m_districts.m_buffer[(int)district].CalculateIndustrialDemandOffset();
                    break;
                case ItemClass.Zone.Office:
                    num4 = instance.m_actualWorkplaceDemand;
                    num4 += instance2.m_districts.m_buffer[(int)district].CalculateOfficeDemandOffset();
                    break;
                default:
                    return;
            }
            Vector2 a = VectorUtils.XZ(m_position);
            Vector2 vector3 = a - 3.5f * xDirection + ((float)spawnpointRow - 3.5f) * zDirection;
            int[] tmpXBuffer = instance.m_tmpXBuffer;
            for (int i = 0; i < 13; i++)
            {
                tmpXBuffer[i] = 0;
            }

            Quad2 quad = default(Quad2);
            quad.a = a - 4f * xDirection + ((float)spawnpointRow - 10f) * zDirection;
            quad.b = a + 3f * xDirection + ((float)spawnpointRow - 10f) * zDirection;
            quad.c = a + 3f * xDirection + ((float)spawnpointRow + 2f) * zDirection;
            quad.d = a - 4f * xDirection + ((float)spawnpointRow + 2f) * zDirection;
            Vector2 vector4 = quad.Min();
            Vector2 vector5 = quad.Max();
            int num5 = Mathf.Max((int)((vector4.x - 46f) / 64f + 75f), 0);
            int num6 = Mathf.Max((int)((vector4.y - 46f) / 64f + 75f), 0);
            int num7 = Mathf.Min((int)((vector5.x + 46f) / 64f + 75f), 149);
            int num8 = Mathf.Min((int)((vector5.y + 46f) / 64f + 75f), 149);
            for (int j = num6; j <= num8; j++)
            {
                for (int k = num5; k <= num7; k++)
                {
                    ushort num9 = instance.m_zoneGrid[j * 150 + k];
                    int num10 = 0;
                    while (num9 != 0)
                    {
                        Vector3 positionVar = instance.m_blocks.m_buffer[(int)num9].m_position;
                        float num11 = Mathf.Max(Mathf.Max(vector4.x - 46f - positionVar.x, vector4.y - 46f - positionVar.z), Mathf.Max(positionVar.x - vector5.x - 46f, positionVar.z - vector5.y - 46f));
                        
                        if (num11 < 0f)
                        {
                            if (_CheckBlock == null)
                            {
                                _CheckBlock = typeof(ZoneBlock).GetMethod("CheckBlock", BindingFlags.NonPublic | BindingFlags.Instance);
                            }
                            
                            _CheckBlock.Invoke(zoneBlock, new object[] {instance.m_blocks.m_buffer[(int)num9], tmpXBuffer, zone, vector3, xDirection, zDirection, quad});
                        }
                        num9 = instance.m_blocks.m_buffer[(int)num9].m_nextGridBlock;
                        if (++num10 >= 32768)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }

            for (int l = 0; l < 13; l++)
            {
                uint num12 = (uint)tmpXBuffer[l];
                int num13 = 0;
                bool flag = (num12 & 196608u) == 196608u;
                bool flag2 = false;
                while ((num12 & 1u) != 0u)
                {
                    num13++;
                    flag2 = ((num12 & 65536u) != 0u);
                    num12 >>= 1;
                }
                if (num13 == 5 || num13 == 6)
                {
                    if (flag2)
                    {
                        num13 -= Singleton<SimulationManager>.instance.m_randomizer.Int32(2u) + 2;
                    }
                    else
                    {
                        num13 = 4;
                    }
                    num13 |= 131072;
                }
                else if (num13 == 7)
                {
                    num13 = 4;
                    num13 |= 131072;
                }
                if (flag)
                {
                    num13 |= 65536;
                }
                tmpXBuffer[l] = num13;
            }
            int num14 = tmpXBuffer[6] & 65535;
            if (num14 == 0)
            {
                return;
            }

            if (_IsGoodPlace == null)
            {
                _IsGoodPlace = typeof(ZoneBlock).GetMethod("IsGoodPlace", BindingFlags.NonPublic | BindingFlags.Instance);
            }

            bool flag3 = (bool) _IsGoodPlace.Invoke(zoneBlock, new object[] {vector3});
            if (Singleton<SimulationManager>.instance.m_randomizer.Int32(100u) >= num4)
            {
                if (flag3)
                {
                    instance.m_goodAreaFound[(int)zone] = 1024;
                }
                return;
            }
            if (!flag3 && instance.m_goodAreaFound[(int)zone] > -1024)
            {
                if (instance.m_goodAreaFound[(int)zone] == 0)
                {
                    instance.m_goodAreaFound[(int)zone] = -1;
                }
                return;
            }
            int num15 = 6;
            int num16 = 6;
            bool flag4 = true;
            while (true)
            {
                if (flag4)
                {
                    while (num15 != 0)
                    {
                        if ((tmpXBuffer[num15 - 1] & 65535) != num14)
                        {
                            break;
                        }
                        num15--;
                    }
                    while (num16 != 12)
                    {
                        if ((tmpXBuffer[num16 + 1] & 65535) != num14)
                        {
                            break;
                        }
                        num16++;
                    }
                }
                else
                {
                    while (num15 != 0)
                    {
                        if ((tmpXBuffer[num15 - 1] & 65535) < num14)
                        {
                            break;
                        }
                        num15--;
                    }
                    while (num16 != 12)
                    {
                        if ((tmpXBuffer[num16 + 1] & 65535) < num14)
                        {
                            break;
                        }
                        num16++;
                    }
                }
                int num17 = num15;
                int num18 = num16;
                while (num17 != 0)
                {
                    if ((tmpXBuffer[num17 - 1] & 65535) < 2)
                    {
                        break;
                    }
                    num17--;
                }
                while (num18 != 12)
                {
                    if ((tmpXBuffer[num18 + 1] & 65535) < 2)
                    {
                        break;
                    }
                    num18++;
                }
                bool flag5 = num17 != 0 && num17 == num15 - 1;
                bool flag6 = num18 != 12 && num18 == num16 + 1;
                if (flag5 && flag6)
                {
                    if (num16 - num15 > 2)
                    {
                        break;
                    }
                    if (num14 <= 2)
                    {
                        if (!flag4)
                        {
                            goto Block_34;
                        }
                    }
                    else
                    {
                        num14--;
                    }
                }
                else if (flag5)
                {
                    if (num16 - num15 > 1)
                    {
                        goto Block_36;
                    }
                    if (num14 <= 2)
                    {
                        if (!flag4)
                        {
                            goto Block_38;
                        }
                    }
                    else
                    {
                        num14--;
                    }
                }
                else if (flag6)
                {
                    if (num16 - num15 > 1)
                    {
                        goto Block_40;
                    }
                    if (num14 <= 2)
                    {
                        if (!flag4)
                        {
                            goto Block_42;
                        }
                    }
                    else
                    {
                        num14--;
                    }
                }
                else
                {
                    if (num15 != num16)
                    {
                        goto IL_884;
                    }
                    if (num14 <= 2)
                    {
                        if (!flag4)
                        {
                            goto Block_45;
                        }
                    }
                    else
                    {
                        num14--;
                    }
                }
                flag4 = false;
            }
            num15++;
            num16--;
        Block_34:
            goto IL_891;
        Block_36:
            num15++;
        Block_38:
            goto IL_891;
        Block_40:
            num16--;
        Block_42:
        Block_45:
        IL_884:
        IL_891:
            int num19;
            int num20;
            if (num14 == 1 && num16 - num15 >= 1)
            {
                num15 += Singleton<SimulationManager>.instance.m_randomizer.Int32((uint)(num16 - num15));
                num16 = num15 + 1;
                num19 = num15 + Singleton<SimulationManager>.instance.m_randomizer.Int32(2u);
                num20 = num19;
            }
            else
            {
                do
                {
                    num19 = num15;
                    num20 = num16;
                    if (num16 - num15 == 2)
                    {
                        if (Singleton<SimulationManager>.instance.m_randomizer.Int32(2u) == 0)
                        {
                            num20--;
                        }
                        else
                        {
                            num19++;
                        }
                    }
                    else if (num16 - num15 == 3)
                    {
                        if (Singleton<SimulationManager>.instance.m_randomizer.Int32(2u) == 0)
                        {
                            num20 -= 2;
                        }
                        else
                        {
                            num19 += 2;
                        }
                    }
                    else if (num16 - num15 == 4)
                    {
                        if (Singleton<SimulationManager>.instance.m_randomizer.Int32(2u) == 0)
                        {
                            num16 -= 2;
                            num20 -= 3;
                        }
                        else
                        {
                            num15 += 2;
                            num19 += 3;
                        }
                    }
                    else if (num16 - num15 == 5)
                    {
                        if (Singleton<SimulationManager>.instance.m_randomizer.Int32(2u) == 0)
                        {
                            num16 -= 3;
                            num20 -= 2;
                        }
                        else
                        {
                            num15 += 3;
                            num19 += 2;
                        }
                    }
                    else if (num16 - num15 >= 6)
                    {
                        if (num15 == 0 || num16 == 12)
                        {
                            if (num15 == 0)
                            {
                                num15 = 3;
                                num19 = 2;
                            }
                            if (num16 == 12)
                            {
                                num16 = 9;
                                num20 = 10;
                            }
                        }
                        else if (Singleton<SimulationManager>.instance.m_randomizer.Int32(2u) == 0)
                        {
                            num16 = num15 + 3;
                            num20 = num19 + 2;
                        }
                        else
                        {
                            num15 = num16 - 3;
                            num19 = num20 - 2;
                        }
                    }
                }
                while (num16 - num15 > 3 || num20 - num19 > 3);
            }
            int depth_A = 4;
            int width_A = num16 - num15 + 1;
            BuildingInfo.ZoningMode zoningMode = BuildingInfo.ZoningMode.Straight;
            bool flag7 = true;
            for (int m = num15; m <= num16; m++)
            {
                depth_A = Mathf.Min(depth_A, tmpXBuffer[m] & 65535);
                if ((tmpXBuffer[m] & 131072) == 0)
                {
                    flag7 = false;
                }
            }
            if (num16 > num15)
            {
                if ((tmpXBuffer[num15] & 65536) != 0)
                {
                    zoningMode = BuildingInfo.ZoningMode.CornerLeft;
                    num20 = num15 + num20 - num19;
                    num19 = num15;
                }
                if ((tmpXBuffer[num16] & 65536) != 0 && (zoningMode != BuildingInfo.ZoningMode.CornerLeft || Singleton<SimulationManager>.instance.m_randomizer.Int32(2u) == 0))
                {
                    zoningMode = BuildingInfo.ZoningMode.CornerRight;
                    num19 = num16 + num19 - num20;
                    num20 = num16;
                }
            }
            int depth_B = 4;
            int width_B = num20 - num19 + 1;
            BuildingInfo.ZoningMode zoningMode2 = BuildingInfo.ZoningMode.Straight;
            bool flag8 = true;
            for (int n = num19; n <= num20; n++)
            {
                depth_B = Mathf.Min(depth_B, tmpXBuffer[n] & 65535);
                if ((tmpXBuffer[n] & 131072) == 0)
                {
                    flag8 = false;
                }
            }
            if (num20 > num19)
            {
                if ((tmpXBuffer[num19] & 65536) != 0)
                {
                    zoningMode2 = BuildingInfo.ZoningMode.CornerLeft;
                }
                if ((tmpXBuffer[num20] & 65536) != 0 && (zoningMode2 != BuildingInfo.ZoningMode.CornerLeft || Singleton<SimulationManager>.instance.m_randomizer.Int32(2u) == 0))
                {
                    zoningMode2 = BuildingInfo.ZoningMode.CornerRight;
                }
            }
            ItemClass.SubService subService = ItemClass.SubService.None;
            ItemClass.Level level = ItemClass.Level.Level1;
            ItemClass.Service service;
            switch (zone)
            {
                case ItemClass.Zone.ResidentialLow:
                    service = ItemClass.Service.Residential;
                    subService = ItemClass.SubService.ResidentialLow;
                    break;
                case ItemClass.Zone.ResidentialHigh:
                    service = ItemClass.Service.Residential;
                    subService = ItemClass.SubService.ResidentialHigh;
                    break;
                case ItemClass.Zone.CommercialLow:
                    service = ItemClass.Service.Commercial;
                    subService = ItemClass.SubService.CommercialLow;
                    break;
                case ItemClass.Zone.CommercialHigh:
                    service = ItemClass.Service.Commercial;
                    subService = ItemClass.SubService.CommercialHigh;
                    break;
                case ItemClass.Zone.Industrial:
                    service = ItemClass.Service.Industrial;
                    break;
                case ItemClass.Zone.Office:
                    service = ItemClass.Service.Office;
                    subService = ItemClass.SubService.None;
                    break;
                default:
                    return;
            }
            BuildingInfo buildingInfo = null;
            Vector3 vector6 = Vector3.zero;
            int num25_row = 0;
            int depth = 0;
            int width = 0;
            BuildingInfo.ZoningMode zoningMode3 = BuildingInfo.ZoningMode.Straight;
            int num28 = 0;

            // begin mod
            int depth_alt;
            int width_alt;

            // determine the calculated plot with the maximum depth
            depth_alt = depth_A;
            width_alt = width_A;

            if (depth_alt > 4) depth_alt = 4;


            UnityEngine.Debug.Log("spawnpointRow = " + spawnpointRow);
            UnityEngine.Debug.Log("num15 + num16 + 1 = " + (num15 + num16 + 1));
            UnityEngine.Debug.Log("num19 + num20 + 1 = " + (num19 + num20 + 1));
            // end mod

            while (num28 < 8) // while (num28 < 6)
            {
                switch (num28)
                {
                    // Corner cases
                    
                    case 0:
                        if (zoningMode != BuildingInfo.ZoningMode.Straight)
                        {
                            num25_row = num15 + num16 + 1;
                            depth = depth_A;
                            width = width_A;
                            zoningMode3 = zoningMode;
                            goto IL_D6A;
                        }
                        break;
                    case 1:
                        if (zoningMode2 != BuildingInfo.ZoningMode.Straight)
                        {
                            num25_row = num19 + num20 + 1;
                            depth = depth_B;
                            width = width_B;
                            zoningMode3 = zoningMode2;
                            goto IL_D6A;
                        }
                        break;
                    case 2:
                        if (zoningMode != BuildingInfo.ZoningMode.Straight)
                        {
                            if (depth_A >= 4)
                            {
                                num25_row = num15 + num16 + 1;
                                depth = ((!flag7) ? 2 : 3);
                                width = width_A;
                                zoningMode3 = zoningMode;
                                goto IL_D6A;
                            }
                        }
                        break;
                    case 3:
                        if (zoningMode2 != BuildingInfo.ZoningMode.Straight)
                        {
                            if (depth_B >= 4)
                            {
                                num25_row = num19 + num20 + 1;
                                depth = ((!flag8) ? 2 : 3);
                                width = width_B;
                                zoningMode3 = zoningMode2;
                                goto IL_D6A;
                            }
                        }
                        break;
                    // begin mod
                    case 4:
                        if (zoningMode != BuildingInfo.ZoningMode.Straight)
                        {
                            if (width_alt > 1)
                            {
                                width_alt--;
                            }
                            else if (depth_alt > 1)
                            {
                                depth_alt--;
                                width_alt = width_A;
                            }
                            else
                            {
                                break;
                            }

                            //TODO play with this
                            if (width_alt == width_A)
                            {
                                num25_row = num15 + num16 + 1;
                                
                            }
                            else
                            {
                                if (zoningMode == BuildingInfo.ZoningMode.CornerLeft)
                                {
                                    num25_row = num15 + num16 + 1 - (width_A - width_alt);
                                }
                                else
                                {
                                    num25_row = num15 + num16 + 1 + (width_A - width_alt);
                                }
                            }



                            depth = depth_alt;
                            width = width_alt;

                            zoningMode3 = zoningMode;

                            num28--;
                            goto IL_D6A;
                        }
                        break;
                    // end mod
                    // Straight cases
                    case 5:
                        //int width_A = num16 - num15 + 1;
                        num25_row = num15 + num16 + 1;
                        depth = depth_A;
                        width = width_A;
                        zoningMode3 = BuildingInfo.ZoningMode.Straight;
                        goto IL_D6A;
                    case 6:
                        // begin mod

                        // again for straight cases
                        depth_alt = depth_A;
                        width_alt = width_A;
                        if (depth_alt > 4) depth_alt = 4;
                        // end mod
                    
                        //int width_B = num20 - num19 + 1;
                        num25_row = num19 + num20 + 1;
                        depth = depth_B;
                        width = width_B;
                        zoningMode3 = BuildingInfo.ZoningMode.Straight;
                        goto IL_D6A;
                    // begin mod
                    case 7:
                       
                        if (width_alt > 1)
                        {
                            width_alt--;
                        }
                        else if (depth_alt > 1)
                        {
                            depth_alt--;
                            width_alt = width_A;
                        }
                        else 
                        {
                            break;
                        }

                        //TODO play with this
                        if (width_alt == width_A)
                        {
                            num25_row = num15 + num16 + 1;
                        }
                        else if (width_A % 2 != width_alt % 2)
                        {
                            num25_row = num15 + num16;
                        }
                        else
                        {
                            num25_row = num15 + num16 + 1;
                        }

                        depth = depth_alt;
                        width = width_alt;

                        zoningMode3 = BuildingInfo.ZoningMode.Straight;

                        num28--;
                        goto IL_D6A;
                    // end mod
                    default:
                        goto IL_D6A;
                }
            IL_DF0:
                num28++;
                continue;
            IL_D6A:
                vector6 = m_position + VectorUtils.X_Y(((float)depth * 0.5f - 4f) * xDirection + ((float)num25_row * 0.5f + (float)spawnpointRow - 10f) * zDirection);
                if (zone == ItemClass.Zone.Industrial)
                {
                    ZoneBlock.GetIndustryType(vector6, out subService, out level);
                }
                buildingInfo = Singleton<BuildingManager>.instance.GetRandomBuildingInfo(ref Singleton<SimulationManager>.instance.m_randomizer, service, subService, level, width, depth, zoningMode3);


                if (zoningMode3 != BuildingInfo.ZoningMode.Straight) UnityEngine.Debug.LogFormat("Searching prefab ({6}). {0}, {1}, {2}, footprint: {3} x {4}, mode: {5}", 
                
                    service, subService, level, width, depth, zoningMode3, num28);

                if (buildingInfo != null)
                {
                    // begin mod
                    if (buildingInfo.GetLength() == depth && buildingInfo.GetWidth() == width)
                    {
                        UnityEngine.Debug.Log("Success! Prefab found.");
                        break;
                    }
                    // end mod
                }
                UnityEngine.Debug.Log("Failure! No prefab found.");
                goto IL_DF0;
            }
            if (buildingInfo == null)
            {
                return;
            }
            float num29 = Singleton<TerrainManager>.instance.WaterLevel(VectorUtils.XZ(vector6));
            if (num29 > vector6.y)
            {
                return;
            }
            float num30 = m_angle + 1.57079637f;
            if (zoningMode3 == BuildingInfo.ZoningMode.CornerLeft && buildingInfo.m_zoningMode == BuildingInfo.ZoningMode.CornerRight)
            {
                num30 -= 1.57079637f;
                depth = width;
            }
            else if (zoningMode3 == BuildingInfo.ZoningMode.CornerRight && buildingInfo.m_zoningMode == BuildingInfo.ZoningMode.CornerLeft)
            {
                num30 += 1.57079637f;
                depth = width;
            }
            ushort num31;
            if (Singleton<BuildingManager>.instance.CreateBuilding(out num31, ref Singleton<SimulationManager>.instance.m_randomizer, buildingInfo, vector6, num30, depth, Singleton<SimulationManager>.instance.m_currentBuildIndex))
            {
                UnityEngine.Debug.LogFormat("Building created: {0}", buildingInfo.name);
                
                Singleton<SimulationManager>.instance.m_currentBuildIndex += 1u;
                switch (service)
                {
                    case ItemClass.Service.Residential:
                        instance.m_actualResidentialDemand = Mathf.Max(0, instance.m_actualResidentialDemand - 5);
                        break;
                    case ItemClass.Service.Commercial:
                        instance.m_actualCommercialDemand = Mathf.Max(0, instance.m_actualCommercialDemand - 5);
                        break;
                    case ItemClass.Service.Industrial:
                        instance.m_actualWorkplaceDemand = Mathf.Max(0, instance.m_actualWorkplaceDemand - 5);
                        break;
                    case ItemClass.Service.Office:
                        instance.m_actualWorkplaceDemand = Mathf.Max(0, instance.m_actualWorkplaceDemand - 5);
                        break;
                }
            }
            instance.m_goodAreaFound[(int)zone] = 1024;
        }

        public int ImmaterialResourceManagerAddResource(ImmaterialResourceManager.Resource resource, int rate, Vector3 positionArg, float radius)
        {
                if (BuildingThemesMod.isDebug)
                {
                    UnityEngine.Debug.LogFormat(
                        "Building Themes: Detoured ImmaterialResource.AddResource was called. position: {0}. current thread: {1}",
                        positionArg, Thread.CurrentThread.ManagedThreadId);
                }
                if (resource == ImmaterialResourceManager.Resource.Abandonment)
                {
                    position = positionArg;
                }
                RedirectionHelper.RevertJumpTo(resourceManagerAddResourcePtr, resourceManagerAddResourceState);
                var result = Singleton<ImmaterialResourceManager>.instance.AddResource(resource, rate, positionArg, radius);
                RedirectionHelper.PatchJumpTo(resourceManagerAddResourcePtr, resourceManagerAddResourceDetourPtr);
                return result;

        }



        

        internal static object GetInstanceField(Type type, object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            return field.GetValue(instance);
        }
    }
}
