using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BluePrint
{
    public class BlueprintConnection
    {
        public BluePrintPin connectStart;
        public BluePrintPin connectEnd;
        private float _deltaX = 10;
        private float _deltaY = 70;

        public BlueprintConnection(BluePrintPin start, BluePrintPin end)
        {
            connectStart = start;
            connectEnd = end;
            _deltaX = start.connectDeltaX;
            _deltaY = start.connectDeltaY;
        }

        public bool Compare(BlueprintConnection tmp)
        {
            if (connectStart != tmp.connectStart) return false;
            if (connectEnd != tmp.connectEnd) return false;

            return true;
        }        

        public void Draw()
        {
            Vector3 start = new Vector3(connectStart.xMax, connectStart.yMin + (connectStart.yMax - connectStart.yMin) * GetHeight());
            Vector3 end = new Vector3(connectEnd.xMin, connectEnd.yMin + (connectEnd.yMax - connectEnd.yMin) * GetHeight());
            Color lineColor = (connectStart.GetNode<BluePrintNode>().hasError && connectEnd.GetNode<BluePrintNode>().hasError) ? Color.red : GetConnectionLineColor();
            switch(BlueprintEditor.LineMode)
            {
                case 2:
                    {
                        DrawTool.DrawConnectionLine(start, end, lineColor, null, connectStart.Scale);
                    }
                    break;
                case 1:
                default:
                    {
                        DrawTool.DrawConnectionLine(start, end, lineColor, null, connectStart.Scale, _deltaX, _deltaY);
                    }
                    break;
            }

        }

        public static float GetHeight()
        {
            return 0.5f;
        }

        public float GetEndRange()
        {
            float delta = (connectEnd.yMax - connectEnd.yMin) / 2;
            return 100f + delta;
        }

        public float GetStartRange()
        {
            float maxRange = 200f + connectStart.yMax - connectStart.yMin;
            return maxRange - GetEndRange();
        }

        public Color GetConnectionLineColor()
        {
            if (connectStart.pinStream == PinStream.Out)
            {
                if (connectStart.pinType == PinType.Main)
                {
                    if (connectStart != null && connectStart.IsPinExecuted() &&
                         connectEnd != null && connectEnd.IsPinExecuted())
                        return BluePrintHelper.PinMainActiveColor;
                    if (connectStart.GetNode<BluePrintNode>().IsSelected|| connectEnd.GetNode<BluePrintNode>().IsSelected)
                        return connectStart.SelectColor;
                    return BluePrintHelper.PinMainNoActiveColor;
                }

                if (connectStart.pinType == PinType.Data) return BluePrintHelper.PinDataLineColor;
            }

            return BluePrintHelper.PinErrorLineColor;
        }
    }

    public class BlueprintReverseConnection
    {
        public BluePrintPin reverseConnectStart;
        public BluePrintPin reverseConnectEnd;

        public BlueprintReverseConnection(BluePrintPin start, BluePrintPin end)
        {
            reverseConnectStart = start;
            reverseConnectEnd = end;
        }
    }
}