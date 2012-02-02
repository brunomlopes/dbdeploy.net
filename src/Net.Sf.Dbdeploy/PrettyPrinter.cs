using System;
using System.Collections.Generic;
using System.Text;
using Net.Sf.Dbdeploy.Scripts;

namespace Net.Sf.Dbdeploy
{
    public class PrettyPrinter
    {
        public String Format(ICollection<int> appliedChanges)
        {
            if (appliedChanges.Count == 0)
            {
                return "(none)";
            }

            StringBuilder builder = new StringBuilder();

            int? lastRangeStart = null;
            int? lastNumber = null;

            foreach (int thisNumber in appliedChanges)
            {
                if (!lastNumber.HasValue)
                {
                    // first in loop
                    lastNumber = thisNumber;
                    lastRangeStart = thisNumber;
                }
                else if (thisNumber == lastNumber + 1)
                {
                    // continuation of current range
                    lastNumber = thisNumber;
                }
                else
                {
                    // doesn't fit into last range - so output the old range and
                    // start a new one
                    AppendRange(builder, lastRangeStart.Value, lastNumber.Value);
                    lastNumber = thisNumber;
                    lastRangeStart = thisNumber;
                }
            }

            AppendRange(builder, lastRangeStart.Value, lastNumber.Value);
            return builder.ToString();
        }

        private void AppendRange(StringBuilder builder, int lastRangeStart, int lastNumber)
        {
            if (lastRangeStart == lastNumber)
            {
                AppendWithPossibleComma(builder, lastNumber);
            }
            else if (lastRangeStart + 1 == lastNumber)
            {
                AppendWithPossibleComma(builder, lastRangeStart);
                AppendWithPossibleComma(builder, lastNumber);
            }
            else
            {
                AppendWithPossibleComma(builder, lastRangeStart + ".." + lastNumber);
            }
        }

        private void AppendWithPossibleComma(StringBuilder builder, Object o)
        {
            if (builder.Length != 0)
            {
                builder.Append(", ");
            }
            builder.Append(o);
        }

        public String FormatChangeScriptList(ICollection<ChangeScript> changeScripts)
        {
            List<int> numberList = new List<int>(changeScripts.Count);

            foreach (ChangeScript changeScript in changeScripts)
            {
                numberList.Add(changeScript.GetId());
            }

            return Format(numberList);
        }
    }
}