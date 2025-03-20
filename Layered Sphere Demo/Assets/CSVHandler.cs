//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.IO;
//using UnityEngine;
//using static UnityEditor.UIElements.ToolbarMenu;
//using System.Diagnostics;
//using Unity.VisualScripting;



//    public class CSVHandler
//    {
//        const int EMPTYFIELD = 57;
//        const int SKIP_PLANTS = 25;//25;
//        const int SKIP_ANIMALS = 22;//22;
//        const int SKIP_MARINEFISH = 11;//11;
//        const int SKIP_MARINEPLANTS = 8;//7;
//        const int SKIP_INSECTS = 12;//13;
//        const int SKIP_STONES = 8;
//        const int SKIP_MINERALS = 16;

//        /// <summary>
//        /// Handles turning Comma Separated Values files of specific formats into game data.
//        /// </summary>
//        public CSVHandler()
//        {
//        }

//        // haven't touched this object in years, will obviously need to rereroll a lot of it for the
//        // more modern design paradigm

//        //public void WriteCSV(List<TerrestrialResource> in_resources, string in_saveName)
//        //{
//        //    StringBuilder csv = new StringBuilder();
//        //    int pos = 0;
//        //    while (pos < in_resources.Count)
//        //    {
//        //        TerrestrialResource current = in_resources[pos];
//        //        ResourceDetails details = current.Get_details();
//        //        List<ResourceVariant> variants = current.Get_variants();
//        //        //if (current.GetType() == typeof(TerrestrialResource))
//        //        //{
//        //        //    TerrestrialResource temp = (TerrestrialResource)current;
//        //        //    variants = temp.Get_variants();
//        //        //}
//        //        //else if (current.GetType() == typeof(SoilResource))
//        //        //{
//        //        //    SoilResource temp = (SoilResource)current;
//        //        //    variants.AddRange(temp.Get_variants());
//        //        //}
//        //        //else if (current.GetType() == typeof(StoneResource))
//        //        //{
//        //        //    StoneResource temp = (StoneResource)current;
//        //        //    variants.AddRange(temp.Get_stoneVariants());
//        //        //}
//        //        //else
//        //        //    Debug.LogError("Unknown type fed to CSV " + details.id + ":" + details.label);

//        //        string header = MakeHeader(details, variants);
//        //        csv.Append(header);
//        //        int subPos = 0;
//        //        while (subPos < variants.Count)
//        //        {
//        //            ResourceVariant variant = variants[subPos];
//        //            string variantData = MakeVariant(current, variant);
//        //            csv.Append(variantData);
//        //            string cellData = MakeCell(variant.Get_cell());
//        //            csv.Append(cellData);
//        //            subPos++;
//        //        }
//        //        pos++;
//        //    }

//        //    String txtPath = Path.Combine(Application.dataPath, in_saveName + ".csv");
//        //    File.WriteAllText(txtPath, csv.ToString());
//        //}

//        private string MakeHeader(ResourceDetails in_details, List<ResourceVariant> in_variants)
//        {
//            string id = in_details.id;
//            string label = in_details.label;
//            string variantCount = in_variants.Count.ToString();
//            string toRet = id + ',' + label + ',' + variantCount + '\n';
//            return toRet;
//        }

//        //private string MakeVariant(Resource in_resource, ResourceVariant in_variant)
//        //{
//        //    StringBuilder builder = new StringBuilder();
//        //    builder = Package(builder, "Variant");
//        //    builder.Append(ExtractLocalizedDetails(in_resource, in_variant.Get_details()));
//        //    builder.Append('\n');
//        //    return builder.ToString();
//        //}

//        private string MakeCell(IterableBinaryBubbleCell in_cell)
//        {
//            List<Point2D> interior = in_cell.Get_interior();
//            StringBuilder builder = new StringBuilder();
//            int pos = 0;
//            while (pos < interior.Count)
//            {
//                builder = Package(builder, interior[pos]);
//                pos++;
//            }
//            builder.Append('\n');
//            return builder.ToString();
//        }

//        private string ExtractLocalizedDetails(Resource in_resource, ResourceDetails in_details)
//        {
//            string toRet = "";
//            if (in_resource.GetType() == typeof(Animal))
//                toRet = ExtractAnimalDetails((AnimalDetails)in_details);
//            else if (in_resource.GetType() == typeof(Plant))
//                toRet = ExtractPlantDetails((PlantDetails)in_details);
//            else if (in_resource.GetType() == typeof(Fungus))
//                toRet = ExtractFungusDetails((FungusDetails)in_details);
//            else if (in_resource.GetType() == typeof(MarinePlant))
//                toRet = ExtractMarinePlantDetails((MarinePlantDetails)in_details);
//            else if (in_resource.GetType() == typeof(MarineFish))
//                toRet = ExtractMarineFishDetails((MarineFishDetails)in_details);
//            else if (in_resource.GetType() == typeof(FreshwaterFish))
//                toRet = ExtractFreshwaterFishDetails((FreshwaterFishDetails)in_details);
//            else if (in_resource.GetType() == typeof(Insect))
//                toRet = ExtractInsectDetails((InsectDetails)in_details);
//            else if (in_resource.GetType() == typeof(Mineral))
//                toRet = ExtractMineralDetails((MineralDetails)in_details);
//            else if (in_resource.GetType() == typeof(Stone))
//                toRet = ExtractStoneDetails((StoneDetails)in_details);
//            else if (in_resource.GetType() == typeof(Soil))
//                toRet = ExtractSoilDetails((SoilDetails)in_details);
//            else
//                Debug.LogError("Unknown Terrestrial Resource type " + in_details.id + ":" + in_details.label);

//            return toRet;
//        }

//        private string ExtractAnimalDetails(AnimalDetails in_details)
//        {
//            StringBuilder builder = new StringBuilder();
//            builder = Package(builder, "Animal");
//            builder = Package(builder, in_details.rarity);
//            builder = Package(builder, in_details.diet);
//            builder = Package(builder, in_details.adultAge);
//            builder = Package(builder, in_details.maxAge);
//            builder = Package(builder, in_details.litterSize);
//            builder = Package(builder, in_details.infantMortality);
//            builder = Package(builder, in_details.adultMortality);
//            builder = Package(builder, in_details.minTemperature);
//            builder = Package(builder, in_details.maxTemperature);
//            builder = Package(builder, in_details.sheltersFromCold);
//            builder = Package(builder, in_details.sheltersFromHeat);
//            builder = Package(builder, in_details.canFly);
//            builder = Package(builder, in_details.canSwim);
//            builder = Package(builder, in_details.terrains);
//            builder = Package(builder, in_details.superiorBiomes);
//            builder = Package(builder, in_details.deficientBiomes);
//            builder = Package(builder, in_details.trophicIndex);
//            builder = Package(builder, in_details.foodTargets);

//            return builder.ToString();
//        }

//        private string ExtractPlantDetails(PlantDetails in_details)
//        {
//            StringBuilder builder = new StringBuilder();
//            builder = Package(builder, "Plant");
//            builder = Package(builder, in_details.minimumTemperature);
//            builder = Package(builder, in_details.maximumTemperature);
//            builder = Package(builder, in_details.optimalMinimumTemperature);
//            builder = Package(builder, in_details.optimalMaximumTemperature);
//            builder = Package(builder, in_details.minimumPrecipitation);
//            builder = Package(builder, in_details.maximumPrecipitation);
//            builder = Package(builder, in_details.rarity);
//            builder = Package(builder, in_details.shadeTolerance);
//            builder = Package(builder, in_details.seedingFrequency);
//            builder = Package(builder, in_details.seedingRadius);
//            builder = Package(builder, in_details.seedingAge);
//            builder = Package(builder, in_details.matureAge);
//            builder = Package(builder, in_details.maxAge);
//            builder = Package(builder, in_details.maxHeight);
//            builder = Package(builder, in_details.maxDiameter);
//            builder = Package(builder, in_details.maxVigor);
//            builder = Package(builder, in_details.maxRadius);
//            builder = Package(builder, in_details.heightStep);
//            builder = Package(builder, in_details.radiusStep);

//            return builder.ToString();
//        }

//        private string ExtractFungusDetails(FungusDetails in_details)
//        {
//            StringBuilder builder = new StringBuilder();
//            // not implemented yet

//            return builder.ToString();
//        }

//        private string ExtractMarinePlantDetails(MarinePlantDetails in_details)
//        {
//            StringBuilder builder = new StringBuilder();
//            builder = Package(builder, "Marine Plant");
//            builder = Package(builder, in_details.rarity);
//            builder = Package(builder, in_details.minTemperature);
//            builder = Package(builder, in_details.maxTemperature);
//            builder = Package(builder, in_details.oceanLayers);

//            return builder.ToString();
//        }

//        private string ExtractMarineFishDetails(MarineFishDetails in_details)
//        {
//            StringBuilder builder = new StringBuilder();
//            builder = Package(builder, "Marine Fish");
//            builder = Package(builder, in_details.rarity);
//            builder = Package(builder, in_details.minTemperature);
//            builder = Package(builder, in_details.maxTemperature);
//            builder = Package(builder, in_details.oceanLayers);
//            builder = Package(builder, in_details.diet);
//            builder = Package(builder, in_details.trophicIndex);
//            builder = Package(builder, in_details.foodTargets);

//            return builder.ToString();
//        }

//        private string ExtractFreshwaterFishDetails(FreshwaterFishDetails in_details)
//        {
//            StringBuilder builder = new StringBuilder();
//            // not implemented yet

//            return builder.ToString();
//        }

//        private string ExtractInsectDetails(InsectDetails in_details)
//        {
//            StringBuilder builder = new StringBuilder();
//            builder = Package(builder, "Insect");
//            builder = Package(builder, in_details.rarity);
//            builder = Package(builder, in_details.miniumumTemperature);
//            builder = Package(builder, in_details.maxiumumTemperature);
//            builder = Package(builder, in_details.coldSheltering);
//            builder = Package(builder, in_details.heatSheltering);
//            builder = Package(builder, in_details.biomes);
//            builder = Package(builder, in_details.trophicIndex);
//            builder = Package(builder, in_details.foodTargets);

//            return builder.ToString();
//        }

//        private string ExtractMineralDetails(MineralDetails in_details)
//        {
//            StringBuilder builder = new StringBuilder();
//            builder = Package(builder, "Mineral");
//            builder = Package(builder, in_details.shapeProfile);
//            builder = Package(builder, in_details.selfExclusive);
//            builder = Package(builder, in_details.layerDepth);
//            builder = Package(builder, in_details.mapScale);
//            builder = Package(builder, in_details.lowThreshold);
//            builder = Package(builder, in_details.highThreshold);
//            builder = Package(builder, in_details.pitSuitable);
//            builder = Package(builder, in_details.lowVolume);
//            builder = Package(builder, in_details.highVolume);
//            builder = Package(builder, in_details.lowGrade);
//            builder = Package(builder, in_details.highGrade);
//            builder = Package(builder, in_details.hostRocks);

