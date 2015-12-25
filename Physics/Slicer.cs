﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Physics
{
    public static class Slicer
    {

        public static List<Slice> SliceRect(BoundingRect inner, BoundingRect outter, int slices)
        {
            var sliceList = new List<Slice>();
            var horizontalMargin = inner.X - outter.X;
            var verticalMargin = outter.Height - inner.Height;
            var depthMargin = outter.Z - inner.Z;

            var leftSlice = new Slice(outter.X, outter.Y, outter.Z, horizontalMargin, outter.Height, depthMargin);
            var upperSlice = new Slice(outter.X, inner.Height, outter.Z, outter.Width, verticalMargin, depthMargin);
            var rightSlice = new Slice(inner.X + inner.Width, outter.Y, outter.Z, horizontalMargin, outter.Height, depthMargin);
            sliceList.Add(leftSlice);
            sliceList.Add(upperSlice);
            sliceList.Add(rightSlice);
            return sliceList;
        }
    }
}