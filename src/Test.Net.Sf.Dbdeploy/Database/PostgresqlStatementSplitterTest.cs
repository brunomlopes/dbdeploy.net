using System.Collections.Generic;

namespace Net.Sf.Dbdeploy.Database
{
    using NUnit.Framework;

    [TestFixture]
    public class PostgresqlStatementSplitterTest : QueryStatementSplitterTest
    {
        protected override QueryStatementSplitter CreateSplitter()
        {
            return new PostgresqlStatementSplitter();
        }

        [Test]
        public void ShouldNotSplitWhenInsideDollarQuotedString()
        {
            var statement = @"CREATE OR REPLACE FUNCTION to_day_at_tz(timestamptz, text)
RETURNS timestamptz AS
$$
SELECT (DATE_TRUNC('day', $1 AT TIME ZONE $2) + INTERVAL '1 day') AT TIME ZONE $2;
$$
LANGUAGE SQL STABLE;";
            List<string> result = new List<string>(this.splitter.Split(statement));

            Assert.Contains(statement.TrimEnd(';'), result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ShouldNotSplitWhenInsideDollarQuotedStringWithTag()
        {
            var statement = @"CREATE OR REPLACE FUNCTION to_day_at_tz(timestamptz, text)
RETURNS timestamptz AS
$tag$
SELECT (DATE_TRUNC('day', $1 AT TIME ZONE $2) + INTERVAL '1 day') AT TIME ZONE $2;
$tag$
LANGUAGE SQL STABLE;";
            List<string> result = new List<string>(this.splitter.Split(statement));

            Assert.Contains(statement.TrimEnd(';'), result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ShouldNotSplitIfDollarQuotedStringNeverEnds()
        {
            // this will actually error out on postgres, but the splitter should not crash
            var statement = @"CREATE OR REPLACE FUNCTION to_day_at_tz(timestamptz, text)
RETURNS timestamptz AS
$tag$
SELECT (DATE_TRUNC('day', $1 AT TIME ZONE $2) + INTERVAL '1 day') AT TIME ZONE $2;
LANGUAGE SQL STABLE;";
            List<string> result = new List<string>(this.splitter.Split(statement));

            Assert.Contains(statement.TrimEnd(';'), result);
            Assert.AreEqual(1, result.Count);
        }       
        
        [Test]
        public void ShouldNotSplitWhenTagIsInSameLine()
        {
            var statement = @"CREATE OR REPLACE FUNCTION to_day_at_tz(timestamptz, text)
RETURNS timestamptz AS
$tag$ SELECT (DATE_TRUNC('day', $1 AT TIME ZONE $2) + INTERVAL '1 day') AT TIME ZONE $2; $tag$
LANGUAGE SQL STABLE;";
            List<string> result = new List<string>(this.splitter.Split(statement));

            Assert.Contains(statement.TrimEnd(';'), result);
            Assert.AreEqual(1, result.Count);
        }
              
        
        [Test]
        [Ignore("For this quick release I'll 'ignore' this.")]
        public void ShouldNotSplitWhenTagIsInSameLineAndMultipleStatements()
        {
            var statement = @"CREATE OR REPLACE FUNCTION to_day_at_tz(timestamptz, text)
RETURNS timestamptz AS $tag$ SELECT (DATE_TRUNC('day', $1 AT TIME ZONE $2) + INTERVAL '1 day') AT TIME ZONE $2; $tag$ LANGUAGE SQL STABLE;
CREATE OR REPLACE FUNCTION to_day_at_tz(timestamptz, text)
RETURNS timestamptz AS $tag$ SELECT (DATE_TRUNC('day', $1 AT TIME ZONE $2) + INTERVAL '1 day') AT TIME ZONE $2; $tag$ LANGUAGE SQL STABLE;";
            List<string> result = new List<string>(this.splitter.Split(statement));

            Assert.Contains(statement.TrimEnd(';'), result);
            Assert.AreEqual(2, result.Count);
        }
        
        [Test]
        public void ShouldSplitOutsideOfDollarQuotedString()
        {
            var statement = @"CREATE OR REPLACE FUNCTION to_day_at_tz(timestamptz, text)
RETURNS timestamptz AS
$tag$
SELECT (DATE_TRUNC('day', $1 AT TIME ZONE $2) + INTERVAL '1 day') AT TIME ZONE $2;
$tag$
LANGUAGE SQL STABLE;
CREATE OR REPLACE FUNCTION to_day_at_tz(timestamptz, text)
RETURNS timestamptz AS
$tag$
SELECT (DATE_TRUNC('day', $1 AT TIME ZONE $2) + INTERVAL '1 day') AT TIME ZONE $2;
$tag$
LANGUAGE SQL STABLE;
";
            var result = new List<string>(this.splitter.Split(statement));

            Assert.Contains(@"CREATE OR REPLACE FUNCTION to_day_at_tz(timestamptz, text)
RETURNS timestamptz AS
$tag$
SELECT (DATE_TRUNC('day', $1 AT TIME ZONE $2) + INTERVAL '1 day') AT TIME ZONE $2;
$tag$
LANGUAGE SQL STABLE", result);
            Assert.AreEqual(2, result.Count);
        }
    }
}