//            return builder.ToString();
//        }

//        private string ExtractStoneDetails(StoneDetails in_details)
//        {
//            StringBuilder builder = new StringBuilder();
//            builder = Package(builder, "Stone");
//            builder = Package(builder, in_details.rockType);
//            builder = Package(builder, in_details.scale);
//            builder = Package(builder, in_details.lowThreshold);
//            builder = Package(builder, in_details.highThreshold);

//            return builder.ToString();
//        }

//        private string ExtractSoilDetails(SoilDetails in_details)
//        {
//            StringBuilder builder = new StringBuilder();
//            builder = Package(builder, "Soil");
//            builder = Package(builder, in_details.minimumTemperature);
//            builder = Package(builder, in_details.maximumTemperature);
//            builder = Package(builder, in_details.minimumPrecipitation);
//            builder = Package(builder, in_details.maximumPrecipitation);
//            builder = Package(builder, in_details.associations);

//            return builder.ToString();
//        }

//        private string PackageCharArray(char[] in_chars)
//        {
//            StringBuilder builder = new StringBuilder();
//            int pos = 0;
//            while (pos < in_chars.Length)
//            {
//                char current = in_chars[pos];
//                builder.Append(current);
//                pos++;
//            }
//            return builder.ToString();
//        }

//        private string PackageIDs(string[] in_ids)
//        {
//            StringBuilder builder = new StringBuilder();
//            int pos = 0;
//            while (pos < in_ids.Length)
//            {
//                string id = in_ids[pos];
//                string seperator = "; ";
//                builder.Append(id);
//                builder.Append(seperator);
//                pos++;
//            }
//            return builder.ToString();
//        }

//        private StringBuilder Package(StringBuilder in_builder, int ins)
//        {
//            StringBuilder toRet = in_builder;
//            string data = ins.ToString();
//            toRet.Append(data);
//            toRet.Append(',');
//            return toRet;
//        }

//        private StringBuilder Package(StringBuilder in_builder, float ins)
//        {
//            StringBuilder toRet = in_builder;
//            string data = ins.ToString();
//            toRet.Append(data);
//            toRet.Append(',');
//            return toRet;
//        }

//        private StringBuilder Package(StringBuilder in_builder, bool ins)
//        {
//            StringBuilder toRet = in_builder;
//            string data = ins.ToString();
//            toRet.Append('"');
//            toRet.Append(data);
//            toRet.Append('"');
//            toRet.Append(',');
//            return toRet;
//        }

//        private StringBuilder Package(StringBuilder in_builder, char ins)
//        {
//            StringBuilder toRet = in_builder;
//            string data = ins.ToString();
//            toRet.Append('"');
//            toRet.Append(data);
//            toRet.Append('"');
//            toRet.Append(',');
//            return toRet;
//        }

//        private StringBuilder Package(StringBuilder in_builder, char[] in_set)
//        {
//            {
//                StringBuilder toRet = in_builder;
//                string data = PackageCharArray(in_set);
//                toRet.Append('"');
//                toRet.Append(data);
//                toRet.Append('"');
//                toRet.Append(',');
//                return toRet;
//            }
//        }

//        private StringBuilder Package(StringBuilder in_builder, string ins)
//        {
//            StringBuilder toRet = in_builder;
//            string data = ins.ToString();
//            toRet.Append('"');
//            toRet.Append(data);
//            toRet.Append('"');
//            toRet.Append(',');
//            return toRet;
//        }

//        private StringBuilder Package(StringBuilder in_builder, string[] in_set)
//        {
//            StringBuilder toRet = in_builder;
//            string data = PackageIDs(in_set);
//            toRet.Append('"');
//            toRet.Append(data);
//            toRet.Append('"');
//            toRet.Append(',');
//            return toRet;
//        }

//        private StringBuilder Package(StringBuilder in_builder, Point2D in_point)
//        {
//            StringBuilder toRet = in_builder;
//            string data = in_point.GetStringForm();
//            toRet.Append('"');
//            toRet.Append(data);
//            toRet.Append('"');
//            toRet.Append(',');
//            return toRet;
//        }

//        public void WriteCSV(List<Insect> in_insects)
//        {
//            StringBuilder csv = new StringBuilder();
//            int pos = 0;
//            while (pos < in_insects.Count)
//            {
//                Insect current = in_insects[pos];
//                InsectDetails details = current.baseDetails;
//                string label = details.label;
//                string id = details.id;
//                int trophicIndex = details.trophicIndex;
//                string line = id + "," + label + "," + trophicIndex + '\n';
//                csv.Append(line);
//                pos++;
//            }

//            String txtPath = Path.Combine(Application.dataPath, "InsectTrophicIndices.csv");
//            File.WriteAllText(txtPath, csv.ToString());
//        }

//        public void WriteCSV(List<Animal> in_animals)
//        {
//            StringBuilder csv = new StringBuilder();
//            int pos = 0;
//            while (pos < in_animals.Count)
//            {
//                Animal current = in_animals[pos];
//                AnimalDetails details = current.baseDetails;
//                string label = details.label;
//                string id = details.id;
//                int trophicIndex = details.trophicIndex;
//                string line = id + "," + label + "," + trophicIndex + '\n';
//                csv.Append(line);
//                pos++;
//            }

//            String txtPath = Path.Combine(Application.dataPath, "AnimalTrophicIndices.csv");
//            File.WriteAllText(txtPath, csv.ToString());

//        }

//        public void WriteCSV(List<MarineFish> in_fish)
//        {
//            StringBuilder csv = new StringBuilder();
//            int pos = 0;
//            while (pos < in_fish.Count)
//            {
//                MarineFish current = in_fish[pos];
//                MarineFishDetails details = current.baseDetails;
//                string label = details.label;
//                string id = details.id;
//                int trophicIndex = details.trophicIndex;
//                string line = id + "," + label + "," + trophicIndex + '\n';
//                csv.Append(line);
//                pos++;
//            }

//            String txtPath = Path.Combine(Application.dataPath, "MarineFishTrophicIndices.csv");
//            File.WriteAllText(txtPath, csv.ToString());
//        }

//        public ParadigmRow[] LoadResourceDetails(ParadigmRow[] in_rows)
//        {
//            ParadigmRow[] toRet = in_rows;
//            String txtPath = Path.Combine(Application.dataPath, "ResourceDetails.csv");
//            StreamReader file = new StreamReader(txtPath);
//            string line;
//            int lineCount = 0;
//            while ((line = file.ReadLine()) != null)
//            {
//                if (lineCount != 0)
//                    toRet = ParseResourceDetail(toRet, line);
//                lineCount++;
//            }

//            return toRet;
//        }

//        private ParadigmRow[] ParseResourceDetail(ParadigmRow[] in_rows, string in_line)
//        {
//            //Debug.Log("Attempting to parse line :" + in_line);
//            ParadigmRow[] toRet = in_rows;
//            char[] chars = in_line.ToArray();
//            string id = Extract_ID(chars);
//            int pos = 5;
//            string label = Extract_String(chars, ref pos);
//            string type = Extract_String(chars, ref pos);
//            string name = Extract_String(chars, ref pos);
//            string pluralName = Extract_String(chars, ref pos);
//            char[] idArray = id.ToCharArray();
//            ResourceDetails details = new ResourceDetails(id, label, type, name, pluralName);
//            int a = idArray[0] - '0';
//            int b = idArray[1] - '0';
//            int c = idArray[2] - '0';
//            int d = idArray[3] - '0';
//            int e = idArray[4] - '0';
//            if (a < in_rows.Length)
//            {
//                ParadigmRow paradigm = toRet[a];
//                DomainRow[] domainRows = paradigm.Get_rows();
//                if (b < domainRows.Length)
//                {
//                    DomainRow domain = domainRows[b];
//                    SectorRow[] sectorRows = domain.Get_rows();
//                    if (c < sectorRows.Length)
//                    {
//                        SectorRow sector = sectorRows[c];
//                        ResourceRow[] resourceRows = sector.Get_rows();
//                        if (d < resourceRows.Length)
//                        {
//                            ResourceRow row = resourceRows[d];
//                            Resource[] resources = row.Get_resources();
//                            if (e < resources.Length)
//                            {
//                                Resource target = resources[e];
//                                if (target.Get_label() == label)
//                                {
//                                    bool parsed = false;
//                                    if (target.GetType() == typeof(Plant))
//                                    {
//                                        Plant plant = (Plant)target;
//                                        parsed = Parse_PlantDetails(ref details, pos, chars, id, label, type, name, pluralName);
//                                        PlantDetails plantDetails = (PlantDetails)details;
//                                        if (!parsed)
//                                            Debug.Log("Failed to parse plant " + id + ":" + name);
//                                        else
//                                            plant.baseDetails = plantDetails;
//                                    }
//                                    else if (target.GetType() == typeof(Animal))
//                                    {
//                                        Animal animal = (Animal)target;
//                                        parsed = Parse_AnimalDetails(ref details, pos, chars, id, label, type, name, pluralName);
//                                        AnimalDetails animalDetails = (AnimalDetails)details;
//                                        if (!parsed)
//                                            Debug.Log("Failed to parse animal " + id + ":" + name);
//                                        else
//                                            animal.baseDetails = animalDetails;
//                                    }
//                                    else if (target.GetType() == typeof(MarineFish))
//                                    {
//                                        MarineFish marineFish = (MarineFish)target;
//                                        parsed = Parse_MarineFishDetails(ref details, pos, chars, id, label, type, name, pluralName);
//                                        MarineFishDetails marineFishDetails = (MarineFishDetails)details;
//                                        if (!parsed)
//                                            Debug.Log("Failed to parse marine fish " + id + ":" + name);
//                                        else
//                                            marineFish.baseDetails = marineFishDetails;
//                                    }
//                                    else if (target.GetType() == typeof(MarinePlant))
//                                    {
//                                        MarinePlant marinePlant = (MarinePlant)target;
//                                        parsed = Parse_MarinePlantDetails(ref details, pos, chars, id, label, type, name, pluralName);
//                                        MarinePlantDetails marinePlantDetails = (MarinePlantDetails)details;
//                                        if (!parsed)
//                                            Debug.Log("Failed to parse marine plant " + id + ":" + name);
//                                        else
//                                            marinePlant.baseDetails = marinePlantDetails;
//                                    }
//                                    else if (target.GetType() == typeof(Insect))
//                                    {
//                                        Insect insect = (Insect)target;
//                                        parsed = Parse_InsectDetails(ref details, pos, chars, id, label, type, name, pluralName);
//                                        InsectDetails insectDetails = (InsectDetails)details;
//                                        if (!parsed)
//                                            Debug.Log("Failed to parse insect " + id + ":" + name);
//                                        else
//                                            insect.baseDetails = insectDetails;
//                                    }
//                                    else if (target.GetType() == typeof(Mineral))
//                                    {
//                                        Mineral mineral = (Mineral)target;
//                                        parsed = Parse_MineralDetails(ref details, pos, chars, id, label, type, name, pluralName);
//                                        MineralDetails mineralDetails = (MineralDetails)details;
//                                        if (!parsed)
//                                            Debug.Log("Failed to parse mineral " + id + ":" + name);
//                                        else
//                                            mineral.baseDetails = mineralDetails;
//                                    }
//                                    else if (target.GetType() == typeof(Stone))
//                                    {
//                                        Stone stone = (Stone)target;
//                                        parsed = Parse_StoneDetails(ref details, pos, chars, id, label, type, name, pluralName);
//                                        StoneDetails stoneDetails = (StoneDetails)details;
//                                        if (!parsed)
//                                            Debug.Log("Failed to parse stone " + id + ":" + name);
//                                        else
//                                            stone.baseDetails = stoneDetails;
//                                    }
//                                    else if (target.GetType() == typeof(Soil))
//                                    {
//                                        Soil soil = (Soil)target;
//                                        parsed = Parse_SoilDetails(ref details, pos, chars, id, label, type, name, pluralName);
//                                        SoilDetails soilDetails = (SoilDetails)details;
//                                        if (!parsed)
//                                            Debug.Log("Failed to parse soil " + id + ":" + name);
//                                        else
//                                            soil.baseDetails = soilDetails;
//                                    }
//                                    else
//                                    {
//                                        details = new ResourceDetails(id, label, type, name, pluralName);
//                                    }

