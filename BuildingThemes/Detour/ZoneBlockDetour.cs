using ColossalFramework;
using ColossalFramework.Math;
using System;
using System.Reflection;
using UnityEngine;

namespace BuildingThemes.Detour
{
    // This detour contains the modified plot calculation algorithm
    public class ZoneBlockDetour
    {
        private static bool deployed = false;

        private static RedirectCallsState _ZoneBlock_SimulationStep_state;
        private static MethodInfo _ZoneBlock_SimulationStep_original;
        private static MethodInfo _ZoneBlock_SimulationStep_detour;

        private static MethodInfo _CheckBlock;
        private static MethodInfo _IsGoodPlace;

        public static void Deploy()
        {
            if (!deployed)
            {
                _ZoneBlock_SimulationStep_original = typeof(ZoneBlock).GetMethod("SimulationStep", BindingFlags.Public | BindingFlags.Instance);
                _ZoneBlock_SimulationStep_detour = typeof(ZoneBlockDetour).GetMethod("SimulationStep", BindingFlags.Instance | BindingFlags.Public);
                _ZoneBlock_SimulationStep_state = RedirectionHelper.RedirectCalls(_ZoneBlock_SimulationStep_original, _ZoneBlock_SimulationStep_detour);

                _CheckBlock = typeof(ZoneBlock).GetMethod("CheckBlock", BindingFlags.NonPublic | BindingFlags.Instance);
                _IsGoodPlace = typeof(ZoneBlock).GetMethod("IsGoodPlace", BindingFlags.NonPublic | BindingFlags.Instance);

                deployed = true;

                Debugger.Log("Building Themes: ZoneBlock Methods detoured!");
            }
        }

        public static void Revert()
        {
            if (deployed)
            {
                RedirectionHelper.RevertRedirect(_ZoneBlock_SimulationStep_original, _ZoneBlock_SimulationStep_state);
                _ZoneBlock_SimulationStep_original = null;
                _ZoneBlock_SimulationStep_detour = null;

                _CheckBlock = null;
                _IsGoodPlace = null;

                deployed = false;

                Debugger.Log("Building Themes: ZoneBlock Methods restored!");
            }
        }

        private static int debugCount = 0;

        // Detours

