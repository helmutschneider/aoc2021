﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AOC2023;

public enum Category
{
    None = 0,
    Seed,
    Soil,
    Fertilizer,
    Water,
    Light,
    Temperature,
    Humidity,
    Location,
}

public record Map(Category From, Category To, IReadOnlyList<MapRange> Ranges);
public record MapRange(long DestStart, long SourceStart, long Length)
{
    public long? MaybeGetMapping(long value)
    {
        var hasMapping = value >= this.SourceStart && value < (this.SourceStart + this.Length);
        if (hasMapping)
        {
            var offset = value - this.SourceStart;
            return this.DestStart + offset;
        }
        return null;
    }
}
public record Seeds(IReadOnlyList<long> Nums);

public static partial class Day05
{
    public static long GetLocation(long seed, IReadOnlyList<Map> maps)
    {
        var cat = Category.Seed;
        var iter = seed;

        while (cat != Category.Location)
        {
            var didFindMapping = false;

            foreach (var map in maps)
            {
                if (map.From == cat)
                {
                    didFindMapping = true;
                    var mapping = iter;
                    cat = map.To;

                    foreach (var range in map.Ranges)
                    {
                        if (range.MaybeGetMapping(iter) is long m)
                        {
                            mapping = m;
                            break;
                        }
                    }

                    iter = mapping;
                }
            }

            if (!didFindMapping)
            {
                throw new Exception($"Did not find mapping for '{cat}'");
            }
        }

        return iter;
    }

    [GeneratedRegex(@"^(\w+)-to-(\w+) map:")]
    private static partial Regex MapPattern();

    [GeneratedRegex(@"^(\d+) (\d+) (\d+)")]
    private static partial Regex MapRangePattern();

    public static (Seeds, IReadOnlyList<Map>) Parse(string input)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        using var rdr = new StreamReader(stream);
        var seedNumbers = new List<long>();
        var maps = new List<Map>();

        while (!rdr.EndOfStream)
        {
            var line = rdr.ReadLine()?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (line.StartsWith("seeds:"))
            {
                var parts = line.Split(":");
                var nums = Regex.Split(parts[1].Trim(), @"\s+");
                
                foreach (var num in nums)
                {
                    seedNumbers.Add(long.Parse(num));
                }

                continue;
            }

            var match = MapPattern().Match(line);
            
            if (match?.Success == true)
            {
                var from = match.Groups[1].Value;
                var to = match.Groups[2].Value;
                var ranges = new List<MapRange>();

                while (!rdr.EndOfStream)
                {
                    var line2 = rdr.ReadLine()?.Trim() ?? string.Empty;
                    var match2 = MapRangePattern().Match(line2);

                    if (match2?.Success != true)
                    {
                        break;
                    }

                    var destStart = long.Parse(match2.Groups[1].Value);
                    var sourceStart = long.Parse(match2.Groups[2].Value);
                    var len = long.Parse(match2.Groups[3].Value);

                    ranges.Add(new MapRange(destStart, sourceStart, len));
                }

                var fromCat = ParseCategory(from);
                var toCat = ParseCategory(to);

                maps.Add(new Map(fromCat, toCat, ranges));
            }
        }

        return (new Seeds(seedNumbers), maps);
    }

    static Category ParseCategory(string value)
    {
        return value switch
        {
            "seed" => Category.Seed,
            "soil" => Category.Soil,
            "fertilizer" => Category.Fertilizer,
            "water" => Category.Water,
            "light" => Category.Light,
            "temperature" => Category.Temperature,
            "humidity" => Category.Humidity,
            "location" => Category.Location,
            _ => throw new ArgumentException($"bad category '{value}'"),
        };
    }

    public const string INPUT = @"
seeds: 3082872446 316680412 2769223903 74043323 4131958457 99539464 109726392 353536902 619902767 648714498 3762874676 148318192 1545670780 343889780 4259893555 6139816 3980757676 20172062 2199623551 196958359

seed-to-soil map:
2211745924 1281207339 39747980
3648083739 2564129012 145170114
4171944574 2333022880 44675857
540694760 848661020 78793182
256996824 588160543 260500477
1870557289 1804847051 174857657
3877597859 2853012070 228980636
1634159465 2150723562 100770342
3793253853 2293912908 39109972
652571990 567856215 20304328
2480343183 3372556760 130573730
1831144195 528443121 39413094
0 1690920197 113926854
3145720856 3081992706 290564054
624623106 1979704708 27948884
3844601856 3751059243 32996003
1260492360 1175075910 106131429
1366623789 166330978 138490835
1175000149 927454202 85492211
696570061 1596389312 94530885
2647046837 3784055246 498674019
4216620431 2709299126 78346865
953230443 1450000160 146389152
791100946 1012946413 162129497
1734929807 427093569 96214388
672876318 403399826 23693743
113926854 2007653592 143069970
2045414946 0 166330978
1099619595 328019272 75380554
3832363825 4282729265 12238031
619487942 523307957 5135164
517497301 304821813 23197459
2293912908 2377698737 186430275
1505114624 1320955319 129044841
2610916913 3714929319 36129924
3436284910 3503130490 211798829
4106578495 2787645991 65366079