//                                    //toRet[a].Get_rows()[b].Get_rows()[c].Get_rows()[d].Get_resources()[e].Set_detail(details);

//                                }
//                                else
//                                    Debug.Log("Details for " + id + ":" + label + " does not match the resource identifiers for " + target.Get_id() + ":" + target.Get_label());
//                            }
//                            else
//                                Debug.Log("Resource details not found in library for :" + a + b + c + d + e + ": from :" + in_line);
//                        }
//                        else
//                            Debug.Log("Resource details not found in library for :" + a + b + c + d + e + ": from :" + in_line);

//                    }
//                    else
//                        Debug.Log("Resource details not found in library for :" + a + b + c + d + e + ": from :" + in_line);

//                }
//                else
//                    Debug.Log("Resource details not found in library for :" + a + b + c + d + e + ": from :" + in_line);

//            }
//            else
//                Debug.Log("Resource details not found in library for :" + a + b + c + d + e + ": from :" + in_line);


//            return toRet;
//        }

//        private bool Parse_SoilDetails(ref ResourceDetails details, int in_pos, char[] in_line, string in_id, string in_label, string in_type, string in_name, string in_pluralName)
//        {
//            if ((in_line.Length - in_pos) <= (EMPTYFIELD))
//                return false;
//            else
//            {
//                int pos = in_pos + SKIP_PLANTS + SKIP_ANIMALS + SKIP_MARINEFISH + SKIP_MARINEPLANTS + SKIP_INSECTS + SKIP_STONES + SKIP_MINERALS;
//                float minTemp = Extract_Float(in_line, ref pos);
//                float maxTemp = Extract_Float(in_line, ref pos);
//                float minPrecip = Extract_Float(in_line, ref pos);
//                float maxPrecip = Extract_Float(in_line, ref pos);
//                char associations = Extract_String(in_line, ref pos).ToCharArray()[0];

//                details = new SoilDetails(minTemp, maxTemp, minPrecip, maxPrecip, associations, in_id, in_label, in_type, in_name, in_pluralName);
//                return true;
//            }
//        }

//        private bool Parse_MineralDetails(ref ResourceDetails details, int in_pos, char[] in_line, string in_id, string in_label, string in_type, string in_name, string in_pluralName)
//        {
//            if ((in_line.Length - in_pos) <= (EMPTYFIELD))
//            {
//                return false;
//            }
//            else
//            {
//                int pos = in_pos + SKIP_PLANTS + SKIP_ANIMALS + SKIP_MARINEFISH + SKIP_MARINEPLANTS + SKIP_INSECTS + SKIP_STONES;
//                int shapeProfile = Extract_String(in_line, ref pos).ToCharArray()[0] - '0';
//                bool selfExclusive = Extract_Bool(in_line, ref pos);
//                int layerDepth = Extract_String(in_line, ref pos).ToCharArray()[0] - '0';
//                float mapScale = Extract_Float(in_line, ref pos);
//                float lowThreshold = Extract_Float(in_line, ref pos);
//                float highThreshold = Extract_Float(in_line, ref pos);
//                bool pitSuitable = Extract_Bool(in_line, ref pos);
//                float lowVolume = Extract_Float(in_line, ref pos);
//                float highVolume = Extract_Float(in_line, ref pos);
//                float lowGrade = Extract_Float(in_line, ref pos);
//                float highGrade = Extract_Float(in_line, ref pos);
//                string[] hostRocks = Extract_Ids(in_line, ref pos);

//                details = new MineralDetails(shapeProfile, selfExclusive, layerDepth, mapScale, lowThreshold, highThreshold, pitSuitable, lowVolume, highVolume, lowGrade, highGrade, hostRocks, in_id, in_label, in_type, in_name, in_pluralName);
//                return true;
//            }
//        }

//        private bool Parse_StoneDetails(ref ResourceDetails details, int in_pos, char[] in_line, string in_id, string in_label, string in_type, string in_name, string in_pluralName)
//        {
//            if ((in_line.Length - in_pos) <= (EMPTYFIELD))
//            {
//                return false;
//            }
//            else
//            {
//                int pos = in_pos + SKIP_PLANTS + SKIP_ANIMALS + SKIP_MARINEFISH + SKIP_MARINEPLANTS + SKIP_INSECTS;
//                char rockType = Extract_String(in_line, ref pos).ToCharArray()[0];
//                float scale = Extract_Float(in_line, ref pos);
//                float lowThreshold = Extract_Float(in_line, ref pos);
//                float highThreshold = Extract_Float(in_line, ref pos);

//                details = new StoneDetails(rockType, scale, lowThreshold, highThreshold, in_id, in_label, in_type, in_name, in_pluralName);
//                return true;
//            }
//        }

//        private bool Parse_InsectDetails(ref ResourceDetails details, int in_pos, char[] in_line, string in_id, string in_label, string in_type, string in_name, string in_pluralName)
//        {
//            if ((in_line.Length - in_pos) <= (EMPTYFIELD))
//            {
//                return false;
//            }
//            else
//            {
//                int pos = in_pos + SKIP_PLANTS + SKIP_ANIMALS + SKIP_MARINEFISH + SKIP_MARINEPLANTS;
//                int rarity = Extract_String(in_line, ref pos).ToCharArray()[0] - '0';
//                float minTemp = Extract_Float(in_line, ref pos);
//                float maxTemp = Extract_Float(in_line, ref pos);
//                bool coldSheltering = Extract_Bool(in_line, ref pos);
//                bool heatSheltering = Extract_Bool(in_line, ref pos);
//                char[] biomes = Extract_String(in_line, ref pos).ToCharArray();
//                int trophicIndex = (int)Extract_Float(in_line, ref pos);
//                string[] foodTargets = Extract_Ids(in_line, ref pos);

//                details = new InsectDetails(rarity, minTemp, maxTemp, coldSheltering, heatSheltering, biomes, trophicIndex, foodTargets, in_id, in_label, in_type, in_name, in_pluralName);
//                return true;
//            }
//        }

//        private bool Parse_MarinePlantDetails(ref ResourceDetails details, int in_pos, char[] in_line, string in_id, string in_label, string in_type, string in_name, string in_pluralName)
//        {
//            if ((in_line.Length - in_pos) <= (EMPTYFIELD))
//            {
//                return false;
//            }
//            else
//            {
//                int pos = in_pos + SKIP_PLANTS + SKIP_ANIMALS + SKIP_MARINEFISH;
//                int rarity = Extract_String(in_line, ref pos).ToCharArray()[0] - '0';
//                float minTemp = Extract_Float(in_line, ref pos);
//                float maxTemp = Extract_Float(in_line, ref pos);
//                char[] oceanLayers = Extract_String(in_line, ref pos).ToCharArray();

//                details = new MarinePlantDetails(rarity, minTemp, maxTemp, oceanLayers, in_id, in_label, in_type, in_name, in_pluralName);
//                return true;
//            }
//        }

//        private bool Parse_MarineFishDetails(ref ResourceDetails details, int in_pos, char[] in_line, string in_id, string in_label, string in_type, string in_name, string in_pluralName)
//        {
//            if ((in_line.Length - in_pos) <= (EMPTYFIELD))
//            {
//                return false;
//            }
//            else
//            {
//                int pos = in_pos + SKIP_PLANTS + SKIP_ANIMALS;
//                int rarity = Extract_String(in_line, ref pos).ToCharArray()[0] - '0';
//                float minTemp = Extract_Float(in_line, ref pos);
//                float maxTemp = Extract_Float(in_line, ref pos);
//                char[] oceanLayers = Extract_String(in_line, ref pos).ToCharArray();
//                char diet = Extract_String(in_line, ref pos).ToCharArray()[0];
//                //int trophicIndex = Extract_String(in_line, ref pos).ToCharArray()[0] - '0';
//                int trophicIndex = (int)Extract_Float(in_line, ref pos);
//                string[] foodTargets = Extract_Ids(in_line, ref pos);

//                details = new MarineFishDetails(rarity, minTemp, maxTemp, oceanLayers, diet, trophicIndex, foodTargets, in_id, in_label, in_type, in_name, in_pluralName);
//                return true;
//            }
//        }