        public void SimulationStep(ushort blockID)
        {
            // This is the decompiled ZoneBlock.SimulationStep() method
            // Segments which were changed are marked with "begin mod" and "end mod"

            var zoneBlock = Singleton<ZoneManager>.instance.m_blocks.m_buffer[blockID];

            if (Debugger.Enabled && debugCount < 10)
            {
                debugCount++;
                Debugger.LogFormat("Building Themes: Detoured ZoneBlock.SimulationStep was called. blockID: {0}, position: {1}.", blockID, zoneBlock.m_position);
            }

            ZoneManager zoneManager = Singleton<ZoneManager>.instance;
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
                    num4 = zoneManager.m_actualResidentialDemand;
                    num4 += instance2.m_districts.m_buffer[(int)district].CalculateResidentialLowDemandOffset();
                    break;
                case ItemClass.Zone.ResidentialHigh:
                    num4 = zoneManager.m_actualResidentialDemand;
                    num4 += instance2.m_districts.m_buffer[(int)district].CalculateResidentialHighDemandOffset();
                    break;
                case ItemClass.Zone.CommercialLow:
                    num4 = zoneManager.m_actualCommercialDemand;
                    num4 += instance2.m_districts.m_buffer[(int)district].CalculateCommercialLowDemandOffset();
                    break;
                case ItemClass.Zone.CommercialHigh:
                    num4 = zoneManager.m_actualCommercialDemand;
                    num4 += instance2.m_districts.m_buffer[(int)district].CalculateCommercialHighDemandOffset();
                    break;
                case ItemClass.Zone.Industrial:
                    num4 = zoneManager.m_actualWorkplaceDemand;
                    num4 += instance2.m_districts.m_buffer[(int)district].CalculateIndustrialDemandOffset();
                    break;
                case ItemClass.Zone.Office:
                    num4 = zoneManager.m_actualWorkplaceDemand;
                    num4 += instance2.m_districts.m_buffer[(int)district].CalculateOfficeDemandOffset();
                    break;
                default:
                    return;
            }
            Vector2 a = VectorUtils.XZ(m_position);
            Vector2 vector3 = a - 3.5f * xDirection + ((float)spawnpointRow - 3.5f) * zDirection;
            int[] tmpXBuffer = zoneManager.m_tmpXBuffer;
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
                    ushort num9 = zoneManager.m_zoneGrid[j * 150 + k];
                    int num10 = 0;
                    while (num9 != 0)
                    {
                        Vector3 positionVar = zoneManager.m_blocks.m_buffer[(int)num9].m_position;
                        float num11 = Mathf.Max(Mathf.Max(vector4.x - 46f - positionVar.x, vector4.y - 46f - positionVar.z),
                            Mathf.Max(positionVar.x - vector5.x - 46f, positionVar.z - vector5.y - 46f));

                        if (num11 < 0f)
                        {
                            _CheckBlock.Invoke(zoneBlock, new object[] { zoneManager.m_blocks.m_buffer[(int)num9], tmpXBuffer, zone, vector3, xDirection, zDirection, quad });
                        }
                        num9 = zoneManager.m_blocks.m_buffer[(int)num9].m_nextGridBlock;
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

            bool flag3 = (bool)_IsGoodPlace.Invoke(zoneBlock, new object[] { vector3 });
            if (Singleton<SimulationManager>.instance.m_randomizer.Int32(100u) >= num4)
            {
                if (flag3)
                {
                    zoneManager.m_goodAreaFound[(int)zone] = 1024;
                }
                return;
            }
            if (!flag3 && zoneManager.m_goodAreaFound[(int)zone] > -1024)
            {
                if (zoneManager.m_goodAreaFound[(int)zone] == 0)
                {
                    zoneManager.m_goodAreaFound[(int)zone] = -1;
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
            int length = 0;
            int width = 0;
            BuildingInfo.ZoningMode zoningMode3 = BuildingInfo.ZoningMode.Straight;
            int num28 = 0;

            // begin mod
            int depth_alt = Mathf.Min(depth_A, 4);
            int width_alt = width_A;
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
                            length = depth_A;
                            width = width_A;
                            zoningMode3 = zoningMode;
                            goto IL_D6A;
                        }
                        break;
                    case 1:
                        if (zoningMode2 != BuildingInfo.ZoningMode.Straight)
                        {
                            num25_row = num19 + num20 + 1;
                            length = depth_B;
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
                                length = ((!flag7) ? 2 : 3);
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
                                length = ((!flag8) ? 2 : 3);
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

                            length = depth_alt;
                            width = width_alt;

                            zoningMode3 = zoningMode;

                            num28--;
                            goto IL_D6A;
                        }
                        break;
                    // end mod
                    // Straight cases
                    case 5:
                        num25_row = num15 + num16 + 1;
                        length = depth_A;
                        width = width_A;
                        zoningMode3 = BuildingInfo.ZoningMode.Straight;
                        goto IL_D6A;
                    case 6:
                        // begin mod

                        // reset variables
                        depth_alt = Mathf.Min(depth_A, 4);
                        width_alt = width_A;

                        // end mod

                        //int width_B = num20 - num19 + 1;
                        num25_row = num19 + num20 + 1;
                        length = depth_B;
                        width = width_B;
                        zoningMode3 = BuildingInfo.ZoningMode.Straight;
                        goto IL_D6A;
                    // begin mod
                    case 7:

                        if (width_alt > 1)
                        {
                            width_alt--;
                        }
                        else
                        {
                            break;
                        }

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

                        length = depth_alt;
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
                vector6 = m_position + VectorUtils.X_Y(((float)length * 0.5f - 4f) * xDirection + ((float)num25_row * 0.5f + (float)spawnpointRow - 10f) * zDirection);
                if (zone == ItemClass.Zone.Industrial)
                {
                    ZoneBlock.GetIndustryType(vector6, out subService, out level);
                }

                // begin mod

                // Here we pass the position to the BuildingManager.getRandomBuildingInfo method
                BuildingManagerDetour.position = vector6;
                BuildingManagerDetour.upgrade = false;

                // end mod

                buildingInfo = Singleton<BuildingManager>.instance.GetRandomBuildingInfo(ref Singleton<SimulationManager>.instance.m_randomizer, service, subService, level, width, length, zoningMode3);

                if (buildingInfo != null)
                {
                    // begin mod

                    // If the depth of the found prefab is smaller than the one we were looking for, recalculate the size
                    // This is done by checking the position of every prop
                    // Plots only get shrinked when no assets are placed on the extra space

                    // This is needed for themes which only contain small buildings (e.g. 1x2) 
                    // because those buildings would occupy more space than needed!

                    if (buildingInfo.GetWidth() == width && buildingInfo.GetLength() != length)
                    {
                        // Calculate the z position of the furthest away prop
                        float biggestPropPosZ = 0;
                        foreach (var prop in buildingInfo.m_props)
                        {
                            biggestPropPosZ = Mathf.Max(biggestPropPosZ, buildingInfo.m_expandFrontYard ? prop.m_position.z : -prop.m_position.z);
                        }

                        // Check if the furthest away prop is outside of the bounds of the prefab
                        float occupiedExtraSpace = biggestPropPosZ - buildingInfo.GetLength() * 4;
                        if (occupiedExtraSpace <= 0)
                        {
                            // No? Then shrink the plot to the prefab length so no space is wasted!
                            length = buildingInfo.GetLength();
                        }
                        else
                        {
                            // Yes? Shrink the plot so all props are in the bounds
                            int newLength = buildingInfo.GetLength() + Mathf.CeilToInt(occupiedExtraSpace / 8);
                            length = Mathf.Min(length, newLength);
                        }

                        vector6 = m_position + VectorUtils.X_Y(((float)length * 0.5f - 4f) * xDirection + ((float)num25_row * 0.5f + (float)spawnpointRow - 10f) * zDirection);
                    }

                    // This block handles Corner buildings. We always shrink them
                    else if (buildingInfo.GetLength() == width && buildingInfo.GetWidth() != length)
                    {
                        length = buildingInfo.GetWidth();
                        vector6 = m_position + VectorUtils.X_Y(((float)length * 0.5f - 4f) * xDirection + ((float)num25_row * 0.5f + (float)spawnpointRow - 10f) * zDirection);
                    }

                    // end mod

                    if (Debugger.Enabled)
                    {
                        Debugger.LogFormat("Found prefab: {5} - {0}, {1}, {2}, {3} x {4}", service, subService, level, width, length, buildingInfo.name);
                    }
                    break;
                }
                if (Debugger.Enabled)
                {

                }
                goto IL_DF0;
            }
            if (buildingInfo == null)
            {
                if (Debugger.Enabled)
                {
                    Debugger.LogFormat("No prefab found: {0}, {1}, {2}, {3} x {4}", service, subService, level, width, length);
                }
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
                length = width;
            }
            else if (zoningMode3 == BuildingInfo.ZoningMode.CornerRight && buildingInfo.m_zoningMode == BuildingInfo.ZoningMode.CornerLeft)
            {
                num30 += 1.57079637f;
                length = width;
            }
            ushort num31;
            if (Singleton<BuildingManager>.instance.CreateBuilding(out num31, ref Singleton<SimulationManager>.instance.m_randomizer, buildingInfo, vector6, num30, length, Singleton<SimulationManager>.instance.m_currentBuildIndex))
            {
                Singleton<SimulationManager>.instance.m_currentBuildIndex += 1u;
                switch (service)
                {
                    case ItemClass.Service.Residential:
                        zoneManager.m_actualResidentialDemand = Mathf.Max(0, zoneManager.m_actualResidentialDemand - 5);
                        break;
                    case ItemClass.Service.Commercial:
                        zoneManager.m_actualCommercialDemand = Mathf.Max(0, zoneManager.m_actualCommercialDemand - 5);
                        break;
                    case ItemClass.Service.Industrial:
                        zoneManager.m_actualWorkplaceDemand = Mathf.Max(0, zoneManager.m_actualWorkplaceDemand - 5);
                        break;
                    case ItemClass.Service.Office:
                        zoneManager.m_actualWorkplaceDemand = Mathf.Max(0, zoneManager.m_actualWorkplaceDemand - 5);
                        break;
                }
            }
            zoneManager.m_goodAreaFound[(int)zone] = 1024;
        }
    }
}