soil-to-fertilizer map:
2733576308 471599794 76965554
1171423854 1329782324 37554133
2640052871 928987130 93523437
2015828352 548565348 204028986
3562821857 3651707516 643259780
1208977987 2596877127 12575372
778871551 2204324824 392552303
1221553359 2609452499 201089363
3520687457 3069361301 42134400
4240454288 3542205804 54513008
2219857338 1367336457 420195533
3034988650 3111495701 430710103
307271757 0 471599794
1422642722 2082393746 121931078
3465698753 3596718812 54988704
0 1022510567 307271757
1544573800 1787531990 294861756
4206081637 3034988650 34372651
1839435556 752594334 176392796

fertilizer-to-water map:
1807260819 3957534991 337432305
774926879 2718324291 701236360
2351569884 1690420176 794087185
313174888 2484507361 233816930
3145657069 541109949 949949029
546991818 313174888 227935061
2144693124 3750658231 206876760
4095606098 1491058978 199361198
1476163239 3419560651 331097580

water-to-light map:
3834982820 3688486185 202897824
2016707141 372287565 116618935
3386838019 3412408553 81116937
1125723906 705568567 205087174
4037880644 1840142480 150018623
2359176858 4109550629 126312910
3328178239 4050890849 58659780
1801115923 3893484758 43944958
1516002989 1645262885 194879595
3501845456 488906500 216662067
1373868013 2223115169 142134976
3467954956 3641943956 33890500
1845060881 3937429716 113461133
3718507523 910655741 57371540
3315526510 3675834456 12651729
2936874590 2031117287 84929853
1710882584 3551710617 90233339
372287565 2365250145 753436341
2133326076 968027281 225850782
1330811080 1990161103 40956184
1958522014 3493525490 58185127
3021804443 3118686486 293722067
2485489768 1193878063 451384822
3775879063 4235863539 59103757
1371767264 3891384009 2100749
4187899267 2116047140 107068029

light-to-temperature map:
156743496 2059819668 37694357
4058204935 4136802755 38991573
2484168315 1803830764 54458297
2053264847 2531370441 7735546
586814267 2539105987 96956250
2538626612 2097514025 117228608
4097196508 3782742182 197770788
1246999413 607900903 25957627
1877009740 1752361784 30081637
683770517 3121708332 89729874
1835387899 343006762 41621841
1806332066 3242032508 29055833
2212907940 137512351 205494411
809588378 2954088675 69458905
1689902424 3271088341 436818
3306521233 894737794 308504080
3235066415 3050253514 71454818
1147299995 1960120250 99699418
2046683718 749851354 6581129
3782742182 4222506720 5456115
2061000393 756432483 138305311
792839631 591152156 16748747
1478425864 2742612115 211476560
2793367571 2636062237 73608402
2471831801 2214742633 12336514
194437853 1622309324 130052460
3901915150 3980512970 156289785
2655855220 0 137512351
324490313 1359985370 262323954
3855202758 4175794328 46712392
2445108285 1782443421 21387343
2467550357 3211438206 4281444
3788198297 4227962835 67004461
2199305704 2709670639 13602236
1272957040 385683332 205468824
1690339242 633858530 115992824
1907091377 2227079147 139592341
0 1203241874 156743496
2866975973 1858289061 24590288
1120987137 3215719650 26312858
2891566261 3271525159 343500154
2466495628 384628603 1054729
1043746236 1882879349 77240901
879047283 2366671488 164698953
773500391 2723272875 19339240
2418402351 3023547580 26705934

temperature-to-humidity map:
159374282 333555332 155635040
2263203984 507165487 202752561
1337996197 836383358 269347352
733930089 139752127 104367475
2840449774 3878218681 416748615
3373626730 3111757526 416998602
3943881399 2795875886 241056063
3790625332 2642619819 153256067
3298801153 3036931949 30341941
1671783850 1980182260 485774285
0 313933177 19622155
2642619819 3680388726 197829955
2157558135 1398186167 105645849
4283100212 3528756128 11867084
843670838 1503832016 347905328
1607343549 244119602 64440301
315009322 709918048 126465310
441474632 1105730710 292455457
3257198389 3540623212 41602764
838297564 308559903 5373274
1320021082 489190372 17975115
4184937462 3582225976 98162750
1191576166 1851737344 128444916
3329143094 3067273890 44483636
19622155 0 139752127

humidity-to-location map:
3728200417 3220538748 36833684
1907946842 1065499951 70751518
1978698360 2011387412 298481649
4048923771 3541262102 246043525
1314245652 402299218 396512619
577039234 798811837 266688114
3249926596 2795059130 273974120
1734859977 229212353 173086865
2718652009 3787305627 454867466
1710758271 1405898165 24101706
843727348 1510857580 66114574
3765034101 3257372432 283889670
1179488618 1876630378 134757034
909841922 1136251469 269646696
3675406214 4242173093 52794203
310070062 1576972154 266969172
229212353 1429999871 80857709
3523900716 3069033250 151505498
3173519475 2718652009 76407121
2277180009 1843941326 32689052
    ";
}