//        private bool Parse_AnimalDetails(ref ResourceDetails details, int in_pos, char[] in_line, string in_id, string in_label, string in_type, string in_name, string in_pluralName)
//        {
//            //Extract_String(in_line, ref in_pos);
//            if ((in_line.Length - in_pos) <= (EMPTYFIELD + 3)) // not sure about the +3
//            {
//                //Debug.Log("Details for " + in_id + " : " + in_name + " is too small to contain data");
//                return false;
//            }
//            else
//            {
//                int pos = in_pos + SKIP_PLANTS;
//                int rarity = Extract_String(in_line, ref pos).ToCharArray()[0] - '0';
//                char diet = Extract_String(in_line, ref pos).ToCharArray()[0];
//                float adultAge = Extract_Float(in_line, ref pos);
//                float maxAge = Extract_Float(in_line, ref pos);
//                float litterSize = Extract_Float(in_line, ref pos);
//                float infantMortalityRate = Extract_Float(in_line, ref pos);
//                float adultMortalityRate = Extract_Float(in_line, ref pos);
//                float minTemp = Extract_Float(in_line, ref pos);
//                float maxTemp = Extract_Float(in_line, ref pos);
//                bool sheltersFromCold = Extract_Bool(in_line, ref pos);
//                bool sheltersFromHeat = Extract_Bool(in_line, ref pos);
//                bool canFly = Extract_Bool(in_line, ref pos);
//                bool canSwim = Extract_Bool(in_line, ref pos);
//                char[] terrains = Extract_String(in_line, ref pos).ToCharArray();
//                char[] superiorBiomes = Extract_String(in_line, ref pos).ToCharArray();
//                char[] deficientBiomes = Extract_String(in_line, ref pos).ToCharArray();
//                if (deficientBiomes.Length == 0)
//                    pos++;
//                //int trophicIndex = Extract_String(in_line, ref pos).ToCharArray()[0] - '0';
//                int trophicIndex = (int)Extract_Float(in_line, ref pos);
//                string[] foodTargets = Extract_Ids(in_line, ref pos);

//                details = new AnimalDetails(rarity, diet, adultAge, maxAge, litterSize, infantMortalityRate, adultMortalityRate, minTemp, maxTemp, sheltersFromCold, sheltersFromHeat, canFly, canSwim, terrains, superiorBiomes, deficientBiomes, trophicIndex, foodTargets, in_id, in_label, in_type, in_name, in_pluralName);
//                return true;
//            }
//        }

//        private PlantDetails ParsePlantDetails(int in_pos, char[] in_line, string in_id, string in_label, string in_type, string in_name, string in_pluralName)
//        {
//            int pos = in_pos;
//            Extract_String(in_line, ref pos); // discards the spacer //35
//            float minTemp = Extract_Float(in_line, ref pos);
//            float maxTemp = Extract_Float(in_line, ref pos);
//            float optTempMin = Extract_Float(in_line, ref pos);
//            float optTempMax = Extract_Float(in_line, ref pos);
//            float precipMin = Extract_Float(in_line, ref pos);
//            float precipMax = Extract_Float(in_line, ref pos);
//            char rarity = Extract_String(in_line, ref pos).ToCharArray()[0];

//            float shadeTol = Extract_Float(in_line, ref pos);
//            float seedFreq = Extract_Float(in_line, ref pos);
//            float seedRadi = Extract_Float(in_line, ref pos);
//            float seedAge = Extract_Float(in_line, ref pos);
//            float matureAge = Extract_Float(in_line, ref pos);
//            float maxAge = Extract_Float(in_line, ref pos);
//            float maxHeight = Extract_Float(in_line, ref pos);
//            float maxDiameter = Extract_Float(in_line, ref pos);
//            float maxVigor = Extract_Float(in_line, ref pos);
//            //Debug.Log(in_name + " : " + minTemp + " : " + maxTemp);
//            PlantDetails toRet = new PlantDetails(minTemp, maxTemp, optTempMin, optTempMax, precipMin, precipMax, rarity, shadeTol, seedFreq, seedRadi, seedAge, matureAge, maxAge, maxHeight, maxDiameter, maxVigor, in_id, in_label, in_type, in_name, in_pluralName);
//            return toRet;
//        }

//        private bool Parse_PlantDetails(ref ResourceDetails details, int in_pos, char[] in_line, string in_id, string in_label, string in_type, string in_name, string in_pluralName)
//        {
//            int pos = in_pos;
//            Extract_String(in_line, ref pos); // discards the spacer //35
//            if ((in_line.Length - pos) <= EMPTYFIELD)
//            {
//                //Debug.Log("Details for " + in_id + " : " + in_name + " is too small to contain data");
//                return false;
//            }
//            else
//            {
//                //Debug.Log("Attempting to parse " + in_id + ":" + in_label);
//                float minTemp = Extract_Float(in_line, ref pos);
//                float maxTemp = Extract_Float(in_line, ref pos);
//                float optTempMin = Extract_Float(in_line, ref pos);
//                float optTempMax = Extract_Float(in_line, ref pos);
//                float precipMin = Extract_Float(in_line, ref pos);
//                float precipMax = Extract_Float(in_line, ref pos);
//                char rarity = Extract_String(in_line, ref pos).ToCharArray()[0];

//                float shadeTol = Extract_Float(in_line, ref pos);
//                float seedFreq = Extract_Float(in_line, ref pos);
//                float seedRadi = Extract_Float(in_line, ref pos);
//                float seedAge = Extract_Float(in_line, ref pos);
//                float matureAge = Extract_Float(in_line, ref pos);
//                float maxAge = Extract_Float(in_line, ref pos);
//                float maxHeight = Extract_Float(in_line, ref pos);
//                float maxDiameter = Extract_Float(in_line, ref pos);
//                float maxVigor = Extract_Float(in_line, ref pos);
//                details = new PlantDetails(minTemp, maxTemp, optTempMin, optTempMax, precipMin, precipMax, rarity, shadeTol, seedFreq, seedRadi, seedAge, matureAge, maxAge, maxHeight, maxDiameter, maxVigor, in_id, in_label, in_type, in_name, in_pluralName);
//                return true;
//            }
//        }

//        private string Extract_ID(char[] in_line)
//        {
//            string toRet = "";
//            toRet += in_line[0];
//            toRet += in_line[1];
//            toRet += in_line[2];
//            toRet += in_line[3];
//            toRet += in_line[4];
//            return toRet;
//        }

//        public ParadigmRow[] LoadResources()
//        {
//            List<ParadigmRow> paradigms = new List<ParadigmRow>();
//            List<DomainRow> domains = new List<DomainRow>();
//            List<SectorRow> sectors = new List<SectorRow>();
//            List<ResourceRow> resources = new List<ResourceRow>();
//            String txtPath = Path.Combine(Application.dataPath, "Resources.csv");
//            StreamReader file = new StreamReader(txtPath);
//            string line;
//            int lineCount = 0;
//            int aBit = 0;
//            int bBit = 0;
//            int cBit = 0;
//            int lastA = 0;
//            int lastB = 0;
//            int lastC = 0;
//            while ((line = file.ReadLine()) != null)
//            {
//                if (lineCount != 0)
//                {
//                    //Debug.Log("Reading Resources from line :" + line);
//                    ResourceRow row = Parse_ResourceRow(line);
//                    int nextA = row.Get_aBit();
//                    int nextB = row.Get_bBit();
//                    int nextC = row.Get_cBit();
//                    if (nextA != aBit)
//                    {
//                        SectorRow lastSector = new SectorRow(aBit, bBit, cBit, resources.ToArray());
//                        resources = new List<ResourceRow>();
//                        sectors.Add(lastSector);

//                        DomainRow lastDomain = new DomainRow(aBit, bBit, sectors.ToArray());
//                        sectors = new List<SectorRow>();
//                        domains.Add(lastDomain);

//                        ParadigmRow lastParadigm = new ParadigmRow(aBit, domains.ToArray());
//                        domains = new List<DomainRow>();
//                        paradigms.Add(lastParadigm);
//                    }
//                    else if (nextB != bBit)
//                    {
//                        SectorRow lastSector = new SectorRow(aBit, bBit, cBit, resources.ToArray());
//                        resources = new List<ResourceRow>();
//                        sectors.Add(lastSector);

//                        DomainRow lastDomain = new DomainRow(aBit, bBit, sectors.ToArray());
//                        sectors = new List<SectorRow>();
//                        domains.Add(lastDomain);

//                    }
//                    else if (nextC != cBit)
//                    {
//                        SectorRow lastSector = new SectorRow(aBit, bBit, cBit, resources.ToArray());
//                        resources = new List<ResourceRow>();
//                        sectors.Add(lastSector);
//                    }

//                    resources.Add(row);

//                    lastA = aBit;
//                    lastB = bBit;
//                    lastC = cBit;

//                    aBit = nextA;
//                    bBit = nextB;
//                    cBit = nextC;
//                }
//                lineCount++;
//            }
//            //if (resources.Count > 0)
//            //{
//            //    ResourceRow terminalRow = resources[resources.Count - 1];
//            //    if (lastA != aBit)
//            //    {
//            //        SectorRow lastSector = new SectorRow(aBit, bBit, cBit, resources.ToArray());
//            //        sectors.Add(lastSector);
//            //        DomainRow lastDomain = new DomainRow(aBit, bBit, sectors.ToArray());
//            //        domains.Add(lastDomain);
//            //        ParadigmRow lastParadigm = new ParadigmRow(aBit, domains.ToArray());
//            //        paradigms.Add(lastParadigm);
//            //    }
//            //    else if (lastB == bBit)
//            //    {
//            //        SectorRow lastSector = new SectorRow(aBit, bBit, cBit, resources.ToArray());
//            //        sectors.Add(lastSector);
//            //        DomainRow lastDomain = new DomainRow(aBit, bBit, sectors.ToArray());
//            //        domains.Add(lastDomain);
//            //        paradigms.Add()
//            //    }
//            //    else if (lastC != cBit)
//            //    {
//            //        sectors.Add(new SectorRow(aBit, bBit, cBit, resources.ToArray()));

//            //    }

//            //}
//            SectorRow terminalSector = new SectorRow(aBit, bBit, cBit, resources.ToArray());
//            sectors.Add(terminalSector);

//            DomainRow terminalDomain = new DomainRow(aBit, bBit, sectors.ToArray());
//            domains.Add(terminalDomain);

//            ParadigmRow terminalParadigm = new ParadigmRow(aBit, domains.ToArray());
//            paradigms.Add(terminalParadigm);

//            return paradigms.ToArray();
//        }

//        private ResourceRow Parse_ResourceRow(string in_line)
//        {
//            //Debug.Log("Attempting to parse :" + in_line);
//            char[] line = in_line.ToArray();
//            int aBit = line[0] - '0';
//            int bBit = line[2] - '0';
//            int cBit = line[4] - '0';
//            int dBit = line[6] - '0';
//            int pos = 7;
//            string paradigm = Extract_String(line, ref pos);
//            string domain = Extract_String(line, ref pos);
//            string sector = Extract_String(line, ref pos);
//            string field = Extract_String(line, ref pos);
//            char type = Extract_String(line, ref pos).ToArray()[0];
//            List<Resource> resources = new List<Resource>();
//            string toAdd = "NOT NULL";
//            while ((pos < line.Length) && (resources.Count < 10) && (toAdd.Length > 0))
//            {
//                toAdd = Extract_String(line, ref pos);
//                if (toAdd.Length > 0)
//                {
//                    string fullID = "";
//                    fullID += aBit.ToString();
//                    fullID += bBit.ToString();
//                    fullID += cBit.ToString();
//                    fullID += dBit.ToString();
//                    fullID += resources.Count.ToString();

