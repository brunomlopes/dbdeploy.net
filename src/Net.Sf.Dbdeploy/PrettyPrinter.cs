using System;
using System.Collections.Generic;
using System.Text;

namespace Net.Sf.Dbdeploy
{
    using System.Linq;

    public class PrettyPrinter
    {
        public string Format(IEnumerable<UniqueChange> changes)
        {
            var changeList = changes.ToList();
            if (!changeList.Any())
            {
                return "(none)";
            }

            StringBuilder builder = new StringBuilder();

            int? lastRangeStart = null;
            int? lastNumber;

            var changesByFolder = changeList.GroupBy(c => c.Folder).ToList();
            bool isFirst;
            foreach (var group in changesByFolder)
            {
                AppendFolder(builder, group.Key);

                isFirst = true;
                lastNumber = null;
                foreach (var thisNumber in group.Select(c => c.ScriptNumber))
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
                        AppendRange(builder, lastRangeStart.Value, lastNumber.Value, isFirst);
                        isFirst = false;
                        lastNumber = thisNumber;
                        lastRangeStart = thisNumber;
                    }
                }

                this.AppendRange(builder, lastRangeStart.Value, lastNumber.Value, isFirst);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Appends the specified scripts folder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="folder">The last folder.</param>
        private static void AppendFolder(StringBuilder builder, string folder)
        {
            if (builder.Length > 0)
            {
                builder.AppendLine();
            }

            builder.AppendFormat("{0}: ", folder);
        }

        private void AppendRange(StringBuilder builder, int lastRangeStart, int lastNumber, bool isFirst)
        {
            if (lastRangeStart == lastNumber)
            {
                this.AppendWithPossibleComma(builder, lastNumber, isFirst);
            }
            else if (lastRangeStart + 1 == lastNumber)
            {
                this.AppendWithPossibleComma(builder, lastRangeStart, isFirst);
                this.AppendWithPossibleComma(builder, lastNumber, false);
            }
            else
            {
                this.AppendWithPossibleComma(builder, lastRangeStart + ".." + lastNumber, isFirst);
            }
        }

        private void AppendWithPossibleComma(StringBuilder builder, Object o, bool isFirst)
        {
            if (!isFirst)
            {
                builder.Append(", ");
            }

            builder.Append(o);
        }
    }
}