/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;

public class CopyFormat 
{

    public static string FormatBrand(Brand brand)
    {
        switch(brand)
        {
            case Brand.JAGUAR:
                return "Jaguar";
            case Brand.RANGE_ROVER:
                return "Range Rover";
            case Brand.LAND_ROVER:
                return "Land Rover";
            default:
                return "BRAND";
        }
    }

}