//                    //Debug.Log("The ID for " + toAdd + " is " +  fullID);
//                    switch (type)
//                    {
//                        // Rework coming
//                        /*
//                         * Resources to placed on the map:
//                         * A - Animal
//                         * P - Plant
//                         * G - Fungus
//                         * L - Marine Plants
//                         * F - Marine Fish
//                         * W - Freshwater Fish
//                         * I - Insects
//                         * M - Minerals
//                         * S - Stone
//                         * D - Dirt/Soil
//                         * Catch-all for all remaining resources, but doesn't have to be
//                         * R - Resource
//                         */
//                        case 'P':
//                            // Plants
//                            resources.Add(new Plant(fullID, toAdd));
//                            break;
//                        case 'A':
//                            // Animals
//                            resources.Add(new Animal(fullID, toAdd));
//                            break;
//                        case 'G':
//                            // Fungi
//                            resources.Add(new Fungus(fullID, toAdd));
//                            break;
//                        case 'L':
//                            // Marine Plants
//                            resources.Add(new MarinePlant(fullID, toAdd));
//                            break;
//                        case 'F':
//                            // Marine Fish
//                            resources.Add(new MarineFish(fullID, toAdd));
//                            break;
//                        case 'W':
//                            // Freshwater Fish
//                            resources.Add(new FreshwaterFish(fullID, toAdd));
//                            break;
//                        case 'I':
//                            // Insects
//                            resources.Add(new Insect(fullID, toAdd));
//                            break;
//                        case 'M':
//                            // Minerals
//                            resources.Add(new Mineral(fullID, toAdd));
//                            break;
//                        case 'S':
//                            // Stones
//                            resources.Add(new Stone(fullID, toAdd));
//                            break;
//                        case 'D':
//                            // Dirt/soil
//                            resources.Add(new Soil(fullID, toAdd));
//                            break;
//                        default:
//                            Debug.Log(type + "-type not found for " + fullID + ":" + toAdd);
//                            resources.Add(new Resource(fullID, toAdd));
//                            break;
//                    }
//                }
//                else
//                    break;
//            }
//            ResourceRow toRet = new ResourceRow(paradigm, domain, sector, field, type, aBit, bBit, cBit, dBit, resources.ToArray());
//            return toRet;
//        }


//        /// <summary>
//        /// Loads the Biome List.
//        /// </summary>
//        /// <returns>A biome list.</returns>
//        public WorldSpace.BiomeSpace.Biome[] LoadBiomes()
//        {
//            List<WorldSpace.BiomeSpace.Biome> biomeList = new List<WorldSpace.BiomeSpace.Biome>();

//            String txtPath = Path.Combine(Application.dataPath, "Biomes.csv");//System.AppDomain.CurrentDomain.BaseDirectory
//            StreamReader file = new StreamReader(txtPath);
//            string line;
//            while ((line = file.ReadLine()) != null)
//            {
//                biomeList.Add(Parse_Biome(line));
//            }

//            return biomeList.ToArray();
//        }

//        /// <summary>
//        /// Loads the minerals list.
//        /// </summary>
//        /// <returns>The minerals list.</returns>
//        public WorldSpace.MineralSpace.Mineralization[] LoadMinerals()
//        {
//            List<WorldSpace.MineralSpace.Mineralization> minList = new List<WorldSpace.MineralSpace.Mineralization>();
//            String txtPath = Path.Combine(Application.dataPath, "Minerals.csv");
//            StreamReader file = new StreamReader(txtPath);
//            string line;
//            while ((line = file.ReadLine()) != null)
//            {
//                minList.Add(Parse_Mineral(line));
//            }
//            return minList.ToArray();
//        }

//        /// <summary>
//        /// Loads the products list.
//        /// </summary>
//        /// <returns>The product list.</returns>
//        public ProductSpace.ProductSuperGroup[] LoadProducts()
//        {
//            String path = Path.Combine(Application.dataPath, "GameProducts.csv");
//            StreamReader file = new StreamReader(path);
//            string line;
//            List<ProductSpace.ProductRow> rows = new List<ProductSpace.ProductRow>();
//            while ((line = file.ReadLine()) != null)
//            {
//                rows.Add(Parse_ProductRow(line));
//            }
//            ProductSpace.ProductSuperGroup[] toRet = CollateGroups(CollateSubGroups(CollateProductRows(rows)));
//            return toRet;
//        }

//        /// <summary>
//        /// Loads game sub processes.
//        /// </summary>
//        /// <param name="in_table">Product lookup table.</param>
//        /// <returns>The sub process list.</returns>
//        public SubProcess[] LoadSubbies(ProductSpace.ProductTable in_table)
//        {
//            String path = Path.Combine(Application.dataPath, "SubProcesses.csv");
//            StreamReader file = new StreamReader(path);
//            List<SubProcess> subProcesses = new List<SubProcess>();
//            string line = "Not Null...";

//            while ((line = file.ReadLine()) != null)
//            {
//                subProcesses.Add(LineToSubProcess(line, in_table));
//            }
//            SubProcess[] toRet = subProcesses.ToArray();
//            return toRet;
//        }

//        /// <summary>
//        /// Loads primary game processes.
//        /// </summary>
//        /// <param name="in_table">Product lookup table.</param>
//        /// <param name="in_subs">Already filled sub process list.</param>
//        /// <returns>Primary game process list.</returns>
//        public PrimaryProcess[] LoadPrimaries(ProductSpace.ProductTable in_table, SubProcess[] in_subs)
//        {
//            String path = Path.Combine(Application.dataPath, "PrimaryProcesses.csv");
//            StreamReader file = new StreamReader(path);
//            List<PrimaryProcess> priProcesses = new List<PrimaryProcess>();
//            string line = "Not Null...";

//            while ((line = file.ReadLine()) != null)
//            {
//                priProcesses.Add(LineToPrimaryProcess(line.ToArray(), in_table, in_subs));
//            }
//            PrimaryProcess[] toRet = priProcesses.ToArray();
//            return toRet;
//        }

//        // Parsing. Turning a string into one unit of game data.

//        /// <summary>
//        /// Converts a line into a biome.
//        /// </summary>
//        /// <param name="in_line">The line containing a biome.</param>
//        /// <returns>A biome.</returns>
//        private WorldSpace.BiomeSpace.Biome Parse_Biome(string in_line)
//        {
//            WorldSpace.BiomeSpace.Biome toRet = null;
//            char[] line = in_line.ToArray();
//            int pos = 11;

//            char tmp = line[1];
//            char prcp = line[5];
//            char type = line[9];

//            string name = Extract_String(line, ref pos);
//            string mutt = "No River Mutant";
//            mutt = Extract_String(line, ref pos);
//            float spd = Extract_Float(line, ref pos);
//            float cost = Extract_Float(line, ref pos);

//            toRet = new WorldSpace.BiomeSpace.Biome(tmp, prcp, type, name, mutt, spd, cost);

//            return toRet;
//        }

//        /// <summary>
//        /// Converts a line into a mineral.
//        /// </summary>
//        /// <param name="in_line">The line containing a mineral.</param>
//        /// <returns>A mineral.</returns>
//        private WorldSpace.MineralSpace.Mineralization Parse_Mineral(string in_line)
//        {
//            WorldSpace.MineralSpace.Mineralization toRet = null;
//            char[] line = in_line.ToArray();
//            int pos = 0;

//            string name = Extract_String(line, ref pos);
//            char type = Extract_String(line, ref pos).ToArray()[0];
//            float lowL = Extract_Float(line, ref pos);
//            float uppL = Extract_Float(line, ref pos);
//            float lowM = Extract_Float(line, ref pos);
//            float uppM = Extract_Float(line, ref pos);
//            float depth = Extract_Float(line, ref pos);
//            float scale = Extract_Float(line, ref pos);
//            float lowCut = Extract_Float(line, ref pos);
//            float uppCut = Extract_Float(line, ref pos);

//            float fe = Extract_Float(line, ref pos);
//            float cu = Extract_Float(line, ref pos);
//            float sn = Extract_Float(line, ref pos);
//            float pb = Extract_Float(line, ref pos);
//            float au = Extract_Float(line, ref pos);
//            float ag = Extract_Float(line, ref pos);
//            float pt = Extract_Float(line, ref pos);
//            float ni = Extract_Float(line, ref pos);
//            float zn = Extract_Float(line, ref pos);
//            float u = Extract_Float(line, ref pos);
//            float mo = Extract_Float(line, ref pos);
//            float w = Extract_Float(line, ref pos);
//            float ti = Extract_Float(line, ref pos);
//            float cr = Extract_Float(line, ref pos);
//            float al = Extract_Float(line, ref pos);
//            float dia = Extract_Float(line, ref pos);
//            float gem = Extract_Float(line, ref pos);
//            float ree = Extract_Float(line, ref pos);
//            float oil = Extract_Float(line, ref pos);
//            float coal = Extract_Float(line, ref pos);

//            toRet = new WorldSpace.MineralSpace.Mineralization(name, type, lowL, uppL, lowM, uppM, depth, scale, lowCut, uppCut, fe, cu, sn, pb, au, ag, pt, ni, zn, u, mo, w, ti, cr, al, dia, gem, ree, oil, coal);
//            return toRet;
//        }

//        /// <summary>
//        /// Converts a string into a product row.
//        /// </summary>
//        /// <param name="in_line">A line containing a product.</param>
//        /// <returns>A product.</returns>
//        private ProductSpace.ProductRow Parse_ProductRow(string in_line)
//        {
//            char[] line = in_line.ToArray();
//            int pos = 7;

//            int a = in_line[0] - '0';
//            int b = in_line[2] - '0';
//            int c = in_line[4] - '0';
//            int d = in_line[6] - '0';
//            String name = Extract_String(line, ref pos);
//            List<ProductSpace.Product> products = new List<ProductSpace.Product>();
//            String toAdd = "Not Null";
//            while ((pos < line.Length) && (products.Count < 10) && (toAdd.Length > 0))
//            {
//                toAdd = Extract_String(line, ref pos);
//                if (toAdd.Length > 0)
//                    products.Add(new ProductSpace.Product(toAdd, a, b, c, d, products.Count));
//                else
//                    break;
//            }
//            ProductSpace.Product[] set = products.ToArray();
//            ProductSpace.ProductRow toRet = new ProductSpace.ProductRow(a, b, c, d, name, set);
//            return toRet;
//        }

//        /// <summary>
//        /// Converts a basic process into a sub process.
//        /// </summary>
//        /// <param name="in_line">A line containing a sub process.</param>
//        /// <param name="pidTable">Product look up table.</param>
//        /// <returns>A sub process.</returns>
//        private SubProcess LineToSubProcess(string in_line, ProductSpace.ProductTable pidTable)
//        {
//            Process absType = LineToProcess(in_line.ToArray(), pidTable, new Process[0]);
//            SubProcess toRet = new SubProcess(absType);
//            return toRet;
//        }

//        /// <summary>
//        /// Converts a basic process into a primary process.
//        /// </summary>
//        /// <param name="in_line">A line containing a primary process.</param>
//        /// <param name="pidTable">A product lookup table.</param>
//        /// <param name="in_subs">Already filled sub process list.</param>
//        /// <returns>A primary process.</returns>
//        private PrimaryProcess LineToPrimaryProcess(char[] in_line, ProductSpace.ProductTable pidTable, Process[] in_subs)
//        {
//            Process absType = LineToProcess(in_line, pidTable, in_subs);
//            PrimaryProcess toRet = new PrimaryProcess(absType);
//            return toRet;
//        }

//        /// <summary>
//        /// This workhorse turns a line into a basic process.
//        /// </summary>
//        /// <param name="line">The line defining a process.</param>
//        /// <param name="pidTable">Product lookup table.</param>
//        /// <param name="in_subs">Already filled sub process list.</param>
//        /// <returns>A basic process.</returns>
//        private Process LineToProcess(char[] line, ProductSpace.ProductTable pidTable, Process[] in_subs)
//        {
//            char type = line[1];
//            int pos = 3;
//            int prID = (int)Extract_Float(line, ref pos);
//            int lim = line.Length;

//            string name = Extract_String(line, ref pos);

//            List<Process_USE> uses = new List<Process_USE>();
//            List<Process_CONS> consumes = new List<Process_CONS>();
//            List<Process_FULL> fulfills = new List<Process_FULL>();
//            List<Process_BPRD> byProducts = new List<Process_BPRD>();
//            List<Process_SSUB> seqSubs = new List<Process_SSUB>();
//            List<Process_ASUB> altSubs = new List<Process_ASUB>();
//            List<Process_FAIR> fairWeather = new List<Process_FAIR>();

//            Process_PTAG primaryTag = null;
//            List<Process_STAG> subTags = new List<Process_STAG>();
//            List<Process_RTAG> reqTags = new List<Process_RTAG>();
//            List<Process_VTAG> varTags = new List<Process_VTAG>();
//            List<Process_TTAG> reqVarTags = new List<Process_TTAG>();
//            List<Process_JTAG> aloReqTags = new List<Process_JTAG>();
//            List<Process_GTAG> aloReqVarTags = new List<Process_GTAG>();

//            Process_ATMP ambientTemp = null;
//            Process_PRCP precip = null;
//            Process_TIME hours = null;
//            Process_SDAT startDays = null;
//            Process_EDAT endDays = null;
//            Process_QUAL qualityMod = null;
//            Process_EJMP endJump = null;
//            Process_MULT herdMultiplies = null;
//            Process_SEAS season = null;
//            Process_LDEP localDependency = null;

//            while (pos < lim)
//            {
//                string cmd = null;
//                if ((pos + 3) < lim)
//                    cmd = Extract_String(line, ref pos);
//                else
//                    break;

//                switch (cmd)
//                {
//                    case "USE":
//                        ProductSpace.Product USE_tgt = pidTable.Get_Product(Extract_String(line, ref pos).ToArray());
//                        float USE_qty = Extract_Float(line, ref pos);
//                        Process_USE temp_USE = new Process_USE(USE_tgt, USE_qty);
//                        uses.Add(temp_USE);
//                        break;
//                    case "CONS":
//                        ProductSpace.Product CONS_tgt = pidTable.Get_Product(Extract_String(line, ref pos).ToArray());
//                        float CONS_qty = Extract_Float(line, ref pos);
//                        Process_CONS temp_cons = new Process_CONS(CONS_tgt, CONS_qty);
//                        consumes.Add(temp_cons);
//                        break;
//                    case "FULL":
//                        ProductSpace.Product FULL_tgt = pidTable.Get_Product(Extract_String(line, ref pos).ToArray());
//                        float FULL_qty = Extract_Float(line, ref pos);
//                        Process_FULL temp_FULL = new Process_FULL(FULL_tgt, FULL_qty);
//                        fulfills.Add(temp_FULL);
//                        break;
//                    case "SSUB":
//                        Process SSUB_subP = in_subs[(int)Extract_Float(line, ref pos)];
//                        int SSUB_pos = (int)Extract_Float(line, ref pos);
//                        Process_SSUB temp_SSUB = new Process_SSUB(SSUB_subP, SSUB_pos);
//                        seqSubs.Add(temp_SSUB);
//                        break;
//                    case "ASUB":
//                        Process ASUB_subP = in_subs[(int)Extract_Float(line, ref pos)];
//                        int ASUB_aPos = (int)Extract_Float(line, ref pos);
//                        Process_ASUB temp_ASUB = new Process_ASUB(ASUB_subP, ASUB_aPos);
//                        altSubs.Add(temp_ASUB);
//                        break;
//                    case "FAIR":
//                        int FAIR_type = (int)Extract_Float(line, ref pos);
//                        Process_FAIR temp_FAIR = new Process_FAIR(FAIR_type);
//                        fairWeather.Add(temp_FAIR);
//                        break;
//                    case "BPRD":
//                        ProductSpace.Product BPRD_tgt = pidTable.Get_Product(Extract_String(line, ref pos).ToArray());
//                        float BPRD_qty = Extract_Float(line, ref pos);
//                        Process_BPRD temp_bprd = new Process_BPRD(BPRD_tgt, BPRD_qty);
//                        byProducts.Add(temp_bprd);
//                        break;
//                    case "STAG":
//                        ProductSpace.Product STAG_tag = pidTable.Get_Product(Extract_String(line, ref pos).ToArray());
//                        float STAG_score = Extract_Float(line, ref pos);
//                        Process_STAG temp_STAG = new Process_STAG(STAG_tag, STAG_score);
//                        subTags.Add(temp_STAG);
//                        break;
//                    case "RTAG":
//                        ProductSpace.Product RTAG_tag = pidTable.Get_Product(Extract_String(line, ref pos).ToArray());
//                        float RTAG_score = Extract_Float(line, ref pos);
//                        Process_RTAG temp_RTAG = new Process_RTAG(RTAG_tag, RTAG_score);
//                        reqTags.Add(temp_RTAG);
//                        break;
//                    case "VTAG":
//                        int VTAG_type = (int)Extract_Float(line, ref pos);
//                        float VTAG_score = Extract_Float(line, ref pos);
//                        Process_VTAG temp_VTAG = new Process_VTAG(VTAG_type, VTAG_score);
//                        varTags.Add(temp_VTAG);
//                        break;
//                    case "TTAG":
//                        int TTAG_type = (int)Extract_Float(line, ref pos);
//                        float TTAG_score = Extract_Float(line, ref pos);
//                        Process_TTAG temp_TTAG = new Process_TTAG(TTAG_type, TTAG_score);
//                        reqVarTags.Add(temp_TTAG);
//                        break;
//                    case "JTAG":
//                        ProductSpace.Product JTAG_tag = pidTable.Get_Product(Extract_String(line, ref pos).ToArray());
//                        float JTAG_score = Extract_Float(line, ref pos);
//                        Process_JTAG temp_JTAG = new Process_JTAG(JTAG_tag, JTAG_score);
//                        aloReqTags.Add(temp_JTAG);
//                        break;
//                    case "GTAG":
//                        int GTAG_type = (int)Extract_Float(line, ref pos);
//                        float GTAG_score = Extract_Float(line, ref pos);
//                        Process_GTAG temp_GTAG = new Process_GTAG(GTAG_type, GTAG_score);
//                        aloReqVarTags.Add(temp_GTAG);
//                        break;


//                    case "ATMP":
//                        if (ambientTemp == null)
//                        {
//                            bool ATMP_above = ZeroBool((int)Extract_Float(line, ref pos));
//                            float ATMP_tempC = Extract_Float(line, ref pos);
//                            ambientTemp = new Process_ATMP(ATMP_above, ATMP_tempC);
//                        }
//                        break;
//                    case "PRCP":
//                        if (precip == null)
//                        {
//                            float PRCP_mm = Extract_Float(line, ref pos);
//                            precip = new Process_PRCP(PRCP_mm);
//                        }
//                        break;
//                    case "TIME":
//                        if (hours == null)
//                        {
//                            float TIME_h = Extract_Float(line, ref pos);
//                            hours = new Process_TIME(TIME_h);
//                        }
//                        break;
//                    case "SDAT":
//                        if (startDays == null)
//                        {
//                            float SDAT_days = Extract_Float(line, ref pos);
//                            float SDAT_delta = Extract_Float(line, ref pos);
//                            startDays = new Process_SDAT(SDAT_days, SDAT_delta);
//                        }
//                        break;
//                    case "EDAT":
//                        if (endDays == null)
//                        {
//                            float EDAT_days = Extract_Float(line, ref pos);
//                            float EDAT_delta = Extract_Float(line, ref pos);
//                            endDays = new Process_EDAT(EDAT_days, EDAT_delta);
//                        }
//                        break;
//                    case "QUAL":
//                        if (qualityMod == null)
//                        {
//                            float QUAL_mod = Extract_Float(line, ref pos);
//                            qualityMod = new Process_QUAL(QUAL_mod);
//                        }
//                        break;
//                    case "EJMP":
//                        if (endJump == null)
//                        {
//                            int EJMP_sPos = (int)Extract_Float(line, ref pos);
//                            Process_EJMP temp_EJMP = new Process_EJMP(EJMP_sPos);
//                            endJump = temp_EJMP;
//                        }
//                        break;
//                    case "MULT":
//                        if (herdMultiplies == null)
//                        {
//                            int MULT_size = (int)Extract_Float(line, ref pos);
//                            float MULT_chance = Extract_Float(line, ref pos);
//                            Process_MULT temp_MULT = new Process_MULT(MULT_size, MULT_chance);
//                            herdMultiplies = temp_MULT;
//                        }
//                        break;
//                    case "SEAS":
//                        if (season == null)
//                        {
//                            int SEAS_id = (int)Extract_Float(line, ref pos);
//                            Process_SEAS temp_SEAS = new Process_SEAS(SEAS_id);
//                            season = temp_SEAS;
//                        }
//                        break;
//                    case "PTAG":
//                        if (primaryTag == null)
//                        {
//                            ProductSpace.Product PTAG_tag = pidTable.Get_Product(Extract_String(line, ref pos).ToArray());
//                            float PTAG_score = Extract_Float(line, ref pos);
//                            Process_PTAG temp_PTAG = new Process_PTAG(PTAG_tag, PTAG_score);
//                            primaryTag = temp_PTAG;
//                        }
//                        break;
//                    case "LDEP":
//                        if (localDependency == null)
//                        {
//                            int LDEP_type = (int)Extract_Float(line, ref pos);
//                            Process_LDEP temp_LDEP = new Process_LDEP(LDEP_type);
//                            localDependency = temp_LDEP;
//                        }
//                        break;
//                    default:
//                        pos++;
//                        break;
//                }
//            }

//            Process toRet = new Process(name, type, prID, uses.ToArray(), consumes.ToArray(), fulfills.ToArray(), byProducts.ToArray(), seqSubs.ToArray(), altSubs.ToArray(), fairWeather.ToArray(), primaryTag, subTags.ToArray(), reqTags.ToArray(), varTags.ToArray(), reqVarTags.ToArray(), aloReqTags.ToArray(), aloReqVarTags.ToArray(), ambientTemp, precip, hours, startDays, endDays, qualityMod, endJump, herdMultiplies, season, localDependency);
//            return toRet;
//        }

//        /// <summary>
//        /// If zero then true.
//        /// </summary>
//        /// <param name="in_preState">The integer value.</param>
//        /// <returns>A true or false statement.</returns>
//        private bool ZeroBool(int in_preState)
//        {
//            bool toRet = false;
//            if (in_preState == 0)
//                toRet = true;
//            return toRet;
//        }

//        /// <summary>
//        /// Collates product rows into product sub groups.
//        /// </summary>
//        /// <param name="in_rows">Complete product rows.</param>
//        /// <returns>Product sub groups.</returns>
//        private ProductSpace.ProductSubGroup[] CollateProductRows(List<ProductSpace.ProductRow> in_rows)
//        {
//            int pos = 0;
//            int lim = in_rows.Count;
//            List<ProductSpace.ProductSubGroup> preRet = new List<ProductSpace.ProductSubGroup>();
//            const String defaultName = "DEFAULT SUB GROUP NAME";

//            while (pos < lim)
//            {
//                int[] address = in_rows[pos].Get_rowAddress();
//                int a = address[0];
//                int b = address[1];
//                int c = address[2];
//                int d = address[3];
//                int aMark = a;
//                int bMark = b;
//                int cMark = c;
//                int dMark = d;
//                List<ProductSpace.ProductRow> rows = new List<ProductSpace.ProductRow>();
//                //Console.WriteLine("sub row " + a + b + c + d + " " + pos + "/" + lim);
//                while (((c == cMark) && (b == bMark) && (a == aMark)) && (pos < lim))
//                {
//                    address = in_rows[pos].Get_rowAddress();
//                    a = address[0];
//                    b = address[1];
//                    c = address[2];
//                    d = address[3];
//                    if ((c == cMark) && (b == bMark) && (a == aMark))
//                    {
//                        rows.Add(in_rows[pos]);
//                        pos++;
//                    }
//                    else break;
//                    //Console.WriteLine("Rows " + a + b + c + d + " " + pos + "/" + lim);
//                }
//                rows.TrimExcess();
//                ProductSpace.ProductRow[] rowSet = rows.ToArray();
//                ProductSpace.ProductSubGroup toAdd = new ProductSpace.ProductSubGroup(aMark, bMark, cMark, defaultName, rowSet);
//                //Console.WriteLine("...into sub group " + preRet.Count + " with rows numbering " + rowSet.Length);
//                preRet.Add(toAdd);
//            }
//            preRet.TrimExcess();
//            ProductSpace.ProductSubGroup[] subSet = preRet.ToArray();
//            return subSet;
//        }

//        /// <summary>
//        /// Collates product sub groups into product groups.
//        /// </summary>
//        /// <param name="in_subs">Complete sub groups.</param>
//        /// <returns>Product groups.</returns>
//        private ProductSpace.ProductGroup[] CollateSubGroups(ProductSpace.ProductSubGroup[] in_subs)
//        {
//            //Console.WriteLine();
//            int pos = 0;
//            int lim = in_subs.Length;
//            List<ProductSpace.ProductGroup> preRet = new List<ProductSpace.ProductGroup>();
//            const String DEFAULTSUBGROUPNAME = "DEFAULT SUB GROUP NAME";
//            while (pos < lim)
//            {
//                int[] address = in_subs[pos].Get_subGroupAddresss();
//                int a = address[0];
//                int b = address[1];
//                int c = address[2];
//                int aMark = a;
//                int bMark = b;
//                int cMark = c;
//                List<ProductSpace.ProductSubGroup> subbies = new List<ProductSpace.ProductSubGroup>();
//                while (((b == bMark) && (a == aMark)) && (pos < lim))
//                {
//                    //Console.WriteLine("Subs " + a + b + c + " " + pos + "/" + lim + " with a row length of " + in_subs[pos].Get_rows().Length);

//                    if ((b == bMark) && (a == aMark))
//                        subbies.Add(in_subs[pos]);
//                    pos++;
//                    if (pos < lim)
//                        address = in_subs[pos].Get_subGroupAddresss();

//                    a = address[0];
//                    b = address[1];
//                    c = address[2];
//                }
//                subbies.TrimExcess();
//                ProductSpace.ProductSubGroup[] subSet = subbies.ToArray();
//                ProductSpace.ProductGroup toAdd = new ProductSpace.ProductGroup(aMark, bMark, DEFAULTSUBGROUPNAME, subSet);
//                //Console.WriteLine("...into group " + preRet.Count + " with a subSet numbering " + subSet.Length);
//                preRet.Add(toAdd);
//            }
//            preRet.TrimExcess();
//            ProductSpace.ProductGroup[] toRet = preRet.ToArray();
//            return toRet;
//        }

//        /// <summary>
//        /// Collates product groups into super groups.
//        /// </summary>
//        /// <param name="in_groups">Complete product groups.</param>
//        /// <returns>Product super groups.</returns>
//        private ProductSpace.ProductSuperGroup[] CollateGroups(ProductSpace.ProductGroup[] in_groups)
//        {
//            //Console.WriteLine();
//            int pos = 0;
//            int lim = in_groups.Length;
//            List<ProductSpace.ProductSuperGroup> suppies = new List<ProductSpace.ProductSuperGroup>();
//            const String DEFAULTNAME = "DEFAULT SUPERGROUP NAME";
//            while (pos < lim)
//            {
//                int[] address = in_groups[pos].Get_groupAddress();
//                int a = address[0];
//                int b = address[0];
//                int aMark = a;
//                int bMark = b;
//                List<ProductSpace.ProductGroup> groups = new List<ProductSpace.ProductGroup>();
//                while (((a == aMark)) && (pos < lim))
//                {

//                    //Console.WriteLine("Group " + a + b + " " + pos + "/" + lim + " with a sub length of " + in_groups[pos].Get_subbies().Length);

//                    if ((a == aMark))
//                        groups.Add(in_groups[pos]);
//                    pos++;
//                    if (pos < lim)
//                        address = in_groups[pos].Get_groupAddress();
//                    a = address[0];
//                    b = address[1];

//                }
//                groups.TrimExcess();
//                ProductSpace.ProductGroup[] groupSet = groups.ToArray();
//                ProductSpace.ProductSuperGroup toAdd = new ProductSpace.ProductSuperGroup(aMark, DEFAULTNAME, groupSet);
//                suppies.Add(toAdd);
//                //Console.WriteLine("...into superGroup " + suppies.Count + " with a groupSet numbering " + groupSet.Length);

//            }
//            suppies.TrimExcess();
//            ProductSpace.ProductSuperGroup[] toRet = suppies.ToArray();
//            return toRet;
//        }

//        private string[] Extract_Ids(char[] in_line, ref int pos)
//        {
//            List<string> toRet = new List<string>();
//            if (IsStartChar(ref pos, in_line))
//            {
//                while ((!IsBreakChar(ref pos, in_line)))
//                {
//                    string phrase = "";
//                    while ((!IsBreakChar(ref pos, in_line)) && (in_line[pos] != ';'))
//                    {
//                        if (in_line[pos] != ',')
//                        {
//                            if (in_line[pos] != ' ')
//                                phrase += in_line[pos];
//                        }
//                        else
//                            break;
//                        pos++;
//                    }
//                    if (phrase.Length == 5)
//                        toRet.Add(phrase);
//                    pos++;
//                }
//            }

//            return toRet.ToArray();
//        }

//        private bool Extract_Bool(char[] in_line, ref int pos)
//        {
//            bool toRet = false;
//            if (IsStartChar(ref pos, in_line))
//            {
//                char boolChar = in_line[pos];
//                if ((boolChar == 'Y') || (boolChar == 'y') || (boolChar == '0'))
//                    toRet = true;
//                pos++;
//                while (!IsBreakChar(ref pos, in_line))
//                {
//                    pos++;
//                    Debug.Log("Extra characters detected in a bool cell");
//                }

//            }
//            return toRet;
//        }

//        /// <summary>
//        /// Turns chars begining at the pos into a float.
//        /// </summary>
//        /// <param name="in_line">The line containing the float.</param>
//        /// <param name="pos">The refernce position to start in the line.</param>
//        /// <returns>A float, also moves the pos to a break char.</returns>
//        private float Extract_Float(char[] in_line, ref int pos)
//        {
//            int goBack = pos;
//            float toRet = 0.0f;
//            bool negative = false;
//            bool decy = false;
//            int expo = 0;
//            float left = 0.0f;
//            float right = 0.0f;
//            if (IsStartChar(ref pos, in_line))
//            {
//                while (!IsBreakChar(ref pos, in_line))
//                {
//                    if (in_line[pos] == '-')
//                    {
//                        negative = true;
//                        pos++;
//                    }
//                    if (in_line[pos] == '.')
//                    {
//                        decy = true;
//                        pos++;
//                    }
//                    if (!decy)
//                    {
//                        if (left == 0.0f)
//                            left += in_line[pos] - '0';
//                        else
//                        {
//                            left *= 10.0f;
//                            left += in_line[pos] - '0';
//                        }
//                        pos++;
//                    }
//                    else
//                    {
//                        int preVal = in_line[pos] - '0';
//                        expo++;
//                        float toAdd = ((float)preVal) / ((float)Math.Pow(10, expo));
//                        right += toAdd;
//                        pos++;
//                    }
//                }
//            }
//            //else
//            //{
//            //    Console.WriteLine("Couldn't find a start char...");

//            //    int sPos = 0;
//            //    int lim = goBack;
//            //    while (sPos < lim)
//            //    {
//            //        Console.Write(' ');
//            //        sPos++;
//            //    }
//            //    Console.WriteLine("@");
//            //    DBGOutLine(in_line);
//            //    Console.ReadLine();
//            //}
//            float modifier = 1f;
//            if (negative)
//                modifier = -1f;
//            toRet = modifier * (left + right);
//            return toRet;
//        }

//        /// <summary>
//        /// Turns chars beinging at the pos into a string. No commas, colons, or quotations allowed.
//        /// </summary>
//        /// <param name="in_line">The line containing the string.</param>
//        /// <param name="pos">The reference position to start int the line.</param>
//        /// <returns>A string, also moves the pos to a break char.</returns>
//        private string Extract_String(char[] in_line, ref int pos)
//        {
//            int goBack = pos;
//            string toRet = "";
//            if (IsStartChar(ref pos, in_line))
//            {
//                while (!IsBreakChar(ref pos, in_line))
//                {
//                    if (in_line[pos] != '"')
//                        toRet += in_line[pos];
//                    pos++;
//                }
//            }
//            //else
//            //{
//            //    Console.WriteLine("Couldn't find a start char...");
//            //    int sPos = 0;
//            //    int lim = goBack;
//            //    while (sPos < lim)
//            //    {
//            //        Console.Write(' ');
//            //        sPos++;
//            //    }
//            //    Console.WriteLine("@");
//            //    DBGOutLine(in_line);
//            //    Console.ReadLine();
//            //    pos++;
//            //}
//            return toRet;
//        }

//        /// <summary>
//        /// Performs a lot of tests to see if the chars around the pos are valid to begin, and moves the pos to the first valid char.
//        /// </summary>
//        /// <param name="pos">The initial pos. Should be on a break char or fresh line.</param>
//        /// <param name="in_line">The line.</param>
//        /// <returns>True if it is a break char, and moves the pos. False if not a break char.</returns>
//        private bool IsStartChar(ref int pos, char[] in_line)
//        {
//            bool toRet = false;

//            if (pos < in_line.Length)
//            {
//                char a = '@';
//                if (pos > 0)
//                {
//                    a = in_line[pos - 1];
//                }
//                char b = in_line[pos];
//                char c = '@';
//                if ((pos + 1) == in_line.Length)
//                {
//                    toRet = false;
//                    return toRet;
//                }
//                else
//                    c = in_line[pos + 1];
//                if (Condition_StartA(a, b))
//                {
//                    toRet = true;
//                }

//                else if (Condition_StartB(b, c))
//                {
//                    toRet = true;
//                    pos = pos + 2;
//                }
//                else if ((Condition_StartC(b, c)) || (Condition_StartE(b, c)) || (Condition_StartF(b, c)))
//                {
//                    toRet = true;
//                    pos = pos + 1;
//                }
//                else if (Condition_StartD(a, b))
//                {
//                    toRet = true;
//                }
//                else if (Condition_StartIgnore(a, b, c))
//                {
//                    //if (Program.DEBUG)
//                    //    Console.WriteLine("Ignoring field...");
//                    //pos++;
//                }
//                else
//                {
//                    //Console.WriteLine("IsStartChar(...) failed. a: " + a + " : b: " + b + " : c: " + c);
//                    //int sPos = 0;
//                    //int lim = pos - 1;
//                    //while (sPos < lim)
//                    //{
//                    //    Console.Write(' ');
//                    //    sPos++;
//                    //}
//                    //Console.WriteLine("<@>");
//                    //DBGOutLine(in_line);
//                    //Console.ReadLine();
//                    Debug.Log("IsStartChar(...) failed. a: " + a + " : b: " + b + " : c: " + c + " at position :" + pos + " line is :" + CharsToString(in_line));
//                }
//            }


//            return toRet;
//        }

//        private string CharsToString(char[] line)
//        {
//            StringBuilder builder = new StringBuilder();
//            int pos = 0;
//            while (pos < line.Length)
//            {
//                char c = line[pos];
//                builder.Append(c);
//                pos++;
//            }
//            return builder.ToString();
//        }

//        /// <summary>
//        /// A : and B not "
//        /// </summary>
//        /// <param name="a">The previous character.</param>
//        /// <param name="b">The current character.</param>
//        /// <returns>A true or false statement.</returns>
//        private bool Condition_StartA(char a, char b)
//        {
//            bool toRet = false;
//            if ((a == ':') && (b != '"'))
//                toRet = true;
//            return toRet;
//        }

//        /// <summary>
//        /// B , and c "
//        /// </summary>
//        /// <param name="b">The current character.</param>
//        /// <param name="c">The next character.</param>
//        /// <returns>A true or false statement.</returns>
//        private bool Condition_StartB(char b, char c)
//        {
//            bool toRet = false;
//            if ((b == ',') && (c == '"'))
//                toRet = true;
//            return toRet;
//        }

//        /// <summary>
//        /// B , and C not ,
//        /// </summary>
//        /// <param name="b">The current character.</param>
//        /// <param name="c">The next character.</param>
//        /// <returns>A true or false statement.</returns>
//        private bool Condition_StartC(char b, char c)
//        {
//            bool toRet = false;
//            if ((b == ',') && (c != ','))
//                toRet = true;
//            return toRet;
//        }

//        /// <summary>
//        /// A , and B not ,
//        /// </summary>
//        /// <param name="a">The previous character.</param>
//        /// <param name="b">The current character.</param>
//        /// <returns>A true or false statement.</returns>
//        private bool Condition_StartD(char a, char b)
//        {
//            bool toRet = false;
//            if ((a == ',') && (b != ','))
//                toRet = true;
//            return toRet;
//        }

//        /// <summary>
//        /// B " and C not "
//        /// </summary>
//        /// <param name="b">The current character.</param>
//        /// <param name="c">the previous character.</param>
//        /// <returns>A true or false statement.</returns>
//        private bool Condition_StartE(char b, char c)
//        {
//            bool toRet = false;
//            if ((b == '"') && (c != '"'))
//                toRet = true;
//            return toRet;
//        }

//        /// <summary>
//        /// B : and C not "
//        /// </summary>
//        /// <param name="b">The current character.</param>
//        /// <param name="c">The next Character.</param>
//        /// <returns>A true or false statement.</returns>
//        private bool Condition_StartF(char b, char c)
//        {
//            bool toRet = false;
//            if ((b == ':') && (c != '"'))
//                toRet = true;
//            return toRet;
//        }

//        /// <summary>
//        /// Ignore the sequential commas.
//        /// </summary>
//        /// <param name="a">The previous character.</param>
//        /// <param name="b">The current character.</param>
//        /// <param name="c">The next character.</param>
//        /// <returns></returns>
//        private bool Condition_StartIgnore(char a, char b, char c)
//        {
//            bool toRet = false;
//            if (((a == ',') && (b == ',')) || ((b == ',') && (c == ',')))
//                toRet = true;
//            return toRet;
//        }

//        /// <summary>
//        /// Performs a check to see if the chars around the pos a breaking character, such as , : or "
//        /// </summary>
//        /// <param name="pos">The current position.</param>
//        /// <param name="in_line">The line.</param>
//        /// <returns>True if breaking, moves pos.</returns>
//        private bool IsBreakChar(ref int pos, char[] in_line)
//        {
//            bool toRet = false;
//            if ((pos + 1) > in_line.Length)
//            {
//                //Console.WriteLine("Abrupt line end.");
//                //int sPos = 0;
//                //int lim = pos;
//                //while (sPos < lim)
//                //{
//                //    Console.Write(' ');
//                //    sPos++;
//                //}
//                //Console.WriteLine("<@>");
//                //DBGOutLine(in_line);
//                //Console.ReadLine();
//                toRet = true;
//            }
//            else if ((pos + 1) == in_line.Length)
//            {
//                toRet = false;
//            }
//            else
//            {
//                int next = pos + 1;

//                char a = in_line[pos - 1];
//                char b = in_line[pos];
//                char c = in_line[next];

//                if (Condition_BreakA(b, c))
//                {
//                    toRet = true;
//                    pos = next;
//                }
//                else if ((Condition_BreakC(a, b)) || (Condition_BreakB(b, c)) || (Condition_BreakD(b, c)))
//                    toRet = true;
//            }

//            return toRet;
//        }

//        /// <summary>
//        /// B " and C ,
//        /// </summary>
//        /// <param name="in_b">The current character.</param>
//        /// <param name="in_c">The next character.</param>
//        /// <returns>A true or false statement.</returns>
//        private bool Condition_BreakA(char in_b, char in_c)
//        {
//            bool toRet = false;
//            if ((in_b == '"') && (in_c == ','))
//                toRet = true;
//            return toRet;
//        }

//        /// <summary>
//        /// B : and C not "
//        /// </summary>
//        /// <param name="in_b">The current character.</param>
//        /// <param name="in_c">The next character.</param>
//        /// <returns>A true or false statement.</returns>
//        private bool Condition_BreakB(char in_b, char in_c)
//        {
//            bool toRet = false;
//            if ((in_b == ':') && (in_c != '"'))
//                toRet = true;
//            return toRet;
//        }

//        /// <summary>
//        /// A " and B ,
//        /// </summary>
//        /// <param name="in_a">The previous character.</param>
//        /// <param name="in_b">The current character.</param>
//        /// <returns>A true or false statement.</returns>
//        private bool Condition_BreakC(char in_a, char in_b)
//        {
//            bool toRet = false;
//            if ((in_a == '"') && (in_b == ','))
//                toRet = true;
//            return toRet;
//        }

//        /// <summary>
//        /// B , and C not ,
//        /// </summary>
//        /// <param name="in_b">The current character.</param>
//        /// <param name="in_c">The next character.</param>
//        /// <returns>A true or false statement.</returns>
//        private bool Condition_BreakD(char in_b, char in_c)
//        {
//            bool toRet = false;
//            if ((in_b == ',') && (in_c != ','))
//                toRet = true;
//            return toRet;
//        }

//        /// <summary>
//        /// Quick output of the line. Doesn't end the line.
//        /// </summary>
//        /// <param name="in_line"></param>
//        private void DBGOutLine(char[] in_line)
//        {
//            int pos = 0;
//            int lim = in_line.Length;
//            while (pos < lim)
//            {
//                Console.Write(in_line[pos]);
//                pos++;
//            }
//        }
//    }

